using MSP.Infrastructure.Processors;
using MSP.Application.Abstracts;
using AuthService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Implementations;
using MSP.Infrastructure.Options;
using MSP.Infrastructure.Repositories;
using MSP.Application.Repositories;

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
            services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
            // Register Services
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }
    }
}
