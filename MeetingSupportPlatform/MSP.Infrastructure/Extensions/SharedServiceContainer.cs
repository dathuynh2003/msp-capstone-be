using MSP.Shared.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MSP.Infrastructure.Extensions
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext
        {
            //Add Generic Database context
            services.AddDbContext<TContext>(option => option.UseNpgsql(config.GetConnectionString("DbConnectionString"), sqlserverOption => sqlserverOption.EnableRetryOnFailure()));

            //configure serilog logging
            Log.Logger = (ILogger)new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //Add  JWT Authentication Scheme
            //services.AddAppAuthentication(config);
            return services;
        }


        public static IApplicationBuilder UserSharedPolicies(this IApplicationBuilder app)
        {
            //Use global Exception
            app.UseMiddleware<GlobalException>();
            //Register middleware to block all outsides API calls
            //app.UseMiddleware<ListenToOnlyApiGateway>();
            return app;
        }
    }
}
