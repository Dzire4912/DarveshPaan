using static TAN.DomainModels.Models.UploadModel;
using static TANWeb.Services.GoogleCaptchaService;

namespace TANWeb.Interface
{
    public interface IGoogleCaptcha
    {
        Task<ReCaptchaRepsonse> ValidateToken(string Token);
    }
}
