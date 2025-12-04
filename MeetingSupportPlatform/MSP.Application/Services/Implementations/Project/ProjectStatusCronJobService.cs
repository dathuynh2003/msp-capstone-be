using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Project;
using MSP.Domain.Entities;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Project
{
    /// <summary>
    /// Service to automatically update project statuses using Hangfire
    /// - Scheduled → InProgress when StartDate is reached
    /// - Send notifications to PM (Owner) only when project is nearing deadline
    /// </summary>
    public class ProjectStatusCronJobService
    {
    private readonly IProjectRepository _projectRepository;
    private readonly INotificationService _notificationService;
    private readonly IProjectService _projectService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ProjectStatusCronJobService> _logger;

        public ProjectStatusCronJobService(
            IProjectRepository projectRepository,
            INotificationService notificationService,
            IProjectService projectService,
            UserManager<User> userManager,
            ILogger<ProjectStatusCronJobService> logger)
        {
            _projectRepository = projectRepository;
            _notificationService = notificationService;
            _projectService = projectService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Update project statuses and send deadline warnings
        /// This method will be called by Hangfire Recurring Job
        /// </summary>
        public async Task UpdateProjectStatusesAsync()
        {
            try
            {
                _logger.LogInformation("Starting to update project statuses at {Time}", DateTime.UtcNow);

                var now = DateTime.UtcNow;
                int updatedCount = 0;
                int notificationsSent = 0;

                // 1. Update projects from NotStarted to InProgress
                var notStartedProjects = await _projectRepository.GetNotStartedProjectsProjectsToStartAsync(
                    now,
                    ProjectStatusEnum.NotStarted.ToString());

                if (notStartedProjects.Any())
                {
                    _logger.LogInformation("Found {Count} not started projects to start", notStartedProjects.Count());

                    foreach (var project in notStartedProjects)
                    {
                        project.Status = ProjectStatusEnum.InProgress.ToString();
                        project.UpdatedAt = now;
                        await _projectRepository.UpdateAsync(project);

                        _logger.LogInformation(
                            "Updated project {ProjectId} ('{Name}') from NotStarted to InProgress. StartDate was {StartDate}",
                            project.Id,
                            project.Name,
                            project.StartDate);

                        updatedCount++;
                    }

                    await _projectRepository.SaveChangesAsync();
                }

                // 2. Send alerts for projects nearing deadline (7 days before EndDate)
                var projectsNearingDeadline = await _projectRepository.GetProjectsNearingDeadlineAsync(
                    now,
                    now.AddDays(7),
                    ProjectStatusEnum.InProgress.ToString());

                if (projectsNearingDeadline.Any())
                {
                    _logger.LogWarning("Found {Count} projects nearing deadline (within 7 days)", projectsNearingDeadline.Count());

                    foreach (var project in projectsNearingDeadline)
                    {
                        var daysRemaining = (project.EndDate!.Value - now).Days;
                        var deadlineText = daysRemaining == 0 
                            ? "today"
                            : $"in {daysRemaining} days";

                        _logger.LogWarning(
                            "Project {ProjectId} ('{Name}') is nearing deadline. EndDate: {EndDate} ({DeadlineText})",
                            project.Id,
                            project.Name,
                            project.EndDate,
                            deadlineText);

                        // Use ProjectService to get Project Managers (more reliable)
                        var projectManagers = new List<Guid>();
                        try
                        {
                            var pmResponse = await _projectService.GetProjectManagersAsync(project.Id);
                            if (pmResponse == null || !pmResponse.Success || pmResponse.Data == null || !pmResponse.Data.Any())
                            {
                                _logger.LogWarning(
                                    "No Project Managers found for project {ProjectId} via ProjectService. Skipping notification.",
                                    project.Id);
                                continue;
                            }

                            projectManagers = pmResponse.Data.Select(pm => pm.UserId).ToList();

                            _logger.LogInformation(
                                "Found {Count} Project Manager(s) for project {ProjectId} via ProjectService",
                                projectManagers.Count,
                                project.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to retrieve Project Managers for project {ProjectId} via ProjectService", project.Id);
                            continue;
                        }

                        // Send notification to all Project Managers
                        foreach (var pmUserId in projectManagers)
                        {
                            try
                            {
                                await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
                                {
                                    UserId = pmUserId,
                                    Title = "Project Deadline Warning",
                                    Message = $"Project '{project.Name}' is due {deadlineText} (End date: {project.EndDate:dd/MM/yyyy}). Please review project progress.",
                                    Type = NotificationTypeEnum.InApp.ToString(),
                                    Data = $"{{\"eventType\":\"ProjectDeadlineWarning\",\"projectId\":\"{project.Id}\",\"projectName\":\"{project.Name}\",\"endDate\":\"{project.EndDate:dd/MM/yyyy}\",\"daysRemaining\":{daysRemaining}}}"
                                });

                                notificationsSent++;
                                
                                _logger.LogInformation(
                                    "Sent deadline notification to PM {UserId} for project {ProjectId}",
                                    pmUserId,
                                    project.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, 
                                    "Failed to send notification to PM {UserId} for project {ProjectId}",
                                    pmUserId,
                                    project.Id);
                            }
                        }
                    }
                }

                // Summary log
                if (updatedCount > 0 || notificationsSent > 0)
                {
                    _logger.LogInformation(
                        "Job completed: Updated {UpdatedCount} projects, sent {NotificationCount} deadline notifications",
                        updatedCount,
                        notificationsSent);
                }
                else
                {
                    _logger.LogInformation("No projects to update and no deadline warnings to send");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating project statuses");
                throw; // Rethrow for Hangfire to retry if needed
            }
        }
    }
}
