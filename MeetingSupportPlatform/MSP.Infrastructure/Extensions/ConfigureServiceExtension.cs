using MSP.Infrastructure.Processors;
using MSP.Application.Abstracts;
using AuthService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Implementations;
using MSP.Infrastructure.Options;

namespace MSP.Infrastructure.Extensions
{
    public static class ConfigureServiceExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // Configure Email Settings
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));

            services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // Register Services
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }
    }
}
