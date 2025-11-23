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
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff7f0;'>
                            <div style='background-color: #ffffff; padding: 40px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center;'>
                                <h2 style='color: #d35400; margin-bottom: 20px;'>Welcome to Meeting Support Platform!</h2>
                                <p style='color: #555555; font-size: 16px; line-height: 1.6; margin-bottom: 25px;'>
                                    Hello <strong>{Fullname}</strong>,
                                </p>
                                <p style='color: #555555; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    Thank you for registering with us! To complete your sign-up and start using the platform, please verify your email by clicking the button below.
                                </p>
                                <div style='margin: 30px 0;'>
                                    <a href='{confirmationUrl}'
                                       style='background-color: #ff6f00; color: white; padding: 15px 35px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                                        Confirm Email
                                    </a>
                                </div>
                                <p style='color: #999999; font-size: 12px; margin-top: 20px;'>
                                    This link will expire in 24 hours for security reasons.
                                </p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'/>
                                <p style='color: #999999; font-size: 12px;'>
                                    Best regards,<br/>Meeting Support Platform Team
                                </p>
                            </div>
                        </body>
                        </html>";
        }


        public static string BusinessOwnerStatusNotification(
                string Fullname,
                bool isApproved // true: duyệt, false: từ chối
            )
        {
            string title = isApproved ? "Congratulations! You have been approved"
        : "Notification: Your request has been declined";
            string mainMessage = isApproved
                ? $"Hello <strong>{Fullname}</strong>,<br/>We are pleased to inform you that your Business Owner request has been approved. You can now start managing and growing your business using our professional support tools."
                : $"Hello <strong>{Fullname}</strong>,<br/>We regret to inform you that your request to become a Business Owner has not been approved at this time. You may try again or contact our support team for more details.";

            string buttonHtml = isApproved
                ? @"<div style='margin: 30px 0;'>
                        <a href='http://localhost:3000'
                           style='background-color: #ff6f00; color: white; padding: 15px 35px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                            Access Your Account
                        </a>
                    </div>"
                : ""; // nếu từ chối thì không có button

            return $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff7f0;'>
                            <div style='background-color: #ffffff; padding: 40px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center;'>
                                <h2 style='color: #d35400; margin-bottom: 20px;'>{title}</h2>
                                <p style='color: #555555; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    {mainMessage}
                                </p>
                                {buttonHtml}
                                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'/>
                                <p style='color: #999999; font-size: 12px;'>
                                    Best regards,<br/>Meeting Support Platform Team
                                </p>
                            </div>
                        </body>
                        </html>";
        }
    }
}
