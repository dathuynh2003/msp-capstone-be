using Hangfire;
using Microsoft.AspNetCore.Builder;
using MSP.Application.Services.Implementations.ProjectTask;
using MSP.Application.Services.Implementations.Meeting;
using MSP.Application.Services.Implementations.Cleanup;
using MSP.Application.Services.Implementations.Project;

namespace MSP.Application.Extensions
{
    /// <summary>
    /// Extension methods to configure Hangfire Recurring Jobs
    /// </summary>
    public static class HangfireJobConfiguration
    {
        /// <summary>
        /// Configure all Hangfire Recurring Jobs for the application
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <returns>IApplicationBuilder for method chaining</returns>
        public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
        {
            // Get Vietnam timezone (UTC+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // 1. Task Status Cron Job
            // Automatically check and update overdue tasks to OverDue status + send notifications
            RecurringJob.AddOrUpdate<TaskStatusCronJobService>(
                "update-overdue-tasks",
                service => service.UpdateOverdueTasksAsync(),
                Cron.Daily(), // Run every day at 12:00 AM Vietnam time
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            // 2. Meeting Status Cron Job
            // Automatically update meeting statuses
            // - Scheduled ? Ongoing when StartTime is reached
            // - Ongoing ? Finished when 1 hour has passed from StartTime
            RecurringJob.AddOrUpdate<MeetingStatusCronJobService>(
                "update-meeting-statuses",
                service => service.UpdateMeetingStatusesAsync(),
                "*/5 * * * *", // Run every 5 minutes
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            // 3. Project Status Cron Job
            // Automatically update project statuses
            // - Scheduled ? InProgress when StartDate is reached
            // - Send deadline warnings to Owner and Members (7 days before EndDate)
            RecurringJob.AddOrUpdate<ProjectStatusCronJobService>(
                "update-project-statuses",
                service => service.UpdateProjectStatusesAsync(),
                Cron.Daily(), // Run every day at 12:00 AM Vietnam time
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            // 4. Cleanup Expired Refresh Tokens
            // Automatically cleanup expired refresh tokens for security and reduce database bloat
            RecurringJob.AddOrUpdate<CleanupExpiredTokensCronJobService>(
                "cleanup-expired-tokens",
                service => service.CleanupExpiredTokensAsync(),
                Cron.Daily(), // Run daily at 12:00 AM Vietnam time
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            // 5. Cleanup Expired Pending Invitations
            // Automatically cancel/expire pending organization invitations after 7 days
            RecurringJob.AddOrUpdate<CleanupPendingInvitationsCronJobService>(
                "cleanup-pending-invitations",
                service => service.CleanupExpiredInvitationsAsync(),
                Cron.Daily(), // Run daily at 12:00 AM Vietnam time
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            // 6. Task Deadline Reminder
            // Send reminder notifications for tasks that are due in 1-2 days
            RecurringJob.AddOrUpdate<TaskReminderCronJobService>(
                "send-task-deadline-reminders",
                service => service.SendDeadlineRemindersAsync(),
                Cron.Daily(), // Run daily at 12:00 AM Vietnam time
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            return app;
        }

        /// <summary>
        /// Configure Recurring Jobs with custom options
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <param name="taskStatusCronExpression">Cron expression for task status job</param>
        /// <param name="meetingStatusCronExpression">Cron expression for meeting status job</param>
        /// <returns>IApplicationBuilder for method chaining</returns>
        public static IApplicationBuilder UseHangfireJobs(
            this IApplicationBuilder app, 
            string meetingStatusCronExpression = "*/5 * * * *")
        {
            // Get Vietnam timezone (UTC+7)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            RecurringJob.AddOrUpdate<TaskStatusCronJobService>(
                "update-overdue-tasks",
                service => service.UpdateOverdueTasksAsync(),
                Cron.Daily(),
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            RecurringJob.AddOrUpdate<MeetingStatusCronJobService>(
                "update-meeting-statuses",
                service => service.UpdateMeetingStatusesAsync(),
                meetingStatusCronExpression,
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            RecurringJob.AddOrUpdate<ProjectStatusCronJobService>(
                "update-project-statuses",
                service => service.UpdateProjectStatusesAsync(),
                Cron.Daily(),
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            RecurringJob.AddOrUpdate<CleanupExpiredTokensCronJobService>(
                "cleanup-expired-tokens",
                service => service.CleanupExpiredTokensAsync(),
                Cron.Daily(),
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            RecurringJob.AddOrUpdate<CleanupPendingInvitationsCronJobService>(
                "cleanup-pending-invitations",
                service => service.CleanupExpiredInvitationsAsync(),
                Cron.Daily(),
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            RecurringJob.AddOrUpdate<TaskReminderCronJobService>(
                "send-task-deadline-reminders",
                service => service.SendDeadlineRemindersAsync(),
                Cron.Daily(),
                new RecurringJobOptions
                {
                    TimeZone = vietnamTimeZone
                });

            return app;
        }
    }
}
