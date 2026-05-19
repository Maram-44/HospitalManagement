using HospitalManagement.BussinessLogic.InterfacesServices;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) { _config = config; }
        public async Task SendEmailAsync(string email, string subject, string message)
        {


            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Pulse Health", "noreply@pulse.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            // تصميم الرسالة (يمكنك استخدام HTML لتبدو احترافية كشعار Pulse)
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                var host = _config["Mailtrap:Host"];
                var port = int.Parse(_config["Mailtrap:Port"]);
                var user = _config["Mailtrap:UserName"];
                var pass = _config["Mailtrap:Password"];

                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
