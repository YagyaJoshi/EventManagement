using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace EventManagement.Utilities.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;
        //private readonly IAuthServices _authService;

        public EmailService(IConfiguration configuration, IHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public async Task SendEmail(string to, string subject, string html, List<string> cc = null)
        {
            try
            {
                // Create an instance of the SmtpClient class
                using (SmtpClient smtpClient = new SmtpClient(_configuration["AppSettings:SmtpHost"], Convert.ToInt32(_configuration["AppSettings:SmtpPort"])))
                {
                    smtpClient.Credentials = new NetworkCredential(_configuration["AppSettings:SmtpUser"], _configuration["AppSettings:SmtpPass"]);

                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.EnableSsl = true;

                    // Set "Ilumis" as the display name
                    string fromEmail = _configuration["AppSettings:EmailFrom"];
                    string fromName = _configuration["AppSettings:EmailFromName"];

                    // Create a MailMessage object
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(fromEmail, fromName); // Corrected
                        mailMessage.To.Add(to);
                        mailMessage.Subject = subject;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Body = html;
                        if (cc != null && cc.Count > 0)
                        {
                            foreach (var ccAddress in cc)
                            {
                                mailMessage.CC.Add(ccAddress);
                            }
                        }
                        // Send the email
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
