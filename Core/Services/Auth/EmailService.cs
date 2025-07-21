using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Services.Auth
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendResetLink(string toEmail, string token)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:User"];
            var smtpPass = _config["Smtp:Pass"];
            var fromEmail = _config["Smtp:From"];

            var resetUrl = $"{_config["App:FrontendUrl"]}/reset-password?email={WebUtility.UrlEncode(toEmail)}&token={WebUtility.UrlEncode(token)}";

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Password Reset",
                Body = $"Click here to reset your password: {resetUrl}",
                IsBodyHtml = false
            };
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };
            await client.SendMailAsync(message);
        }
    }
}
