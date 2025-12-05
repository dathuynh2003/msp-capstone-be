//using AuthService.Application.Extensions;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MSP.Application.Extensions;
using MSP.Infrastructure.Extensions;
using MSP.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Services Registration
builder.Services.AddControllers();
builder.Services.AddInfrastuctureService(builder.Configuration);
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddAuthorization();

// Add Memory Cache for Rate Limiting
builder.Services.AddMemoryCache();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MSP API",
        Version = "v1"
    });

    // Add JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// HTTP Client
builder.Services.AddHttpClient();

// Add Hangfire
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DbConnectionString"),
        new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire",
            DistributedLockTimeout = TimeSpan.FromMinutes(5) // tăng timeout để tránh lỗi
        });
});

builder.Services.AddHangfireServer();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
builder.Services.AddSignalRNotificationService<NotificationHub>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "https://msp-capstone-fe.vercel.app",
                "https://msp.audivia.vn"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Important for SignalR
        });
});

// Build & Configure Pipeline
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Add routing
app.UseRouting();

app.UseInfrastructurePolicy();
app.Use(async (context, next) =>
{
    Console.WriteLine($"[TRACE] Request {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[TRACE] Response {context.Response.StatusCode} for {context.Request.Path}");
});
app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();

// Configure Hangfire Recurring Jobs
app.UseHangfireJobs();

// Map SignalR Hub (Authorization handled in Hub class)
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();
app.Run();
