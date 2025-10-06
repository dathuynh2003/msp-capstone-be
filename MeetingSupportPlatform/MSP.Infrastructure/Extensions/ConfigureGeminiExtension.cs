using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using MSP.Application.Services.Interfaces.Summarize;
using MSP.Infrastructure.Processors;

namespace MSP.Infrastructure.Extensions
{
    public static class ConfigureGeminiExtension
    {
        public static IServiceCollection AddGeminiService(this IServiceCollection services, IConfiguration config)
        {
            var apiKey = config["Gemini:ApiKey"];
            var modelName = config["Gemini:ModelName"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Error configuration");

            // HttpClient (chỉ bypass SSL khi cần)
            services.AddHttpClient("GeminiHttpClient")
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));

            // GenerativeModel
            services.AddSingleton<GenerativeModel>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var logger = sp.GetRequiredService<ILogger<GenerativeModel>>();

                return new GenerativeModel(httpClientFactory, logger)
                {
                    ApiKey = apiKey,
                    Model = modelName
                };
            });

            services.AddScoped<IGeminiTextSummarizer, GeminiTextSummarizer>();
            services.AddScoped<IGeminiVideoTextSummarizer, GeminiVideoTextSummarizer>();
            return services;
        }
    }
}
