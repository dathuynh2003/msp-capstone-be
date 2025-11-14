using AuthService.Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Infrastructure.Options;
using MSP.Infrastructure.Processors;
using MSP.Infrastructure.Repositories;
using MSP.Infrastructure.Services;
using NotificationService.Infrastructure.Implementations;
using NotificationService.Infrastructure.Repositories;

namespace MSP.Infrastructure.Extensions
{
    public static class ConfigureServiceExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Configure Email Settings
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));


            // Register Processors
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
            services.AddScoped<IOrganizationInviteRepository, OrganizationInvitationRepository>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();  
            services.AddScoped<ILimitationRepository, LimitationRepository>();
            
            // Register Services
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }

        /// <summary>
        /// Register SignalR Notification Service with Hub type
        /// This must be called AFTER SignalR is added (AddSignalR)
        /// Usage: services.AddSignalRNotificationService<NotificationHub>();
        /// </summary>
        public static IServiceCollection AddSignalRNotificationService<THub>(this IServiceCollection services)
            where THub : Hub
        {
            services.AddScoped<ISignalRNotificationService>(provider =>
            {
                var hubContext = provider.GetRequiredService<IHubContext<THub>>();
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SignalRNotificationService>>();
                return new SignalRNotificationService(hubContext, logger);
            });

            return services;
        }
    }
}
