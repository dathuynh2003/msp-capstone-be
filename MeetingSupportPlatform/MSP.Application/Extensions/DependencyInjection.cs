using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSP.Application.Services.Interfaces.Auth;
using MSP.Application.Services.Implementations.Auth;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Implementations.Meeting;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Implementations.Summarize;
using MSP.Application.Services.Interfaces.Summarize;
using MSP.Application.Services.Interfaces.Project;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Application.Services.Implementations.Milestone;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Application.Services.Implementations.ProjectTask;
using MSP.Application.Services.Interfaces.Users;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Application.Services.Implementations.Todos;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Application.Services.Implementations.OrganizationInvitation;
using MSP.Application.Services.Interfaces.TaskReassignRequest;
using MSP.Application.Services.Implementations.TaskReassignRequest;
using MSP.Application.Services.Implementations.Cleanup;
using MSP.Application.Services.Interfaces.Document;
using MSP.Application.Services.Implementations.Document;

namespace MSP.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Business Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<INotificationService, MSP.Application.Services.Implementations.Notification.NotificationService>();
            services.AddScoped<ISummarizeTextService, SummarizeTextService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IProjectTaskService, ProjectTaskService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrganizationInvitationService, OrganizationInvitationService>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<ITaskReassignRequestService, TaskReassignRequestService>();
            services.AddScoped<IDocumentService, DocumentService>();
            
            // Register Hangfire Cron Job Services
            services.AddScoped<TaskStatusCronJobService>();
            services.AddScoped<MeetingStatusCronJobService>();
            services.AddScoped<ProjectStatusCronJobService>();
            services.AddScoped<CleanupExpiredTokensCronJobService>();
            services.AddScoped<CleanupPendingInvitationsCronJobService>();
            services.AddScoped<TaskReminderCronJobService>();

            // Register StreamSettings from appsettings.json
            services.Configure<StreamSettings>(
                configuration.GetSection("Stream"));

            // Register HttpClientFactory
            services.AddHttpClient();

            // Register StreamService with DI
            services.AddScoped<IStreamService, StreamService>();

            return services;
        }
    }
}
