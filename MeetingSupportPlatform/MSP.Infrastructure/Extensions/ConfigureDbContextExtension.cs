using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSP.Infrastructure.Options;

namespace MSP.Infrastructure.Extensions
{
    public static class ConfigureDbContextExtension
    {
        public static IServiceCollection AddCustomDBContext(this IServiceCollection services, IConfiguration config)
        {
            // Add database connectivity
            // JWT Add Authentication Scheme
            SharedServiceContainer.AddSharedServices<ApplicationDbContext>(services, config, config["MySerilog:FileName"]!);

            services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 6;
                opt.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddDefaultTokenProviders();

            services.Configure<JwtOptions>(config.GetSection(JwtOptions.JwtOptionsKey));
            return services;
        }
    }
}
