namespace MSP.Shared.Specifications
{
    public class EmailNotificationTemplate
    {
        public static string ConfirmMailNotification(
            string Fullname,
            string confirmationUrl
        )
        {
            return $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                                <h2 style='color: #333; margin-bottom: 20px;'>Welcome to Meeting Support Platform!</h2>
                                <p style='color: #666; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    Hello {Fullname},
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
        }
    }
}
