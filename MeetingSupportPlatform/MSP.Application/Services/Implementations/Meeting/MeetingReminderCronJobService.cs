using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Meeting
{
    /// <summary>
    /// Service to send meeting reminder notifications 1 hour before meeting starts using Hangfire
    /// </summary>
    public class MeetingReminderCronJobService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<MeetingReminderCronJobService> _logger;

        public MeetingReminderCronJobService(
            IMeetingRepository meetingRepository,
            INotificationService notificationService,
            UserManager<User> userManager,
            ILogger<MeetingReminderCronJobService> logger)
        {
            _meetingRepository = meetingRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Send reminder notifications for meetings starting in 1 hour
        /// This method will be called by Hangfire Recurring Job every 10-15 minutes
        /// </summary>
        public async Task SendMeetingRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting to check for upcoming meetings at {Time}", DateTime.UtcNow);

                var now = DateTime.UtcNow;
                var reminderWindow = now.AddHours(1);
                int notificationsSent = 0;

                // Get scheduled meetings that will start in approximately 1 hour
                // Window: between 50 minutes and 70 minutes from now (to handle job frequency)
                var upcomingMeetings = await _meetingRepository.GetUpcomingMeetingsForReminderAsync(
                    now.AddMinutes(50),
                    now.AddMinutes(70),
                    MeetingEnum.Scheduled.ToString());

                if (upcomingMeetings.Any())
                {
                    _logger.LogInformation("Found {Count} meetings starting in approximately 1 hour", upcomingMeetings.Count());

                    foreach (var meeting in upcomingMeetings)
                    {
                        try
                        {
                            var minutesUntilStart = (int)(meeting.StartTime - now).TotalMinutes;
                            
                            _logger.LogInformation(
                                "Processing reminder for meeting {MeetingId} ('{Title}'). Starts in {Minutes} minutes at {StartTime}",
                                meeting.Id,
                                meeting.Title,
                                minutesUntilStart,
                                meeting.StartTime);

                            // Get all attendees for this meeting
                            var attendees = meeting.Attendees?.ToList() ?? new List<User>();
                            
                            // Add meeting creator if not already in attendees
                            if (meeting.CreatedBy != null && !attendees.Any(a => a.Id == meeting.CreatedById))
                            {
                                attendees.Add(meeting.CreatedBy);
                            }

                            if (!attendees.Any())
                            {
                                _logger.LogWarning("No attendees found for meeting {MeetingId}", meeting.Id);
                                continue;
                            }

                            _logger.LogInformation(
                                "Sending reminder to {Count} attendee(s) for meeting {MeetingId}",
                                attendees.Count,
                                meeting.Id);

                            // Send notification to each attendee
                            foreach (var attendee in attendees)
                            {
                                try
                                {
                                    // Send in-app notification
                                    var notificationRequest = new CreateNotificationRequest
                                    {
                                        UserId = attendee.Id,
                                        Title = "Meeting Reminder",
                                        Message = $"Meeting '{meeting.Title}' will start in approximately {minutesUntilStart} minutes at {meeting.StartTime:HH:mm}.",
                                        Type = NotificationTypeEnum.MeetingReminder.ToString(),
                                        EntityId = meeting.Id.ToString(),
                                        Data = System.Text.Json.JsonSerializer.Serialize(new
                                        {
                                            MeetingId = meeting.Id,
                                            MeetingTitle = meeting.Title,
                                            StartTime = meeting.StartTime,
                                            ProjectId = meeting.ProjectId,
                                            ProjectName = meeting.Project?.Name,
                                            MinutesUntilStart = minutesUntilStart,
                                            EventType = "MeetingReminder"
                                        })
                                    };

                                    await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                                    // Send email notification
                                    if (!string.IsNullOrEmpty(attendee.Email))
                                    {
                                        _notificationService.SendEmailNotification(
                                            attendee.Email,
                                            "Meeting Reminder",
                                            $"Hello {attendee.FullName},<br/><br/>" +
                                            $"This is a reminder that you have an upcoming meeting:<br/><br/>" +
                                            $"<strong>Meeting:</strong> {meeting.Title}<br/>" +
                                            $"<strong>Start Time:</strong> {meeting.StartTime:dd/MM/yyyy HH:mm}<br/>" +
                                            $"<strong>Project:</strong> {meeting.Project?.Name ?? "N/A"}<br/>" +
                                            (string.IsNullOrEmpty(meeting.Description) ? "" : $"<strong>Description:</strong> {meeting.Description}<br/>") +
                                            $"<br/>" +
                                            $"The meeting will start in approximately <strong>{minutesUntilStart} minutes</strong>.<br/><br/>" +
                                            $"Please be prepared and join on time.");
                                    }

                                    notificationsSent++;

                                    _logger.LogInformation(
                                        "Sent meeting reminder to user {UserId} ({Email}) for meeting {MeetingId}",
                                        attendee.Id,
                                        attendee.Email,
                                        meeting.Id);
                                }
                                catch (Exception innerEx)
                                {
                                    _logger.LogError(innerEx,
                                        "Failed to send meeting reminder to user {UserId} for meeting {MeetingId}",
                                        attendee.Id,
                                        meeting.Id);
                                    // Continue with other attendees
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed to process meeting reminder for meeting {MeetingId}",
                                meeting.Id);
                            // Continue processing other meetings
                        }
                    }

                    _logger.LogInformation("Successfully sent {Count} meeting reminder notifications", notificationsSent);
                }
                else
                {
                    _logger.LogInformation("No meetings starting in the next hour");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending meeting reminders");
                throw; // Rethrow for Hangfire to retry if needed
            }
        }
    }
}
