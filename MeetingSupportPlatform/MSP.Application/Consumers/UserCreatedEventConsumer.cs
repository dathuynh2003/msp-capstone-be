using MSP.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using MSP.Application.Services.Interfaces.Notification;

namespace NotificationService.Application.Consumers
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedEventConsumer> _logger;
        private readonly INotificationService _notificationService;

        public UserCreatedEventConsumer(
            ILogger<UserCreatedEventConsumer> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var userCreatedEvent = context.Message;
            
            _logger.LogInformation("Received UserCreatedEvent for user: {Email}", userCreatedEvent.Email);
            
            try
            {
                // Create welcome notification
                await _notificationService.SendToUserAsync(
                    userCreatedEvent.UserId.ToString(),
                    "Welcome to Meeting Support Platform!",
                    $"Hello {userCreatedEvent.FirstName} {userCreatedEvent.LastName}, welcome to our platform. Your account has been created successfully with role: {userCreatedEvent.Role}",
                    "InApp",
                    $"{{\"eventType\":\"UserCreated\",\"userId\":\"{userCreatedEvent.UserId}\",\"role\":\"{userCreatedEvent.Role}\"}}"
                );

                // Send email confirmation if token is provided
                if (!string.IsNullOrEmpty(userCreatedEvent.ConfirmationToken))
                {
                    var confirmationUrl = $"https://localhost:7129/api/v1/auth/confirm-email?email={Uri.EscapeDataString(userCreatedEvent.Email)}&token={Uri.EscapeDataString(userCreatedEvent.ConfirmationToken)}";
                    
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                                <h2 style='color: #333; margin-bottom: 20px;'>Welcome to Meeting Support Platform!</h2>
                                <p style='color: #666; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    Hello {userCreatedEvent.FirstName} {userCreatedEvent.LastName},
                                </p>
                                <p style='color: #666; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    Thank you for registering with us! To complete your registration and start using our platform, 
                                    please confirm your email address by clicking the button below.
                                </p>
                                <div style='margin: 30px 0;'>
                                    <a href='{confirmationUrl}' 
                                       style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                                        Confirm Email Address
                                    </a>
                                </div>
                                <p style='color: #999; font-size: 14px; margin-top: 30px;'>
                                    If the button doesn't work, you can copy and paste this link into your browser:<br/>
                                    <a href='{confirmationUrl}' style='color: #007bff; word-break: break-all;'>{confirmationUrl}</a>
                                </p>
                                <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                                    This link will expire in 24 hours for security reasons.
                                </p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                                <p style='color: #999; font-size: 12px;'>
                                    Best regards,<br/>Meeting Support Platform Team
                                </p>
                            </div>
                        </body>
                        </html>";

                    await _notificationService.SendToUserAsync(
                        userCreatedEvent.Email,
                        "Confirm Your Email Address - Meeting Support Platform",
                        emailBody,
                        "Email"
                    );
                    
                    _logger.LogInformation("Email confirmation sent to user {Email}", userCreatedEvent.Email);
                }
                else
                {
                    // Send regular welcome email if no confirmation token
                    await _notificationService.SendToUserAsync(
                        userCreatedEvent.Email,
                        "Welcome to Meeting Support Platform!",
                        $"Hello {userCreatedEvent.FirstName} {userCreatedEvent.LastName},<br/><br/>Welcome to our platform. Your account has been created successfully with role: {userCreatedEvent.Role}.<br/><br/>Best regards,<br/>Meeting Support Platform Team",
                        "Email"
                    );
                }
                
                _logger.LogInformation("Welcome notification sent to user {Email}", userCreatedEvent.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome notification to user {Email}", userCreatedEvent.Email);
            }
            
            _logger.LogInformation("User {Email} created successfully. Role: {Role}", 
                userCreatedEvent.Email, userCreatedEvent.Role);
        }
    }
}
