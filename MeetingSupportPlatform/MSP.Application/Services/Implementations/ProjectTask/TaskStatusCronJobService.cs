using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.ProjectTask
{
    /// <summary>
    /// Service ?? t? ??ng ki?m tra và c?p nh?t status c?a task thành OverDue s? d?ng Hangfire
    /// </summary>
    public class TaskStatusCronJobService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly ILogger<TaskStatusCronJobService> _logger;

        public TaskStatusCronJobService(
            IProjectTaskRepository projectTaskRepository,
            ILogger<TaskStatusCronJobService> logger)
        {
            _projectTaskRepository = projectTaskRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ki?m tra và c?p nh?t các task quá h?n thành OverDue
        /// Method này s? ???c g?i b?i Hangfire Recurring Job
        /// </summary>
        public async Task UpdateOverdueTasksAsync()
        {
            try
            {
                _logger.LogInformation("Starting to check for overdue tasks at {Time}", DateTime.UtcNow);
                
                var now = DateTime.UtcNow;
                
                // Query tr?c ti?p t? database các tasks quá h?n
                var tasksToUpdate = await _projectTaskRepository.GetOverdueTasksAsync(
                    now,
                    TaskEnum.OverDue.ToString(),
                    TaskEnum.Completed.ToString());

                if (tasksToUpdate.Any())
                {
                    _logger.LogInformation("Found {Count} tasks that are overdue", tasksToUpdate.Count());
                    
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
                    }
                    
                    await _projectTaskRepository.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated {Count} overdue tasks", tasksToUpdate.Count());
                }
                else
                {
                    _logger.LogInformation("No overdue tasks found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating overdue tasks");
                throw; // Rethrow ?? Hangfire có th? retry n?u c?n
            }
        }
    }
}
