using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Controllers;
using TANWeb.Interface;
using TANWeb.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Moq.Protected;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class StorageControllerMock
    {
        private readonly StorageController _storageController;
        private readonly Mock<IStorage> _storageServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<UserManager<AspNetUser>> _userManagerMock;
        private readonly Mock<IHttpClientFactory> _iHttpClientFactory;

        public StorageControllerMock()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c.GetSection("ConnectionStrings")["blobConn"])
                              .Returns("DefaultEndpointsProtocol=https;AccountName=thinkanewpbjsnap;AccountKey=/E6AG/OzewMNY3pSzqbCLsTOQNTINEPF3wt7uSnXI1kMCnm0/M1V4A/aElodgq5GqbXBdyPeaYLO+AStFDHwIw==;EndpointSuffix=core.windows.net");
            _userManagerMock = new Mock<UserManager<AspNetUser>>(Mock.Of<IUserStore<AspNetUser>>(), null, null, null, null, null, null, null, null);
            _storageServiceMock = new Mock<IStorage>();
            _iHttpClientFactory = new Mock<IHttpClientFactory>();
            _storageController = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task UploadFile_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            IFormFile file = null;
            StorageRequestViewModel storageRequestViewModel = new StorageRequestViewModel();

            // Act
            var result = await _storageController.UploadFile(storageRequestViewModel, file);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UploadFile_ValidFile_Success()
        {
            // Arrange
            StorageRequestViewModel storageRequestViewModel = new StorageRequestViewModel();
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns("demo123467.csv");
            formFileMock.Setup(ff => ff.Length).Returns(100);

            // Simulate file content
            var fileContent = "This is the content of the example file.";
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            formFileMock.Setup(ff => ff.OpenReadStream()).Returns(contentStream);

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Assuming "success" scenario for storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync("success");

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, formFileMock.Object);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("success", jsonResult.Value);
        }

        [Fact]
        public async Task UploadFile_ValidFile_Exists()
        {
            // Arrange
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance
            StorageRequestViewModel storageRequestViewModel = new StorageRequestViewModel();

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns("demo123457.csv");
            formFileMock.Setup(ff => ff.Length).Returns(100);

            // Simulate file content
            var fileContent = "This is the content of the example file.";
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            formFileMock.Setup(ff => ff.OpenReadStream()).Returns(contentStream);

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Assuming "success" scenario for storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync("success");

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, formFileMock.Object);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("exists", jsonResult.Value);
        }

        [Fact]
        public async Task UploadFile_InValidFile_Failed()
        {
            // Arrange
            StorageRequestViewModel storageRequestViewModel = new StorageRequestViewModel();
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns("demo4.csv");
            formFileMock.Setup(ff => ff.Length).Returns(100);

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Assuming "success" scenario for storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync("failed");

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, formFileMock.Object);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("error", jsonResult.Value);
        }

        [Fact]
        public async Task ListFiles_FilesReturned_ReturnsJsonResultWithFiles()
        {
            // Arrange
            string searchValue = "OneDriveCSVDemo1";
            string start = "0";
            string length = "10";
            string sortColumn = "Name"; // Replace with your desired column name
            string sortDirection = "asc"; // Replace with your desired sort direction
            int draw = 1;
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var mockBlobInfoList = new List<BlobInfoViewModel>
    {
        new BlobInfoViewModel { Name = "OneDriveCSVDemo1.csv" },
    };
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = mockBlobInfoList,
                                  TotalCount = mockBlobInfoList.Count
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(searchValue, start, length, sortColumn, sortDirection, draw);

            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsTotal);
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsFiltered);
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
            Assert.Equal(mockBlobInfoList.Count, blobInfoList.Count);
            Assert.Equal(mockBlobInfoList[0].Name, blobInfoList[0].Name);
        }

        [Fact]
        public async Task ListFiles_FilesReturned_ReturnsJsonResultWithFilesSortNameDesc()
        {
            // Arrange
            string searchValue = "OneDriveCSVDemo1";
            string start = "0";
            string length = "10";
            string sortColumn = "Name"; // Replace with your desired column name
            string sortDirection = "desc"; // Replace with your desired sort direction
            int draw = 1;
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var mockBlobInfoList = new List<BlobInfoViewModel>
    {
        new BlobInfoViewModel { Name = "OneDriveCSVDemo1.csv" },
    };
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = mockBlobInfoList,
                                  TotalCount = mockBlobInfoList.Count
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(searchValue, start, length, sortColumn, sortDirection, draw);

            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsTotal);
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsFiltered);
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
            Assert.Equal(mockBlobInfoList.Count, blobInfoList.Count);
            Assert.Equal(mockBlobInfoList[0].Name, blobInfoList[0].Name);
        }

        [Fact]
        public async Task ListFiles_FilesReturned_ReturnsJsonResultWithFilesSortSizeDesc()
        {
            // Arrange
            string searchValue = "OneDriveCSVDemo1";
            string start = "0";
            string length = "10";
            string sortColumn = "Size"; // Replace with your desired column name
            string sortDirection = "desc"; // Replace with your desired sort direction
            int draw = 1;
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var mockBlobInfoList = new List<BlobInfoViewModel>
    {
        new BlobInfoViewModel { Name = "OneDriveCSVDemo1.csv" },
    };
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = mockBlobInfoList,
                                  TotalCount = mockBlobInfoList.Count
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(searchValue, start, length, sortColumn, sortDirection, draw);

            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsTotal);
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsFiltered);
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
            Assert.Equal(mockBlobInfoList.Count, blobInfoList.Count);
            Assert.Equal(mockBlobInfoList[0].Name, blobInfoList[0].Name);
        }

        [Fact]
        public async Task ListFiles_FilesReturned_ReturnsJsonResultWithFilesSortSizeAsc()
        {
            // Arrange
            string searchValue = "OneDriveCSVDemo1";
            string start = "0";
            string length = "10";
            string sortColumn = "Size"; // Replace with your desired column name
            string sortDirection = "asc"; // Replace with your desired sort direction
            int draw = 1;
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var mockBlobInfoList = new List<BlobInfoViewModel>
    {
        new BlobInfoViewModel { Name = "OneDriveCSVDemo1.csv" },
    };
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = mockBlobInfoList,
                                  TotalCount = mockBlobInfoList.Count
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(searchValue, start, length, sortColumn, sortDirection, draw);

            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsTotal);
            Assert.Equal(mockBlobInfoList.Count, (int)jsonData.recordsFiltered);
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
            Assert.Equal(mockBlobInfoList.Count, blobInfoList.Count);
            Assert.Equal(mockBlobInfoList[0].Name, blobInfoList[0].Name);
        }

        [Fact]
        public async Task ListFiles_NoFiles_ReturnsEmptyJsonResult()
        {
            // Arrange
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "Name"; // Replace with your desired column name
            string sortDirection = "asc"; // Replace with your desired sort direction
            int draw = 1;
            int recordsTotal = 0;
            int recordsFiltered = 0;
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Rahul" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = new List<BlobInfoViewModel>(),
                                  TotalCount = 0
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(searchValue, start, length, sortColumn, sortDirection, draw);

            // Assert

            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(draw, (int)jsonData.draw);
            Assert.Equal(recordsTotal, (int)jsonData.recordsTotal);
            Assert.Equal(recordsFiltered, (int)jsonData.recordsFiltered);
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
            Assert.IsType<List<BlobInfoViewModel>>(blobInfoList);
            Assert.Empty(blobInfoList);
        }

        [Fact]
        public async Task DownloadFile_ReturnsFileContentResult()
        {
            // Arrange
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance
            var fileName = "OneDriveExcelFinalFile.xlsx";

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var stream = new MemoryStream(); // Create a memory stream for testing purposes
            storageServiceMock.Setup(ss => ss.GetFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync(stream);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.DownloadFile(fileName);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);
            using (var memoryStream = new MemoryStream())
            {
                // Copy the memory stream from the mock service response
                await stream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Read the contents of the memoryStream and convert to byte array
                var memoryStreamContents = memoryStream.ToArray();

                // Get the original file contents from the storage service (mock response)
                var originalStream = await storageServiceMock.Object.GetFileAsync("user-firstname", "testfile.txt");

                // Read the contents of the original stream and convert to byte array
                var originalStreamContents = new byte[originalStream.Length];
                await originalStream.ReadAsync(originalStreamContents, 0, originalStreamContents.Length);

                // Assert that the contents of the two streams are equal
                Assert.Equal(originalStreamContents, memoryStreamContents);
            }
        }
        [Fact]
        public async Task DownloadFile_FileNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            storageServiceMock.Setup(ss => ss.GetFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync((Stream)null); // Return null to simulate file not found

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.DownloadFile("nonexistent.txt");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteFile_FileDeleted_ReturnsSuccessJsonResult()
        {
            // Arrange
            var user = new AspNetUser { Id = "b9ca1b79-cf52-4aab-9493-e3ba9b554ac9", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            storageServiceMock.Setup(ss => ss.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync(true); // Simulate successful deletion

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.DeleteFile("demo123456.csv");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isDeleted = Assert.IsType<bool>(jsonResult.Value);
            Assert.True(isDeleted);
        }

        [Fact]
        public async Task DeleteFile_FileNotDeleted_ReturnsFailedJsonResult()
        {
            // Arrange
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            storageServiceMock.Setup(ss => ss.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .ReturnsAsync(false); // Simulate failed deletion

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.DeleteFile("testfile.txt");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isDeleted = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(isDeleted);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object); // Create an instance of your controller

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // By default, the view name should be null, indicating the default view
        }

        [Fact]
        public async Task UploadFile_WithOneDrive_Success()
        {
            // Arrange
            var storageRequestViewModel = new StorageRequestViewModel
            {
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "testtesttesttest12345.csv",
            };
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream()),
                });

            var httpClient = new HttpClient(handler.Object);
            httpClient.BaseAddress = new Uri("http://example.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _iHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, null);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.Equal("success", jsonResult?.Value);
        }

        [Fact]
        public async Task UploadFile_OneDriveRequest_NotSuccessful_ReturnsError()
        {
            // Arrange
            var storageRequestViewModel = new StorageRequestViewModel
            {
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "example.txt",
            };
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(handler.Object);
            httpClient.BaseAddress = new Uri("http://example.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _iHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                 .Returns(httpClient);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, null);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.Equal("error", jsonResult?.Value);
        }

        [Fact]
        public async Task ListFiles_FilesReturned_ReturnsJsonResultWithFilesWithoutSearchValue()
        {
            // Arrange
            string start = "0";
            string length = "10";
            string sortColumn = "Name"; // Replace with your desired column name
            string sortDirection = "asc"; // Replace with your desired sort direction
            int draw = 1;
            var user = new AspNetUser {Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var storageServiceMock = new Mock<IStorage>();
            var mockBlobInfoList = new List<BlobInfoViewModel>
    {
        new BlobInfoViewModel { Name = "DemoFileKrunalSir.xlsx" },
    };
            storageServiceMock.Setup(ss => ss.ListFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ReturnsAsync(new BlobListResponseViewModel
                              {
                                  Files = mockBlobInfoList,
                                  TotalCount = mockBlobInfoList.Count
                              });

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.ListFiles(null, start, length, sortColumn, sortDirection, draw);

            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            var blobInfoList = (jsonData.data as JArray)?.ToObject<List<BlobInfoViewModel>>();
            Assert.NotNull(blobInfoList);
        }

        [Fact]
        public async Task UploadFile_ValidFileSizeExceeds_Failed()
        {
            // Arrange
            StorageRequestViewModel storageRequestViewModel = new StorageRequestViewModel();
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" }; // Create a user instance

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns("demo5.csv");
            formFileMock.Setup(ff => ff.Length).Returns(104857610);
            // Simulate file content
            var fileContent = "This is the content of the example file.";
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            formFileMock.Setup(ff => ff.OpenReadStream()).Returns(contentStream);

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Assuming "success" scenario for storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync("sizeexceeds");

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, formFileMock.Object);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("sizeexceeds", jsonResult.Value);
        }

        [Fact]
        public async Task UploadFile_WithOneDrive_SizeExceeds()
        {
            // Arrange
            var storageRequestViewModel = new StorageRequestViewModel
            {
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "testtesttesttest12345.csv",
            };
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var fileSizeInBytes = 104857610;
            var contentStream = new MemoryStream(new byte[fileSizeInBytes]);
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream),
                });

            var httpClient = new HttpClient(handler.Object);
            httpClient.BaseAddress = new Uri("http://example.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _iHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, null);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.Equal("sizeexceeds", jsonResult?.Value);
        }

        [Fact]
        public async Task UploadFile_WithOneDrive_InvalidExtension()
        {
            // Arrange
            var storageRequestViewModel = new StorageRequestViewModel
            {
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "testtesttesttest12345.txt",
            };
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var fileSizeInBytes = 1000;
            var contentStream = new MemoryStream(new byte[fileSizeInBytes]);
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream),
                });

            var httpClient = new HttpClient(handler.Object);
            httpClient.BaseAddress = new Uri("http://example.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _iHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new StorageController(_configurationMock.Object, _userManagerMock.Object, _iHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UploadFile(storageRequestViewModel, null);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.Equal("invalidextension", jsonResult?.Value);
        }
    }
}
