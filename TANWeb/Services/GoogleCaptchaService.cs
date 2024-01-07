using AspNetCore.ReCaptcha;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web;
using TANWeb.Interface;

namespace TANWeb.Services
{
    [ExcludeFromCodeCoverage]
    public class GoogleCaptchaService : IGoogleCaptcha
    {
        private readonly IConfiguration config; 
        public GoogleCaptchaService(IConfiguration _config)
        {
            config = _config; 
        }
        public async Task<ReCaptchaRepsonse> ValidateToken(string Token)
        {
            ReCaptchaRepsonse repsonse = new ReCaptchaRepsonse();
            try
			{
                string RecaptchaSecretKey = config.GetValue<string>("ReCaptcha:SecretKey").ToString();
                string VerifyTokenUrl = config.GetValue<string>("ReCaptcha:VerifyTokenUrl").ToString();
                double ScoreThreshold = config.GetValue<double>("ReCaptcha:ScoreThreshold");
                var dictionary = new Dictionary<string, string>
                    {
                        { "secret", RecaptchaSecretKey },
                        { "response", Token }

                    };
                var postContent = new FormUrlEncodedContent(dictionary); 
                HttpResponseMessage recaptchaResponse = null; 
                string stringContent = "";
                using (var http = new HttpClient())
                {
                    recaptchaResponse = await http.PostAsync(VerifyTokenUrl, postContent);
                    stringContent = await recaptchaResponse.Content.ReadAsStringAsync();
                }
                if (!recaptchaResponse.IsSuccessStatusCode)
                {
                    repsonse.Success = false;
                    return repsonse;
                } 
                if (string.IsNullOrEmpty(stringContent))
                {
                    repsonse.Success = false;
                    return repsonse;
                }
                var googleReCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(stringContent);

                if (!googleReCaptchaResponse.Success)
                {
                    repsonse.Success = false;
                    return repsonse;
                }
                repsonse.Success = true;
                repsonse.Score = googleReCaptchaResponse.Score;
                return repsonse;
            }
            catch (Exception ex)
			{
                Log.Error(ex, "An Error Occured ValidateToken :{ErrorMsg}", ex.Message);
            }
            repsonse.Success = false;
            return repsonse;
        }

        public class ReCaptchaRepsonse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
        }
         
    }
}
