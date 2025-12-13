using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSP.Application.Services.Implementations.Auth;
using MSP.Application.Services.Implementations.Limitation;
using MSP.Application.Services.Implementations.Milestone;
using MSP.Application.Services.Implementations.OrganizationInvitation;
using MSP.Application.Services.Implementations.Cleanup;
using MSP.Application.Services.Interfaces.Document;
using MSP.Application.Services.Implementations.Document;
using MSP.Application.Services.Implementations.Package;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Implementations.ProjectTask;
using MSP.Application.Services.Implementations.SubscriptionService;
using MSP.Application.Services.Implementations.Summarize;
using MSP.Application.Services.Implementations.TaskHistory;
using MSP.Application.Services.Implementations.Todos;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Implementations.Comment;
using MSP.Application.Services.Interfaces.Auth;
using MSP.Application.Services.Interfaces.Limitation;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Application.Services.Interfaces.Package;
using MSP.Application.Services.Interfaces.Payment;
using MSP.Application.Services.Interfaces.Project;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Application.Services.Interfaces.Summarize;
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Application.Services.Interfaces.Users;
using MSP.Application.Services.Interfaces.Comment;
using MSP.Application.Services.Interfaces.Subscription;
using MSP.Application.Services.Implementations.Meeting;

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
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ITaskHistoryService, TaskHistoryService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ILimitationService, LimitationService>();
            services.AddScoped<ICommentService, CommentService>();
            
            // Register Hangfire Job Services
            services.AddScoped<TaskStatusCronJobService>();
            services.AddScoped<MeetingStatusCronJobService>();
            services.AddScoped<MeetingReminderCronJobService>();
            services.AddScoped<ProjectStatusCronJobService>();
            services.AddScoped<CleanupExpiredTokensCronJobService>();
            services.AddScoped<CleanupPendingInvitationsCronJobService>();
            services.AddScoped<TaskReminderCronJobService>();
            services.AddScoped<ProjectCompletionReminderCronJobService>();


            //Configure PayOS Settings


            // Register HttpClientFactory
            services.AddHttpClient();


            return services;
        }
    }
}
