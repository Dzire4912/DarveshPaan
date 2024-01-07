using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PgpCore;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TANWeb.Areas.Inventory.Models;
using TANWeb.Services;

namespace TANWeb.Areas.Inventory.Services
{
    [ExcludeFromCodeCoverage]
    public class DecryptionService
    {
        private readonly IConfiguration _config;
        private readonly BlobStorageCredModel blobStorageCredModel;
        string kronosDecryptionPassword = string.Empty;
        public DecryptionService(IConfiguration config)
        {
            _config = config;
            blobStorageCredModel = new BlobStorageCredModel();
            config.Bind("BlobStorageCreds", blobStorageCredModel);
            kronosDecryptionPassword = config.GetValue<string>("KronosDecryptionPasswordGLC");
        }

        public async Task<Stream> DecryptPgpFileStream(Stream file,KronosBlobStorageCreds kronosBlobStorageCreds)
        {

            Stream decryptKeyStream = await GetEncryptDecryptKeyStream(kronosBlobStorageCreds);
            Stream outputStream = new MemoryStream();

            decryptKeyStream.Position = 0;
            file.Position = 0;

            try
            {
                using (PGP pgp = new PGP())
                {
                    pgp.DecryptStream(file, outputStream, decryptKeyStream, EncryptionService.Decrypt(kronosBlobStorageCreds.KronosDecryptionPasswordGLC));
                    outputStream.Position = 0;
                    return outputStream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private async Task<Stream> GetEncryptDecryptKeyStream(KronosBlobStorageCreds kronosBlobStorageCreds)
        {
            try
            {
                string storageConnectionString = EncryptionService.Decrypt(kronosBlobStorageCreds.StorageConnectionString);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("keys");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{EncryptionService.Decrypt(kronosBlobStorageCreds.KronosDecryptOrEncryptKeyBlobNameGLC)}.asc");
                Stream keyStream = new MemoryStream();
                await blockBlob.DownloadToStreamAsync(keyStream).ConfigureAwait(false);
                return keyStream;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
