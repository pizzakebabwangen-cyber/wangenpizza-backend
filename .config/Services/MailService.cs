
using MailKit.Security;
using MimeKit;
using QuickMover.Helper;
using System.Diagnostics;
using System.Net.Mail;
using WangenPizza.Helper;
using WangenPizza.Interfaces;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;


namespace WangenPizza.Services {

    public class MailService : IMailService {

        private readonly IConfiguration _configuration;
        public MailService( IConfiguration configuration ) {
            _configuration = configuration;
        }
     



      
        public async Task SendEmailAsync(MailRequest mailRequest, CancellationToken cancellationToken = default)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["MailSettings:Mail"]));
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject ?? "";
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                foreach (var file in mailRequest.Attachments)
                {
                    if (file is not null && file.File.Length > 0)
                    {
                        builder.Attachments.Add(file.Name, file.File, MimeKit.ContentType.Parse(file.ContentType));
                    }
                }
            }

            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Timeout = 30000;

            // for gemail smtp
            await smtp.ConnectAsync(_configuration["MailSettings:Host"], int.Parse(_configuration["MailSettings:Port"]), SecureSocketOptions.StartTls, cancellationToken);

            // for info email
           // await smtp.ConnectAsync(_configuration["MailSettings:Host"], int.Parse(_configuration["MailSettings:Port"]), SecureSocketOptions.SslOnConnect, cancellationToken);

            await smtp.AuthenticateAsync(_configuration["MailSettings:Mail"], _configuration["MailSettings:Password"], cancellationToken);
            await smtp.SendAsync(email, cancellationToken: cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }

    }
}
