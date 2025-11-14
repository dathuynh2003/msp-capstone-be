using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Models.Requests.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.ProjectTask
{
    /// <summary>
    /// Service to automatically check and update task status to OverDue using Hangfire
    /// </summary>
    public class TaskStatusCronJobService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TaskStatusCronJobService> _logger;

        public TaskStatusCronJobService(
            IProjectTaskRepository projectTaskRepository,
            IProjectRepository projectRepository,
            INotificationService notificationService,
            UserManager<User> userManager,
            ILogger<TaskStatusCronJobService> logger)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Check and update overdue tasks to OverDue status and send notifications
        /// This method will be called by Hangfire Recurring Job
        /// </summary>
        public async Task UpdateOverdueTasksAsync()
        {
            try
            {
                _logger.LogInformation("Starting to check for overdue tasks at {Time}", DateTime.UtcNow);
                
                var now = DateTime.UtcNow;
                
                // Query overdue tasks directly from database
                var tasksToUpdate = await _projectTaskRepository.GetOverdueTasksAsync(
                    now,
                    TaskEnum.OverDue.ToString(),
                    TaskEnum.Completed.ToString());

                if (tasksToUpdate.Any())
                {
                    _logger.LogInformation("Found {Count} tasks that are overdue", tasksToUpdate.Count());
                    
                    int notificationsSent = 0;
                    
                    foreach (var task in tasksToUpdate)
                    {
                        var oldStatus = task.Status;
                        task.Status = TaskEnum.OverDue.ToString();
                        task.UpdatedAt = now;
                        
                        await _projectTaskRepository.UpdateAsync(task);
                        
                        _logger.LogInformation(
                            "Updated task {TaskId} ('{TaskTitle}') from {OldStatus} to OverDue. EndDate was {EndDate}", 
                            task.Id, 
                            task.Title,
                            oldStatus,
                            task.EndDate);

                        // Send notification if task is assigned to a user
                        if (task.UserId.HasValue)
                        {
                            try
                            {
                                var user = await _userManager.FindByIdAsync(task.UserId.Value.ToString());
                                if (user != null)
                                {
                                    var project = await _projectRepository.GetByIdAsync(task.ProjectId);
                                    if (project != null && !project.IsDeleted)
                                    {
                                        // Calculate days overdue
                                        var daysOverdue = (now.Date - task.EndDate!.Value.Date).Days;

                                        var notificationRequest = new CreateNotificationRequest
                                        {
                                            UserId = task.UserId.Value,
                                            Title = "Task Overdue",
                                            Message = $"Task '{task.Title}' is now overdue by {daysOverdue} day{(daysOverdue > 1 ? "s" : "")}",
                                            Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                            EntityId = task.Id.ToString(),
                                            Data = System.Text.Json.JsonSerializer.Serialize(new
                                            {
                                                TaskId = task.Id,
                                                TaskTitle = task.Title,
                                                ProjectId = project.Id,
                                                ProjectName = project.Name,
                                                DueDate = task.EndDate,
                                                DaysOverdue = daysOverdue,
                                                NotificationType = "TaskOverdue"
                                            })
                                        };

                                        await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                                        // Send email notification
                                        _notificationService.SendEmailNotification(
                                            user.Email!,
                                            "Task Overdue",
                                            $"Hi {user.FullName},<br/><br/>" +
                                            $"Your task <strong>{task.Title}</strong> is now overdue by {daysOverdue} day{(daysOverdue > 1 ? "s" : "")}.<br/><br/>" +
                                            $"<strong>Project:</strong> {project.Name}<br/>" +
                                            $"<strong>Due Date:</strong> {task.EndDate:yyyy-MM-dd}<br/>" +
                                            $"<strong>Status:</strong> {task.Status}<br/><br/>" +
                                            $"Please complete this task as soon as possible.");

                                        notificationsSent++;

                                        _logger.LogInformation(
                                            "Sent overdue notification for task {TaskId} to user {UserId}. Overdue by {Days} days",
                                            task.Id,
                                            task.UserId,
                                            daysOverdue);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, 
                                    "Failed to send overdue notification for task {TaskId} to user {UserId}",
                                    task.Id,
                                    task.UserId);
                                // Continue processing other tasks even if notification fails
                            }
                        }
                    }
                    
                    await _projectTaskRepository.SaveChangesAsync();
                    _logger.LogInformation(
                        "Successfully updated {Count} overdue tasks and sent {NotificationCount} notifications", 
                        tasksToUpdate.Count(),
                        notificationsSent);
                }
                else
                {
                    _logger.LogInformation("No overdue tasks found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating overdue tasks");
                throw; // Rethrow for Hangfire to retry if needed
            }
        }
    }
}
