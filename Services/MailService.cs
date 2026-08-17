
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
            var fromAddress = _configuration["MailSettings:Mail"];
            var displayName = _configuration["MailSettings:DisplayName"];
            var host = _configuration["MailSettings:Host"];
            var password = _configuration["MailSettings:Password"];
            var portValue = _configuration["MailSettings:Port"];
            var socketOptionValue = _configuration["MailSettings:SecureSocketOption"];

            if (string.IsNullOrWhiteSpace(fromAddress) ||
                string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(password) ||
                !int.TryParse(portValue, out var port))
            {
                throw new InvalidOperationException("MailSettings are incomplete. Check Mail, Password, Host and Port.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(displayName ?? fromAddress, fromAddress));
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

            var socketOptions = ResolveSecureSocketOptions(socketOptionValue, port);

            await smtp.ConnectAsync(host, port, socketOptions, cancellationToken);
            await smtp.AuthenticateAsync(fromAddress, password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken: cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }

        private static SecureSocketOptions ResolveSecureSocketOptions(string? configuredValue, int port)
        {
            if (!string.IsNullOrWhiteSpace(configuredValue) &&
                Enum.TryParse<SecureSocketOptions>(configuredValue, ignoreCase: true, out var configuredOption))
            {
                return configuredOption;
            }

            return port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                25 => SecureSocketOptions.None,
                _ => SecureSocketOptions.StartTls
            };
        }

    }
}
