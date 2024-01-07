using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Inventory.Models
{
    [ExcludeFromCodeCoverage]
    public class BlobStorageCredModel
    {
        public string StorageConnectionString { get; set; }
        public string KronosDecryptOrEncryptKeyBlobNameGLC { get; set; }
    }
}
