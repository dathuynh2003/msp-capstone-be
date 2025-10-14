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
                                <h2 style='color: #d35400; margin-bottom: 20px;'>Chào mừng đến với Meeting Support Platform!</h2>
                                <p style='color: #555555; font-size: 16px; line-height: 1.6; margin-bottom: 25px;'>
                                    Xin chào <strong>{Fullname}</strong>,
                                </p>
                                <p style='color: #555555; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
                                    Cảm ơn bạn đã đăng ký với chúng tôi! Để hoàn tất đăng ký và bắt đầu sử dụng nền tảng, vui lòng xác nhận email bằng cách nhấn nút bên dưới.
                                </p>
                                <div style='margin: 30px 0;'>
                                    <a href='{confirmationUrl}'
                                       style='background-color: #ff6f00; color: white; padding: 15px 35px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                                        Xác nhận Email
                                    </a>
                                </div>
                                <p style='color: #999999; font-size: 14px; margin-top: 25px;'>
                                    Nếu nút trên không hoạt động, sao chép và dán liên kết này vào trình duyệt:<br/>
                                    <a href='{confirmationUrl}' style='color: #ff6f00; word-break: break-all;'>{confirmationUrl}</a>
                                </p>
                                <p style='color: #999999; font-size: 12px; margin-top: 20px;'>
                                    Liên kết này sẽ hết hạn trong 24 giờ vì lý do bảo mật.
                                </p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'/>
                                <p style='color: #999999; font-size: 12px;'>
                                    Trân trọng,<br/>Đội ngũ Meeting Support Platform
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
            string title = isApproved ? "Chúc mừng! Bạn đã được duyệt" : "Thông báo: Yêu cầu của bạn không được chấp nhận";
            string mainMessage = isApproved
                ? $"Xin chào <strong>{Fullname}</strong>,<br/>Chúng tôi rất vui thông báo rằng bạn đã được duyệt làm Business Owner trên nền tảng của chúng tôi. Bây giờ bạn có thể bắt đầu quản lý và phát triển doanh nghiệp của mình với các công cụ hỗ trợ chuyên nghiệp."
                : $"Xin chào <strong>{Fullname}</strong>,<br/>Chúng tôi rất tiếc thông báo rằng yêu cầu đăng ký làm Business Owner của bạn hiện tại chưa được chấp nhận. Bạn có thể thử lại hoặc liên hệ với đội ngũ hỗ trợ để biết thêm chi tiết.";

            string buttonHtml = isApproved
                ? @"<div style='margin: 30px 0;'>
                        <a href='http://localhost:3000'
                           style='background-color: #ff6f00; color: white; padding: 15px 35px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                            Truy cập tài khoản
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
                                    Trân trọng,<br/>Đội ngũ Meeting Support Platform
                                </p>
                            </div>
                        </body>
                        </html>";
        }
    }
}
