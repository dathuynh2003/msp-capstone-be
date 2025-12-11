using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MSP.WebAPI.Filters
{
    /// <summary>
    /// Custom authorization filter for Hangfire Dashboard
    /// WARNING: This allows unrestricted access. Use with caution in production.
    /// Consider implementing proper authentication/authorization for production environments.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _username = "admin@gmail.com"; // Change this
        private readonly string _password = "1"; // Change this

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            string authHeader = httpContext.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                if (username == _username && password == _password)
                {
                    return true;
                }
            }

            // Challenge for Basic Auth
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
            return false;
        }
    }
}
