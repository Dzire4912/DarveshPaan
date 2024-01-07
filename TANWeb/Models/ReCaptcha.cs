using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Models
{
    [ExcludeFromCodeCoverage]
    public class ReCaptcha
    {
        public string? SiteKey { get; set; }
        public string? SecretKey { get; set; }
        public string? Version { get; set; }
        public string? UseRecaptchaNet { get; set; }
        public double ScoreThreshold { get; set; }
        public string? VerifyTokenUrl { get; set; }
    }
}
