using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web;

namespace TANWeb.Helpers
{
    [ExcludeFromCodeCoverage]
    public class SendSMS
    {
        public string SendMobileSMS(string Message, string PhoneNumber)
        {
            try
            {
                string message = HttpUtility.UrlEncode(Message);
                using (var wb = new WebClient())
                {
                    byte[] response = wb.UploadValues("https://api.textlocal.in/send/", new NameValueCollection()
                {
                {"apikey" , "yourapiKey"},
                {"numbers" , PhoneNumber},
                {"message" , message},
                {"sender" , "TXTLCL"}
                });
                    string result = System.Text.Encoding.UTF8.GetString(response);
                    return result;
                }
            }
            catch(Exception ex) {
                throw;
            }
            
        }
    }
}
