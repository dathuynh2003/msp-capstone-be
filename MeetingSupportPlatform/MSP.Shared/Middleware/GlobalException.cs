using System.Net;
using System.Text.Json;
using MSP.Shared.Logs;
using MSP.Shared.Common;
using MSP.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace MSP.Shared.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Declare default variables
            string message = "Sorry, internal server error occurred. Kindly try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";
            try
            {
                await next(context);

                //check if Response is too many request // 429 status code
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // If Response is UnAuthorized // 401 status code
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access";
                    statusCode = (int)StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                //if Response is Forbidden // 403 Status code
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You are not allowed/required to access.";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                //Log Original Exceptions / File, Debugger, Console
                LogException.LogExceptions(ex);

                //check if Exception is Timeout // 408 request timeout
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out of time";
                    message = "Request timeout...try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                else if (ex is BadRequestException badRequestEx)
                {
                    title = "Bad Request";
                    message = string.IsNullOrWhiteSpace(badRequestEx.Message) ? "Bad request" : badRequestEx.Message;
                    statusCode = StatusCodes.Status400BadRequest;
                }
                else if (ex is NotFoundException)
                {
                    title = "Not Found";
                    message = ex.Message;
                    statusCode = StatusCodes.Status404NotFound;
                }
                else if (ex is InternalServerException internalServerEx)
                {
                    title = "Error";
                    message = string.IsNullOrWhiteSpace(internalServerEx.Message) ? message : internalServerEx.Message;
                    statusCode = StatusCodes.Status500InternalServerError;
                }
                else if (ex.GetType().Name == "LoginFailedException")
                {
                    title = "Authentication Failed";
                    message = ex.Message;
                    statusCode = StatusCodes.Status401Unauthorized;
                }
                else if (ex.GetType().Name == "RegistrationFailedException")
                {
                    title = "Registration Failed";
                    message = ex.Message;
                    statusCode = StatusCodes.Status400BadRequest;
                }
                else if (ex.GetType().Name == "RefreshTokenException")
                {
                    title = "Invalid Refresh Token";
                    message = ex.Message;
                    statusCode = StatusCodes.Status401Unauthorized;
                }
                else if (ex.GetType().Name == "UserAlreadyExistsException")
                {
                    title = "Conflict";
                    message = ex.Message;
                    statusCode = StatusCodes.Status409Conflict;
                }
                //If exception is caught.
                //if none of the exception then do the default
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // display scary-free message to client
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // If response already started, do not attempt to write
            if (context.Response.HasStarted)
            {
                return;
            }

            // Build a consistent ApiResponse payload for errors
            var errorResponse = ApiResponse<object>.ErrorResponse(null, message, new List<string> { title });
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse), cancellationToken: CancellationToken.None);
            return;
        }
    }
}
