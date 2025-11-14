using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Models.Requests.Notification;
using MSP.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using MSP.Domain.Entities;

namespace MSP.Application.Services.Implementations.ProjectTask
{
    /// <summary>
    /// Service to send deadline reminder notifications for tasks using Hangfire
    /// </summary>
    public class TaskReminderCronJobService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TaskReminderCronJobService> _logger;

        public TaskReminderCronJobService(
            IProjectTaskRepository projectTaskRepository,
            IProjectRepository projectRepository,
            INotificationService notificationService,
            UserManager<User> userManager,
            ILogger<TaskReminderCronJobService> logger)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Send deadline reminder notifications for tasks that are due in 1-2 days
        /// This method will be called by Hangfire Recurring Job
        /// </summary>
        public async Task SendDeadlineRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting to check for tasks with upcoming deadlines at {Time}", DateTime.UtcNow);
                
                var now = DateTime.UtcNow;
                var oneDayLater = now.AddDays(1);
                var twoDaysLater = now.AddDays(2);

                // Get tasks that are due in 1-2 days and not completed/overdue
                var upcomingTasks = await _projectTaskRepository.GetTasksWithUpcomingDeadlinesAsync(
                    oneDayLater,
                    twoDaysLater,
                    new[] { TaskEnum.Completed.ToString(), TaskEnum.OverDue.ToString() });

                if (upcomingTasks.Any())
                {
                    _logger.LogInformation("Found {Count} tasks with upcoming deadlines", upcomingTasks.Count());

                    foreach (var task in upcomingTasks)
                    {
                        // Only send reminder if task is assigned to a user
                        if (task.UserId.HasValue)
                        {
                            var user = await _userManager.FindByIdAsync(task.UserId.Value.ToString());
                            if (user == null)
                            {
                                _logger.LogWarning("User {UserId} not found for task {TaskId}", task.UserId, task.Id);
                                continue;
                            }

                            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
                            if (project == null || project.IsDeleted)
                            {
                                _logger.LogWarning("Project {ProjectId} not found or deleted for task {TaskId}", task.ProjectId, task.Id);
                                continue;
                            }

                            // Calculate days remaining
                            var daysRemaining = (task.EndDate!.Value.Date - now.Date).Days;
                            
                            var notificationRequest = new CreateNotificationRequest
                            {
                                UserId = task.UserId.Value,
                                Title = "Task Deadline Reminder",
                                Message = $"Task '{task.Title}' is due in {daysRemaining} day{(daysRemaining > 1 ? "s" : "")} ({task.EndDate:yyyy-MM-dd})",
                                Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                EntityId = task.Id.ToString(),
                                Data = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    TaskId = task.Id,
                                    TaskTitle = task.Title,
                                    ProjectId = project.Id,
                                    ProjectName = project.Name,
                                    DueDate = task.EndDate,
                                    DaysRemaining = daysRemaining,
                                    NotificationType = "DeadlineReminder"
                                })
                            };

                            await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                            // Send email notification
                            _notificationService.SendEmailNotification(
                                user.Email!,
                                "Task Deadline Reminder",
                                $"Hi {user.FullName},<br/><br/>" +
                                $"This is a reminder that your task <strong>{task.Title}</strong> is due in {daysRemaining} day{(daysRemaining > 1 ? "s" : "")}.<br/><br/>" +
                                $"<strong>Project:</strong> {project.Name}<br/>" +
                                $"<strong>Due Date:</strong> {task.EndDate:yyyy-MM-dd}<br/>" +
                                $"<strong>Status:</strong> {task.Status}<br/><br/>" +
                                $"Please ensure you complete this task on time.");

                            _logger.LogInformation(
                                "Sent deadline reminder for task {TaskId} ('{TaskTitle}') to user {UserId}. Due in {Days} days",
                                task.Id,
                                task.Title,
                                task.UserId,
                                daysRemaining);
                        }
                    }

                    _logger.LogInformation("Successfully sent {Count} deadline reminders", upcomingTasks.Count());
                }
                else
                {
                    _logger.LogInformation("No tasks with upcoming deadlines found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending deadline reminders");
                throw; // Rethrow for Hangfire to retry if needed
            }
        }
    }
}
