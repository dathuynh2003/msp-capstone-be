using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Meeting
{
    public class MeetingStatusCronJobService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly ILogger<MeetingStatusCronJobService> _logger;

        public MeetingStatusCronJobService(
            IMeetingRepository meetingRepository,
            ILogger<MeetingStatusCronJobService> logger)
        {
            _meetingRepository = meetingRepository;
            _logger = logger;
        }

        public async Task UpdateMeetingStatusesAsync()
        {
            try
            {
                _logger.LogInformation("Starting to update meeting statuses at {Time}", DateTime.UtcNow);
                
                var now = DateTime.UtcNow;
                int updatedCount = 0;

                // 1. Update meetings from Scheduled to Ongoing
                var scheduledMeetings = await _meetingRepository.GetScheduledMeetingsToStartAsync(
                    now, 
                    MeetingEnum.Scheduled.ToString());

                if (scheduledMeetings.Any())
                {
                    _logger.LogInformation("Found {Count} scheduled meetings to start", scheduledMeetings.Count());
                    
                    foreach (var meeting in scheduledMeetings)
                    {
                        meeting.Status = MeetingEnum.Ongoing.ToString();
                        meeting.UpdatedAt = now;
                        await _meetingRepository.UpdateAsync(meeting);
                        
                        _logger.LogInformation(
                            "Updated meeting {MeetingId} ('{Title}') from Scheduled to Ongoing. StartTime was {StartTime}",
                            meeting.Id,
                            meeting.Title,
                            meeting.StartTime);
                        
                        updatedCount++;
                    }
                    
                    await _meetingRepository.SaveChangesAsync();
                }

                // 2. Update meetings from Ongoing to Finished (over 1 hour and no EndTime)
                var ongoingMeetings = await _meetingRepository.GetOngoingMeetingsToCancelledAsync(
                    now,
                    MeetingEnum.Ongoing.ToString());

                if (ongoingMeetings.Any())
                {
                    _logger.LogInformation("Found {Count} ongoing meetings to finish", ongoingMeetings.Count());
                    
                    foreach (var meeting in ongoingMeetings)
                    {
                        meeting.Status = MeetingEnum.Cancelled.ToString();
                        meeting.EndTime = now;
                        meeting.UpdatedAt = now;
                        await _meetingRepository.UpdateAsync(meeting);
                        
                        _logger.LogInformation(
                            "Updated meeting {MeetingId} ('{Title}') from Ongoing to Finished. Started at {StartTime}, auto-finished at {EndTime}",
                            meeting.Id,
                            meeting.Title,
                            meeting.StartTime,
                            now);
                        
                        updatedCount++;
                    }
                    
                    await _meetingRepository.SaveChangesAsync();
                }

                if (updatedCount > 0)
                {
                    _logger.LogInformation("Successfully updated {Count} meetings", updatedCount);
                }
                else
                {
                    _logger.LogInformation("No meetings to update");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating meeting statuses");
                throw; // Rethrow ?? Hangfire có th? retry n?u c?n
            }
        }
    }
}
