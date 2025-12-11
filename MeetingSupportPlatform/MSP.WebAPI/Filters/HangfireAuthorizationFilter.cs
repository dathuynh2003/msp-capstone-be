using Hangfire.Dashboard;

namespace MSP.WebAPI.Filters
{
    /// <summary>
    /// Custom authorization filter for Hangfire Dashboard
    /// WARNING: This allows unrestricted access. Use with caution in production.
    /// Consider implementing proper authentication/authorization for production environments.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // Chỉ cho phép Admin role
            return httpContext.User.Identity?.IsAuthenticated == true 
                   && httpContext.User.IsInRole("Admin");
        }
    }
}
