using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Models
{
    public class SendMail
    {
        public string[]? Recipients { get; set; }
        [ExcludeFromCodeCoverage]
        public string[]? CC { get; set; }
        [ExcludeFromCodeCoverage]
        public string[]? BCC { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        [ExcludeFromCodeCoverage]
        public string[]? AttachmentFromServer { get; set; }
        [ExcludeFromCodeCoverage]
        public List<IFormFile>? AttachmentFromUpload { get; set; }
        [ExcludeFromCodeCoverage]
        public List<KeyValuePair<string, string>>? PlaceHolders { get; set; }
    }
}
