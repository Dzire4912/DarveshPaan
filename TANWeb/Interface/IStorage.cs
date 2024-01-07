using TAN.DomainModels.ViewModels;

namespace TANWeb.Interface
{
    public interface IStorage
    {
        Task<string> UploadFileAsync(string containerName, string fileName, Stream fileStream);
        Task<BlobListResponseViewModel> ListFilesAsync(string containerName, string searchValue, string sortOrder, int skip, int take);
        Task<Stream> GetFileAsync(string containerName, string fileName);
        Task<bool> DeleteFileAsync(string containerName, string fileName);
        Task<Stream> GetFileContentAsync(string containerName, string fileName);

    }
}
