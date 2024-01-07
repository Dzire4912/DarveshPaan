using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using TAN.DomainModels.Helpers;
using TANWeb.Models;

namespace TANWeb.Helpers
{
    public interface IEmailHelper
    {
        Task<bool> SendEmailAsync(SendMail sendMail);

        [ExcludeFromCodeCoverage]
        public class EmailHelper : IEmailHelper
        {
            private static readonly string logicMailAppURL = GetLogicMailAppURLFromConfiguration();
            public EmailHelper()
            {

            }

            public async Task<bool> SendEmailAsync(SendMail sendMail)
            {
                bool isMailSent = await Execute(sendMail);
                return isMailSent;
            }

            static async Task<bool> Execute(SendMail sendMail)
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var requestData = new EmailRequestData
                    {
                        Email = sendMail.Recipients[0],
                        Subject = sendMail.Subject,
                        Message = sendMail.Body
                    };

                    // Serialize the request data to JSON
                    string jsonContent = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, ContentType.JSON);

                    // Make an HTTP POST request
                    var response = await client.PostAsync(logicMailAppURL, content);


                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        Log.Error("Email Sending Failed", "An Error Occured in while sending email in via Email Helper :{ErrorMsg}", "Email Sending Failed");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An Error Occured while sending an Email :{ErrorMsg}", ex.Message);
                    return false;
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public class EmailRequestData
        {
            public string Email { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private static string GetLogicMailAppURLFromConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Load the default appsettings file
                .AddJsonFile("appsettings.qa.json", optional: true, reloadOnChange: true) // Load the QA-specific appsettings file
                .AddJsonFile("appsettings.staging.json", optional: true, reloadOnChange: true) // Load the staging-specific appsettings file
                .AddJsonFile("appsettings.production.json", optional: true, reloadOnChange: true) // Load the production-specific appsettings file
                .Build();

            return configuration.GetSection("LogicMailAppURL")["URL"];
        }
    }
}
