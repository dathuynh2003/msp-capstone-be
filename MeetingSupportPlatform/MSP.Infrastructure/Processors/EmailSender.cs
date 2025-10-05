using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSP.Application.Abstracts;
using MSP.Infrastructure.Options;
using System.Net;
using System.Net.Mail;

namespace NotificationService.Infrastructure.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            await SendEmailAsync(to, subject, body, null, null, null, null, isHtml);
        }

        public async Task SendEmailAsync(string to, string subject, string body, string? from = null, bool isHtml = false)
        {
            await SendEmailAsync(to, subject, body, from, null, null, null, isHtml);
        }

        public async Task SendEmailAsync(string to, string subject, string body, string? from = null, string? replyTo = null, bool isHtml = false)
        {
            await SendEmailAsync(to, subject, body, from, replyTo, null, null, isHtml);
        }

        public async Task SendEmailAsync(string to, string subject, string body, string? from = null, string? replyTo = null, string? cc = null, string? bcc = null, bool isHtml = false)
        {
            try
            {
                using var client = CreateSmtpClient();
                using var message = CreateMailMessage(to, subject, body, from, replyTo, cc, bcc, isHtml);

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
                throw;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient
            {
                Host = _emailSettings.SmtpServer,
                Port = _emailSettings.SmtpPort,
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }

        private MailMessage CreateMailMessage(string to, string subject, string body, string? from, string? replyTo, string? cc, string? bcc, bool isHtml)
        {
            var message = new MailMessage
            {
                From = new MailAddress(from ?? _emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            if (!string.IsNullOrEmpty(replyTo))
                message.ReplyToList.Add(replyTo);

            if (!string.IsNullOrEmpty(cc))
                message.CC.Add(cc);

            if (!string.IsNullOrEmpty(bcc))
                message.Bcc.Add(bcc);

            return message;
        }
    }
}

