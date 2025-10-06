using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace core8_astro_informix.Services
{
    public interface IEmailService {
        void sendMail(string to,string fullname, string subject, string msgBody);
        void sendMailToken(string to, string subject, string msgBody);
    }

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

        public void sendMail(string to,string fullname, string subject, string msgBody) {
                    MimeMessage message = new MimeMessage();
                    message.From.Add(MailboxAddress.Parse(_configuration["rey107@gmail.com"]));
                    message.To.Add(MailboxAddress.Parse(to));                    
                    message.Subject = subject;

                    message.Body = new TextPart(MimeKit.Text.TextFormat.Html) 
                    { 
                        Text = msgBody
                    };

                    // BodyBuilder bodyBuilder = new BodyBuilder();
                    // bodyBuilder.HtmlBody = msgBody;
                    
                    using var smtp = new SmtpClient();
                    try
                    {
                         smtp.ConnectAsync("EmailSettings.smtpserver", 587, SecureSocketOptions.StartTls);
                         smtp.AuthenticateAsync(_configuration["EmailSettings:fromEmail"], _configuration["EmailSettings:emailPassword"]);
                         smtp.SendAsync(message);
                    }
                    finally
                    {
                         smtp.DisconnectAsync(true);
                    }
        }

        public void sendMailToken(string to, string subject, string msgBody) {
                    MimeMessage message = new MimeMessage();
                    message.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:fromEmail"]));
                    message.To.Add(MailboxAddress.Parse(to));                    
                    message.Subject = subject;

                    message.Body = new TextPart(MimeKit.Text.TextFormat.Html) 
                    { 
                        Text = msgBody
                    };

                    // BodyBuilder bodyBuilder = new BodyBuilder();
                    // bodyBuilder.HtmlBody = msgBody;
                    
                    using var smtp = new SmtpClient();
                    try
                    {
                         smtp.ConnectAsync("EmailSettings.smtpserver", 587, SecureSocketOptions.StartTls);
                         smtp.AuthenticateAsync(_configuration["EmailSettings:fromEmail"], _configuration["EmailSettings:emailPassword"]);
                         smtp.SendAsync(message);
                    }
                    finally
                    {
                         smtp.DisconnectAsync(true);
                    }
        }               
}

}