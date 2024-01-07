using TANWeb.Interface;
using TANWeb.Models;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using TAN.DomainModels.ViewModels;
using Microsoft.AspNetCore.Routing.Template;
using Serilog;
using TANWeb.Resources;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Helpers
{
    public interface IMailHelper
    {
        Task SendMail(SendMail sendMail);

        [ExcludeFromCodeCoverage]
        public class MailHelper : IMailHelper
        {
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly IConfiguration _configuration;

            private readonly string? _smtpServer;
            private readonly string? _sender;
            private readonly string? _password;
            private readonly int _smtpPort;
            private readonly bool _smtpEnabledSsl;
            private readonly bool _smtpCredentialsRequired;

            public MailHelper(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
            {
                _configuration = configuration;
                _webHostEnvironment = webHostEnvironment;

                // Get SMTP details
                _smtpServer = _configuration.GetValue<string>("SMTPCredentials:Server");
                _sender = _configuration.GetValue<string>("SMTPCredentials:UserName");
                _password = _configuration.GetValue<string>("SMTPCredentials:Password");
                _smtpPort = _configuration.GetValue<int>("SMTPCredentials:Port");
                _smtpEnabledSsl = _configuration.GetValue<bool>("SMTPCredentials:SSLEnabled");
                _smtpCredentialsRequired = _configuration.GetValue<bool>("SMTPCredentials:IsCredentialRequired");
            }
            public async Task SendMail(SendMail sendMail)
            {
                using (SmtpClient client = new SmtpClient(_smtpServer))
                {
                    client.Port = _smtpPort;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = _smtpEnabledSsl;

                    if (_smtpCredentialsRequired)
                    {
                        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(_sender, _password);
                        client.Credentials = credentials;
                    }

                    try
                    {
                        using (var mail = new MailMessage())
                        {
                            mail.From = new MailAddress(_sender.Trim());
                            foreach (var RecipiAddress in sendMail.Recipients)
                            {
                                mail.To.Add(RecipiAddress.Trim());
                            }
                            if (sendMail.BCC != null)
                            {
                                foreach (var BccAddress in sendMail.BCC)
                                {
                                    mail.Bcc.Add(BccAddress.Trim());
                                }
                            }
                            if (sendMail.CC != null)
                            {
                                foreach (var CcAddress in sendMail.CC)
                                {
                                    mail.CC.Add(CcAddress.Trim());
                                }
                            }
                            mail.Subject = sendMail.Subject;
                            mail.Body = sendMail.Body;
                            mail.IsBodyHtml = true;
                            if (sendMail.AttachmentFromServer != null)
                            {
                                foreach (string filename in sendMail.AttachmentFromServer)
                                {
                                    var path = Path.Combine(_webHostEnvironment.ContentRootPath, filename);
                                    var stream = new FileStream(path, FileMode.Open);
                                    mail.Attachments.Add(new Attachment(stream, filename));
                                }
                            }
                            if (sendMail.AttachmentFromUpload != null)
                            {
                                foreach (IFormFile Filename in sendMail.AttachmentFromUpload)
                                {
                                    mail.Attachments.Add(new Attachment(Filename.OpenReadStream(), Path.GetFileName(Filename.FileName)));
                                }

                            }
                            await client.SendMailAsync(mail);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error occured while sending an email", ex.Message);
                        throw ex;
                    }
                }
            }
        }
    }
}
