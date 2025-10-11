using MSP.Infrastructure.Processors;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MSP.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastuctureService(this IServiceCollection services, IConfiguration config)
        {
            services.AddCustomDBContext(config);
            services.AddServices(config);
            services.AddGeminiService(config);

            return services;
        }

        public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
        {
            // Register middleware such as:
            // Global Exception Handler
            // ListenToOnlyApiGateway : blocks all outside API calls except the API Gateway

            SharedServiceContainer.UserSharedPolicies(app);
            return app;
        }


    }
}
