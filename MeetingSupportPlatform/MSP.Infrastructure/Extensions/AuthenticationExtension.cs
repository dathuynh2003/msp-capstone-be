using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MSP.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MSP.Infrastructure.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
        {

            var authBuilder = services.AddAuthentication(opt =>
            {
                //opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            // Add JWT
            var jwtOptions = config.GetSection(JwtOptions.JwtOptionsKey).Get<JwtOptions>()
                ?? throw new ArgumentException(nameof(JwtOptions));

            authBuilder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero // Loại bỏ clock skew để token hết hạn chính xác
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Ưu tiên Authorization header trước
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                        // Fallback về cookies
                        else
                        {
                            context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Add Google
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = config["Authentication:Google:ClientId"] ?? "";
                options.ClientSecret = config["Authentication:Google:ClientSecret"] ?? "";
            });

            return services;
        }
    }
}
