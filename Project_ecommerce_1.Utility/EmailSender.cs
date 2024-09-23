using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSetting _setting;
        public EmailSender(IOptions<EmailSetting> setting)
        {
            _setting = setting.Value;
        }

        public async Task Execute(string email, string subject, string Message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? _setting.ToEmail : email;
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(_setting.FromEmail, "Rana Book Store "),
                };
                mailMessage.To.Add(toEmail);
                mailMessage.CC.Add(_setting.CcEmail);
                mailMessage.Subject = "Shopping App" + subject;
                mailMessage.Body = Message;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                using(SmtpClient smtp =new SmtpClient(_setting.PrimaryDomain,_setting.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_setting.UserNameEmail, _setting.UserNamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mailMessage);
                };
            }
            catch (Exception ex)
            {

                string msg=ex.Message;
            }
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Execute(email, subject, htmlMessage).Wait();
            return Task.FromResult(0);
        }


    }
}
