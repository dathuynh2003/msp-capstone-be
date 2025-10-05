namespace MSP.Application.Abstracts
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendEmailAsync(string to, string subject, string body, string? from = null, bool isHtml = false);
        Task SendEmailAsync(string to, string subject, string body, string? from = null, string? replyTo = null, bool isHtml = false);
        Task SendEmailAsync(string to, string subject, string body, string? from = null, string? replyTo = null, string? cc = null, string? bcc = null, bool isHtml = false);
    }
}

