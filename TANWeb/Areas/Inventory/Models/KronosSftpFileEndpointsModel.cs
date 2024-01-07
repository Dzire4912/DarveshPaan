using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Inventory.Models
{
    [ExcludeFromCodeCoverage]
    public class KronosSftpFileEndpointsModel
    {
        public string KronosPunchExportGLC { get; set; }
        public string KronosPayrollGLC { get; set; }
    }
}
