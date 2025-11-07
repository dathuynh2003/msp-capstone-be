using Hangfire;
using Microsoft.AspNetCore.Builder;
using MSP.Application.Services.Implementations.ProjectTask;

namespace MSP.Application.Extensions
{
    /// <summary>
    /// Extension methods ?? c?u hình Hangfire Recurring Jobs
    /// </summary>
    public static class HangfireJobConfiguration
    {
        /// <summary>
        /// C?u hình t?t c? Hangfire Recurring Jobs cho ?ng d?ng
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <returns>IApplicationBuilder ?? chain ti?p</returns>
        public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
        {
            // Configure Task Status Cron Job
            // T? ??ng ki?m tra và c?p nh?t task quá h?n thành OverDue
            RecurringJob.AddOrUpdate<TaskStatusCronJobService>(
                "update-overdue-tasks",
                service => service.UpdateOverdueTasksAsync(),
                "*/5 * * * *", // Ch?y m?i 5 phút
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });

            // TODO: Thêm các recurring jobs khác ? ?ây n?u c?n
            // Example:
            // RecurringJob.AddOrUpdate<AnotherService>(
            //     "another-job",
            //     service => service.DoSomethingAsync(),
            //     Cron.Daily, // Ho?c custom cron expression
            //     new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            return app;
        }

        /// <summary>
        /// C?u hình Recurring Jobs v?i custom options
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <param name="cronExpression">Cron expression cho task status job</param>
        /// <returns>IApplicationBuilder ?? chain ti?p</returns>
        public static IApplicationBuilder UseHangfireJobs(
            this IApplicationBuilder app, 
            string taskStatusCronExpression = "*/5 * * * *")
        {
            RecurringJob.AddOrUpdate<TaskStatusCronJobService>(
                "update-overdue-tasks",
                service => service.UpdateOverdueTasksAsync(),
                taskStatusCronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });

            return app;
        }
    }
}
