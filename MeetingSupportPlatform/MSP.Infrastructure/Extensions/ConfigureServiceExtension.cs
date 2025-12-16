using AuthService.Infrastructure.Repositories;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Auth;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Payment;
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
            // Configure PayOS Settings
            services.Configure<PayOSConfiguration>(config.GetSection("PayOS"));
            // Configure Stream Settings
            services.Configure<StreamSettings>(config.GetSection("Stream"));

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
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();    
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

            // Register External Services
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IPaymentService, PayOSService>();
            services.AddScoped<IStreamService, StreamService>();    

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
            services.AddScoped<ISignalRNotificationService, SignalRNotificationService<THub>>();
            return services;
        }
    }
}
