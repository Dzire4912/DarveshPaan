using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Inventory.Models
{
    [ExcludeFromCodeCoverage]
    public class KronosSftpCredModel
    {
        public string KronosHostGLC { get; set; }
        public string KronosPortGLC { get; set; }
        public string KronosUserNameGLC { get; set; }
        public string KronosPasswordGLC { get; set; }
    }
}
