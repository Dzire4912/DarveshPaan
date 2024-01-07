using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Web.Mvc;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Interface;

namespace TANWeb.Services
{
    public class StorageService : IStorage
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public StorageService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);

        }

        [HttpPost]
        public async Task<string> UploadFileAsync(string containerName, string fileName, Stream fileStream)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(fileName);

                if (await blobClient.ExistsAsync())
                {
                    return "exists";
                }
                else
                {
                    await blobClient.UploadAsync(fileStream);
                    return "success";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Storage Service / Uploading File to Cloud :{ErrorMsg}", ex.Message);
                return "failed";
            }
        }

        public async Task<BlobListResponseViewModel> ListFilesAsync(string containerName, string searchValue, string sortOrder, int skip, int take)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!containerClient.Exists())
            {
                return new BlobListResponseViewModel { Files = Enumerable.Empty<BlobInfoViewModel>(), TotalCount = 0 };
            }
            var files = new List<BlobInfoViewModel>();

            if (string.IsNullOrEmpty(searchValue))
            {
                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: null, traits: BlobTraits.Metadata))
                {
                    var blobInfo = new BlobInfoViewModel
                    {
                        Name = blobItem.Name,
                        Size = blobItem.Properties.ContentLength ?? 0
                    };

                    files.Add(blobInfo);
                }
            }
            else
            {
                await foreach (var blobItem in containerClient.GetBlobsByHierarchyAsync(prefix: searchValue, traits: BlobTraits.Metadata))
                {
                    var blobInfo = new BlobInfoViewModel
                    {
                        Name = blobItem.Blob.Name,
                        Size = blobItem.Blob.Properties.ContentLength ?? 0
                    };

                    files.Add(blobInfo);
                }
            }

            //sort data
            if (!string.IsNullOrEmpty(sortOrder))
            {

                switch (sortOrder)
                {
                    case "name_asc":
                        files = files.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
                        break;
                    case "name_desc":
                        files = files.OrderByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
                        break;
                    case "size_asc":
                        files = files.OrderBy(x => x.Size).ToList();
                        break;
                    case "size_desc":
                        files = files.OrderByDescending(x => x.Size).ToList();
                        break;
                }
            }

            int totalCount = files.Count;

            // Implement pagination using Skip and Take
            files = files.Skip(skip).Take(take).ToList();

            return new BlobListResponseViewModel { Files = files, TotalCount = totalCount };
        }

        public async Task<Stream> GetFileAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.DownloadAsync();

                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == Convert.ToInt32(UploadClientFilesFailureSuccess.RequestFailed))
            {
                Log.Error("File not found in the storage container: {FileName}", fileName);
                return (Stream)null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in Storage Service / Getting File from Cloud :{ErrorMsg}", ex.Message);
                return (Stream)null;
            }
        }

        public async Task<bool> DeleteFileAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (!await containerClient.ExistsAsync())
                {
                    // Container does not exist
                    return false;
                }

                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    // Blob does not exist
                    return false;
                }

                await blobClient.DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in Storage Service / Deleting File from Cloud :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<Stream> GetFileContentAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.OpenReadAsync();

                // Return the stream directly
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == Convert.ToInt32(UploadClientFilesFailureSuccess.RequestFailed))
            {
                Log.Error("File not found in the storage container: {FileName}", fileName);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in Storage Service / Getting File from Cloud :{ErrorMsg}", ex.Message);
                return null;
            }
        }


    }
}
