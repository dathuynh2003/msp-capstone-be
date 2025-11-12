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
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Application.Services.Implementations.TaskHistory;

namespace MSP.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            // Create DI
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
            services.AddScoped<ITaskHistoryService, TaskHistoryService>();
            
            // Register Background Services (Cron Jobs) - chỉ giữ MeetingCronJobService
            services.AddHostedService<MeetingCronJobService>();
            
            // Register Hangfire Job Services
            services.AddScoped<TaskStatusCronJobService>();


            // Đăng ký StreamSettings từ appsettings.json
            services.Configure<StreamSettings>(
                configuration.GetSection("Stream"));

            // Đăng ký HttpClientFactory
            services.AddHttpClient();

            // Đăng ký StreamService với DI
            services.AddScoped<IStreamService, StreamService>();
            return services;
        }
    }
}
