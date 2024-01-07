using AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
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
using TANWeb.Areas.TelecomReporting.Controllers;
using TANWeb.Controllers;
using TANWeb.Interface;
using TANWeb.Services;
using static TAN.DomainModels.Helpers.Permissions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TAN.DomainModels.Models;
using System.Reflection;
using TAN.UnitTest.PBJSnap.Controllers;
using TAN.Repository.Implementations;
using Newtonsoft.Json.Linq;
using System.Web.Helpers;

namespace TAN.UnitTest.TelecomReporting.Controllers
{
    public class DataServiceMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DataServiceController _dataServiceController;
        private readonly Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<IStorage> _storageServiceMock;
        public DataServiceMock()
        {
            _userManager = GetUserManagerMock();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configuration = new Mock<IConfiguration>();
            _storageServiceMock = new Mock<IStorage>();
            _configuration.Setup(c => c.GetSection("ConnectionStrings")["blobConn"])
                              .Returns("DefaultEndpointsProtocol=https;AccountName=thinkanewpbjsnap;AccountKey=/E6AG/OzewMNY3pSzqbCLsTOQNTINEPF3wt7uSnXI1kMCnm0/M1V4A/aElodgq5GqbXBdyPeaYLO+AStFDHwIw==;EndpointSuffix=core.windows.net");

            _httpClientFactory = new Mock<IHttpClientFactory>();
            _dataServiceController = new DataServiceController(_unitOfWorkMock.Object, _userManager.Object, _configuration.Object, _httpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private Mock<UserManager<AspNetUser>> GetUserManagerMock()
        {
            var userStoreMock = new Mock<IUserStore<AspNetUser>>();
            var passwordHasherMock = new Mock<IPasswordHasher<AspNetUser>>();
            var userValidators = new List<IUserValidator<AspNetUser>>();
            var passwordValidators = new List<IPasswordValidator<AspNetUser>>();

            return new Mock<UserManager<AspNetUser>>(
                userStoreMock.Object,
                null,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                null,
                null,
                null,
                null
            );
        }

        [Fact]
        public void CarrierInfo_ReturnsViewResult()
        {
            // Act
            var result = _dataServiceController.CarrierInfo();
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SaveCarrier_ValidModelState_ReturnsJsonResult()
        {
            // Arrange
            var validCarrier = new CarrierInfomationViewModel
            {
                CarrierName = "TestCarrier"
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(uow => uow.CarrierInformationRepo.AddCarrier(validCarrier)).ReturnsAsync(expectedResult);
            // Act
            var result = await _dataServiceController.SaveCarrier(validCarrier) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task SaveCarrier_InvalidModelState_ReturnsFailedJson()
        {
            // Arrange
            var invalidCarrier = new CarrierInfomationViewModel();
            _dataServiceController.ModelState.AddModelError("CarrierName", "CarrierName is Required");
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(uow => uow.CarrierInformationRepo.AddCarrier(invalidCarrier)).ReturnsAsync(expectedResult);
            // Act
            var result = await _dataServiceController.SaveCarrier(invalidCarrier) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task CheckForCarrierName_ReturnsJsonResult()
        {
            // Arrange
            var carrierName = "CenturyLink";
            var carrierId = 5;
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.GetCarrierName(carrierName, carrierId))
                .ReturnsAsync(true); // Set up the mock to return a value
            // Act
            var result = await _dataServiceController.CheckForCarrierName(carrierName, carrierId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.True((bool)result.Value); // Assuming your result is a boolean
        }

        [Fact]
        public async Task CheckForCarrierName_ThrowsException()
        {
            // Arrange
            var carrierName = "CenturyLink";
            var carrierId = 5;
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.GetCarrierName(carrierName, carrierId))
                .Throws(new Exception("An error occured while getting carrier name details"));
            // Act
            var result = await _dataServiceController.CheckForCarrierName(carrierName, carrierId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result.Value); // Assuming your result is a boolean
        }

        [Fact]
        public async Task EditCarrier_ReturnsJsonResult()
        {
            // Arrange
            var carrierId = 2;
            var expectedCarrierViewModel = new CarrierInfomationViewModel
            {
                CarrierId = 2,
                CarrierName = "abc",
                CreatedDate = DateTime.UtcNow.ToString(),
                CreatedBy = "3892a65a-8a58-46cf-998e-39e2d4d45c65",
                IsActive = true
            };
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.GetCarrierDetail(carrierId))
                .ReturnsAsync(expectedCarrierViewModel); // Set up the mock to return the expected result
            // Act
            var result = await _dataServiceController.EditCarrier(carrierId);
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockCarrierViewModel = Assert.IsType<CarrierInfomationViewModel>(jsonResult.Value);
            Assert.Equal(expectedCarrierViewModel, mockCarrierViewModel);
        }

        [Fact]
        public async Task EditCarrier_ThrowsException()
        {
            // Arrange
            var carrierId = 2;
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.GetCarrierDetail(carrierId))
                .Throws(new Exception("An error occured while getting the carrier details")); // Set up the mock to return the expected result
            // Act
            var result = await _dataServiceController.EditCarrier(carrierId);
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.IsType<CarrierInfomationViewModel>(jsonResult.Value);
        }

        [Fact]
        public async Task UpdateCarrier_ValidModelState_ReturnsJsonResult()
        {
            // Arrange
            var expectedCarrierViewModel = new CarrierInfomationViewModel
            {
                CarrierId = 2,
                CarrierName = "abc",
                CreatedDate = DateTime.UtcNow.ToString(),
                CreatedBy = "3892a65a-8a58-46cf-998e-39e2d4d45c65",
                IsActive = true
            };
            var expectedResult = "success"; // Set an expected result
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.UpdateCarrier(expectedCarrierViewModel))
                .ReturnsAsync(expectedResult); // Set up the mock to return the expected result
            // Act
            var result = await _dataServiceController.UpdateCarrier(expectedCarrierViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateCarrier_ValidModelState_ThrowsException()
        {
            // Arrange
            var expectedCarrierViewModel = new CarrierInfomationViewModel
            {
                CarrierId = 2,
                CarrierName = "abc",
                CreatedDate = DateTime.UtcNow.ToString(),
                CreatedBy = "3892a65a-8a58-46cf-998e-39e2d4d45c65",
                IsActive = true
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.UpdateCarrier(expectedCarrierViewModel))
                .Throws(new Exception("An error occured while updating the carrier details")); // Set up the mock to return the expected result
            // Act
            var result = await _dataServiceController.UpdateCarrier(expectedCarrierViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteCarrier_Success_ReturnsJsonResult()
        {
            // Arrange
            var carrierId = 1;
            var expectedResult = "success";
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.DeleteCarrierDetail(carrierId))
                .ReturnsAsync(expectedResult);
            // Act
            var result = await _dataServiceController.DeleteCarrier(carrierId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteCarrier_ThrowsException()
        {
            // Arrange
            var carrierId = 1;
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.DeleteCarrierDetail(carrierId))
                .Throws(new Exception("An error occured while deleting the carrier details"));
            // Act
            var result = await _dataServiceController.DeleteCarrier(carrierId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public void GetCarrierList_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "CarrierName";
            string sortDirection = "asc";
            int draw = 1;
            int totalRecord = 10;
            int filterRecord = 10;
            int carrierCount = 10;
            IEnumerable<CarrierInfomationViewModel> data = new List<CarrierInfomationViewModel>
            {
                new CarrierInfomationViewModel{CarrierId=2,CarrierName="abc",CreatedDate=DateTime.UtcNow.ToString(),CreatedBy="3892a65a - 8a58 - 46cf - 998e-39e2d4d45c65", UpdatedDate=null,UpdatedBy=null,IsActive=true},
                new CarrierInfomationViewModel{CarrierId=3,CarrierName="abc123",CreatedDate=DateTime.UtcNow.ToString(),CreatedBy="3892a65a - 8a58 - 46cf - 998e-39e2d4d45c65", UpdatedDate=null,UpdatedBy=null,IsActive=true},
            };
            _unitOfWorkMock.Setup(uow => uow.CarrierInformationRepo.GetAllCarriers(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), out carrierCount))
                .Returns(data);
            // Act
            var result = _dataServiceController.GetCarriersList(searchValue, start, length, sortColumn, sortDirection, draw);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<CarrierInfomationViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_WhenUserInfoExists()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Returns(true);
            // Act
            var result = _dataServiceController.GetLoggedInUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.True(mockedUserInfo);
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_WhenUserInfoDoNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Returns(false);
            // Act
            var result = _dataServiceController.GetLoggedInUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(mockedUserInfo);
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Throws(new Exception("An error occured while getting logged in user information"));
            // Act
            var result = _dataServiceController.GetLoggedInUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(mockedUserInfo);
        }

        [Fact]
        public void GetOrganizationUserInfo_ReturnsJsonResult_WhenUserInfoExists()
        {
            // Arrange
            var expectedOrganizationUserInfo = "132";
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails())
                .Returns(expectedOrganizationUserInfo);
            // Act
            var result = _dataServiceController.GetOrganizationUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var actualOrganizationUserInfo = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedOrganizationUserInfo, actualOrganizationUserInfo);
        }

        [Fact]
        public void GetOrganizationUserInfo_ReturnsJsonResult_WhenUserInfoDoNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails())
                .Returns(string.Empty);
            // Act
            var result = _dataServiceController.GetOrganizationUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(jsonResult.Value, string.Empty);
        }

        [Fact]
        public void GetOrganizationUserInfo_ReturnsJsonResult_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails())
                .Throws(new Exception("An error occured while getting organization user details"));
            // Act
            var result = _dataServiceController.GetOrganizationUserInfo();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public void GetUploadedFileListList_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "OrganizationName";
            string sortDirection = "asc";
            int draw = 1;
            int totalRecord = 10;
            int filterRecord = 10;
            int fileCount = 10;
            string orgName = "";
            string facilityName = "";
            string carrierName = "";
            string orgId = "";
            IEnumerable<UploadClientFileDetailsViewModel> data = new List<UploadClientFileDetailsViewModel>
            {
                new UploadClientFileDetailsViewModel
                {
                    OrganizationId=3,
                    FacilityId=3,
                    CarrierName="Charter",
                    FileName="StaffExport_DemoFacility20231110101187.pdf",
                    FileSize="4158",
                    CreatedDate=DateTime.UtcNow.ToString(),
                    UpdatedDate=DateTime.UtcNow,
                    CreatedBy="3892a65a - 8a58 - 46cf - 998e-39e2d4d45c65",
                    UpdatedBy=null,
                    IsActive=true,
                    IsProcessed=0
                },
                new UploadClientFileDetailsViewModel
                {
                    OrganizationId=3,
                    FacilityId=3,
                    CarrierName="CenturyLink",
                    FileName="StaffExport_DemoFacility20231109181166.pdf",
                    FileSize="3844",
                    CreatedDate=DateTime.UtcNow.ToString(),
                    UpdatedDate=DateTime.UtcNow,
                    CreatedBy="3892a65a - 8a58 - 46cf - 998e-39e2d4d45c65",
                    UpdatedBy=null,
                    IsActive=true,
                    IsProcessed=0
                },
            };
            _unitOfWorkMock.Setup(uow => uow.UploadClientFileDetailsRepo.GetAllUploadedFiles(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out fileCount))
                .Returns(data);
            // Act
            var result = _dataServiceController.GetAllUploadedFileList(searchValue, start, length, sortColumn, sortDirection, draw, orgName, facilityName, carrierName, orgId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<UploadClientFileDetailsViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public async Task UploadFile_ReturnsViewResult()
        {
            // Arrange
            var organizationId = "2";
            var organizations = new List<OrganizationModel> { new OrganizationModel { IsActive = true, OrganizationID = 2, OrganizationName = "Org1" } };
            var organizationServices = new List<OrganizationServices> { new OrganizationServices { IsActive = true, OrganizationId = 2, ServiceType = 1 } };
            var facilities = new List<FacilityModel> { new FacilityModel { Id = 1, OrganizationId = 2, FacilityName = "Facility1" } };
            var carriers = new List<CarrierInfomation> { new CarrierInfomation { Id = 1, IsActive = true, CarrierName = "Carrier1" } };
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal", UserType = 1 }; // Create a user instance
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(u => u.OrganizationRepo.GetAll()).Returns(organizations.AsQueryable());
            _unitOfWorkMock.Setup(u => u.OrganizationServiceRepo.GetAll()).Returns(organizationServices.AsQueryable());
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAll()).Returns(facilities.AsQueryable());
            _unitOfWorkMock.Setup(u => u.CarrierInformationRepo.GetAll()).Returns(carriers.AsQueryable());
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetLoggedInUserInfo()).Returns(false);
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetOrganizationUserDetails()).Returns(organizationId);
            // Act
            var result = await _dataServiceController.UploadFile() as ViewResult;
            // Assert
            Assert.NotNull(result);
            var mockedModel = Assert.IsType<UploadClientFileDetailsViewModel>(result.Model);
            Assert.Equal(mockedModel.CarrierList.Count, carriers.Count);
            Assert.Equal(mockedModel.OrganizationList.Count, organizations.Count);
            Assert.Equal(mockedModel.FacilityList.Count, facilities.Count);
            Assert.Equal(mockedModel.UserType, user.UserType);
        }

        [Fact]
        public void GetOrganizationFaciltyList_ReturnsJsonResultWithFacilityList()
        {
            // Arrange
            var orgId = "1";
            var facilities = new List<FacilityModel>
            {
                new FacilityModel { Id = 1, OrganizationId = 1, FacilityName = "Facility1", IsActive = true },
                new FacilityModel { Id = 2, OrganizationId = 1, FacilityName = "Facility2", IsActive = true },
            };
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAll()).Returns(facilities.AsQueryable());
            // Act
            var result = _dataServiceController.GetOrganizationFaciltyList(orgId);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var facilityList = result.Value as List<Itemlist>;
            Assert.NotNull(facilityList);
            Assert.Equal(facilities.Count, facilityList.Count);
        }

        [Fact]
        public void GetOrganizationFaciltyList_InvalidOrgId_ReturnsJsonResultWithEmptyFacilityList()
        {
            // Arrange
            var orgId = "invalid"; // This will trigger the exception in the method
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAll()).Throws(new Exception("An error occured while getting facility list"));
            // Act
            var result = _dataServiceController.GetOrganizationFaciltyList(orgId);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var facilityList = result.Value as List<Itemlist>;
            Assert.Null(facilityList);
        }

        [Fact]
        public async Task UploadFileToAzureBlobStorage_OneDrive_Success_ReturnsJsonResult()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "test12.pdf",
                Result = "success"
            };
            var expectedResult = "db-success";
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
            _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");
            _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(uploadClientFileRequestViewModel.Result);
            _unitOfWorkMock.Setup(u => u.UploadClientFileDetailsRepo.UploadFileDetailsToDB(uploadClientFileRequestViewModel))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public async Task UploadFileToAzureBlobStorage_OneDrive_Exists_ReturnsJsonResult()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "test12.pdf",
                Result = "exists"
            };
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
            _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");
            _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(uploadClientFileRequestViewModel.Result);


            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uploadClientFileRequestViewModel.Result, result.Value);
        }

        [Fact]
        public async Task UploadFileToAzureBlobStorage_OneDrive_ReturnsJsonResultAsInvalidExtension()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "test12.txt",
                Result = "invalidextension"
            };
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
            _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");
            _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(uploadClientFileRequestViewModel.Result);

            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uploadClientFileRequestViewModel.Result, result.Value);
        }

        [Fact]
        public async Task UploadFileToAzureBlobStorage_OneDrive_ReturnsJsonResultAsFailedExceptionThrows()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "test14.pdf",
                Result = "Failed"
            };
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("An error occurred while sending the request"));
            var httpClient = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://example.com/"),
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");
            _storageServiceMock
                .Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Throws(new Exception("An error occured while uploading file at azure blob storage"));

            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uploadClientFileRequestViewModel.Result, result.Value);
        }

        [Fact]
        public async Task UploadFile_InValidFile_Failed()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = false,
                FileName = "test14.pdf",
                Result = "Failed"
            };
            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns(uploadClientFileRequestViewModel.FileName);
            formFileMock.Setup(ff => ff.Length).Returns(100);


            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode); // Assuming you want to check the status code for BadRequest
        }

        [Fact]
        public async Task UploadFileToAzureBlobStorage_OneDrive_Failed_ReturnsJsonResult()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = true,
                FileURL = "http://example.com/file",
                FileName = "test12.pdf",
                Result = "failed"
            };
            var expectedResult = "error";
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(handler.Object);
            httpClient.BaseAddress = new Uri("http://example.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");

            _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(uploadClientFileRequestViewModel.Result);



            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public async Task UploadFile_ValidFile_Success()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = false,
                FileName = "test16.pdf",
                Result = "success"
            };

            var expectedResult = "success";

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns(uploadClientFileRequestViewModel.FileName);
            formFileMock.Setup(ff => ff.Length).Returns(100);

            // Simulate file content
            var fileContent = "This is the content of the example file.";
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            formFileMock.Setup(ff => ff.OpenReadStream()).Returns(contentStream);

            // Include the IFormFile in the request model
            uploadClientFileRequestViewModel.UploadedFile = formFileMock.Object;

            // Assuming "success" scenario for storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(expectedResult);

            _unitOfWorkMock.Setup(u => u.UploadClientFileDetailsRepo.UploadFileDetailsToDB(uploadClientFileRequestViewModel))
               .ReturnsAsync(expectedResult);



            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(expectedResult, jsonResult.Value);
        }

        //not working
        [Fact]
        public async Task UploadFile_ExceptionDuringStorageUpload_ReturnsErrorJsonResult()
        {
            // Arrange
            var uploadClientFileRequestViewModel = new UploadClientFileRequestViewModel
            {
                OrganizationId = "3",
                FacilityId = "3",
                CarrierName = "abc",
                IsOneDrive = false,
                FileName = "logo1.png",
                Result = "failed"
            };

            var expectedResult = "error"; // This is the expected result when an exception occurs during storage upload

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
                .Returns("taplatformwebstore01");

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(ff => ff.FileName).Returns(uploadClientFileRequestViewModel.FileName);
            formFileMock.Setup(ff => ff.Length).Returns(1); // Simulate an empty file

            // Simulate an empty file content
            var contentStream = new MemoryStream();
            formFileMock.Setup(ff => ff.OpenReadStream()).Returns(contentStream);

            // Include the IFormFile in the request model
            uploadClientFileRequestViewModel.UploadedFile = formFileMock.Object;

            // Simulate an exception during storage service upload
            _storageServiceMock.Setup(ss => ss.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new Exception("An error occured during storage upload at azure blob"));


            // Act
            var result = await _dataServiceController.UploadFileToAzureBlobStorage(uploadClientFileRequestViewModel) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(expectedResult, jsonResult.Value);
        }

        [Fact]
        public async Task DownloadFile_ReturnsFileResult()
        {
            // Arrange
            var fileName = "test16.pdf";
            var containerName = "taplatformwebstore01";

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
               .Returns(containerName);

            // Assume you have a method in IStorageService to get a file stream
            _storageServiceMock.Setup(s => s.GetFileAsync(containerName, fileName))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }));


            // Act
            var result = await _dataServiceController.DownloadFile(fileName) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(fileName, result.FileDownloadName);
        }

        [Fact]
        public async Task DownloadFile_ThrowsException_ReturnsNotFoundResult()
        {
            // Arrange
            var fileName = "nonexistentfile.pdf";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
              .Returns(containerName);

            // Assume you have a method in IStorageService to get a file stream
            _storageServiceMock.Setup(s => s.GetFileAsync(containerName, fileName))
                .ThrowsAsync(new Exception("File not found"));



            // Act
            var result = await _dataServiceController.DownloadFile(fileName) as NotFoundResult;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteFile_FileDeleted_ReturnsJsonResult()
        {
            // Arrange
            var fileName = "test16.pdf";
            var containerName = "taplatformwebstore01";

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.DeleteFileAsync(containerName, fileName))
                .ReturnsAsync(true);

            _unitOfWorkMock.Setup(u => u.UploadClientFileDetailsRepo.DeleteFile(fileName))
                .Returns(true);


            // Act
            var result = await _dataServiceController.DeleteFile(fileName) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.True((bool)result.Value);
        }

        [Fact]
        public async Task DeleteFile_FileNotDeleted_ReturnsJsonResult()
        {
            // Arrange
            var fileName = "nonexistentfile.pdf";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.DeleteFileAsync(containerName, fileName))
                .ReturnsAsync(false);

            // Act
            var result = await _dataServiceController.DeleteFile(fileName) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result.Value);
        }

        [Fact]
        public async Task DisplayFile_FileFound_ReturnsPDFFileResult()
        {
            // Arrange
            var fileName = "test12.pdf";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.GetFileContentAsync(containerName, fileName))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }));

            // Act
            var result = await _dataServiceController.DisplayFile(fileName) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal(fileName, result.FileDownloadName);
        }

        [Fact]
        public async Task DisplayFile_FileFound_ReturnsPNGFileResult()
        {
            // Arrange
            var fileName = "logo1.png";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.GetFileContentAsync(containerName, fileName))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }));

            // Act
            var result = await _dataServiceController.DisplayFile(fileName) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("image/png", result.ContentType);
            Assert.Equal(fileName, result.FileDownloadName);
        }

        [Fact]
        public async Task DisplayFile_FileFound_ReturnsJPGFileResult()
        {
            // Arrange
            var fileName = "latest.jpg";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.GetFileContentAsync(containerName, fileName))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }));

            // Act
            var result = await _dataServiceController.DisplayFile(fileName) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.Equal(fileName, result.FileDownloadName);
        }

        [Fact]
        public async Task DisplayFile_FileNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var fileName = "nonexistentfile.pdf";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.GetFileContentAsync(containerName, fileName))
                .ReturnsAsync((Stream)null); // Simulating file not found

            // Act
            var result = await _dataServiceController.DisplayFile(fileName) as NotFoundResult;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DisplayFile_FileNotFound_ReturnsNotFoundResultOnException()
        {
            // Arrange
            var fileName = "nonexistentfile.pdf";
            var containerName = "taplatformwebstore01"; // Set your container name

            _configuration.Setup(c => c.GetSection("AzureBlobStorageContainerName:ContainerName").Value)
             .Returns(containerName);

            _storageServiceMock.Setup(s => s.GetFileContentAsync(containerName, fileName))
                .Throws(new Exception("An error occured while getting file details")); // Simulating file not found


            // Act
            var result = await _dataServiceController.DisplayFile(fileName) as NotFoundResult;

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForAddDataService))]
        public async Task AddDataService_Test(DataServiceRequest request, string filename, DataServiceResponse response, bool ValidAccNo, bool isSuccess, bool IsCheckFileName, bool IsAccountExist)
        {
            var fileMock = new Mock<IFormFile>();
            if (filename != null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string projectRootPath = GetProjectRootPath(assembly);
                string filePath = Path.Combine(projectRootPath, "PBJSnap/UploadFiles/" + filename);
                if (File.Exists(filePath))
                {
                    //file exist
                }
                var fileStream = System.IO.File.OpenRead(filePath);

                using var _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                _fileStream.Position = 0;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var fileName = Path.GetFileName(filePath);
                fileMock.Setup(_ => _.FileName).Returns(fileName);
                fileMock.Setup(_ => _.Length).Returns(_fileStream.Length);
                fileMock.Setup(_ => _.OpenReadStream()).Returns(_fileStream);
                fileMock.Setup(_ => _.ContentDisposition).Returns(string.Format("inline; filename={0}", fileName));
            }

            _unitOfWorkMock.Setup(u => u.DataServiceRepo.CheckAccountNumber(It.IsAny<CheckAccountNumberRequest>())).ReturnsAsync(ValidAccNo);
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.CheckFileName(It.IsAny<CheckDataServiceFileName>())).ReturnsAsync(IsCheckFileName);
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.AddDataService(It.IsAny<DomainModels.Entities.DataService>())).ReturnsAsync(isSuccess);
            _configuration.Setup(c => c.GetSection(It.IsAny<String>())).Returns(new Mock<IConfigurationSection>().Object);

            var result = filename == null
    ? await _dataServiceController.AddDataService(request, null)
    : await _dataServiceController.AddDataService(request, fileMock.Object);

            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dsResponse = Assert.IsType<DataServiceResponse>(jsonResult.Value);

            Assert.Equal(dsResponse.StatusCode, response.StatusCode);

        }

        [Theory]
        [MemberData(nameof(MockTestDataForDeleteDataService))]
        public async Task DeleteDataService_Test(string Id, bool isTrue, int statusCode)
        {
            int code = 0;
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.DeleteDataService(Id)).ReturnsAsync(isTrue);
            var result = await _dataServiceController.DeleteDataService(Id);
            Assert.NotNull(result);
            if (isTrue)
            {
                code = Assert.IsType<OkResult>(result).StatusCode;
            }
            else
            {
                code = Assert.IsType<NotFoundResult>(result).StatusCode;
            }
            Assert.Equal(code, statusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForGetDataServiceById))]
        public async Task GetDataServiceById_Test(string Id)
        {
            DataServiceRequest data = new DataServiceRequest();
            data.Id = Id;
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.GetDataServiceById(Id)).ReturnsAsync(data);
            var result = await _dataServiceController.GetDataServiceById(Id);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            DataServiceRequest response = Assert.IsType<DataServiceRequest>(jsonResult.Value);
            if (Id == "")
                Assert.Null(response.Id);
            else
                Assert.Equal(response.Id, data.Id);

        }

        [Fact]
        public async Task GetDataServiceById_TestThrowsException()
        {
            DataServiceRequest data = new DataServiceRequest
            {
                Id = "1"
            };

            var statusCode = 404;
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.GetDataServiceById(data.Id)).Throws(new Exception("An error occured while getting service details."));

            var result = await _dataServiceController.GetDataServiceById(data.Id);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(statusCode, jsonResult.StatusCode);
        }

        [Fact]
        public async Task DeleteDataServiceByIdThrowsException()
        {
            DataServiceRequest data = new DataServiceRequest
            {
                Id = "1"
            };

            var statusCode = 404;
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.DeleteDataService(data.Id)).Throws(new Exception("An error occured while deleting data service details."));

            var result = await _dataServiceController.DeleteDataService(data.Id);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(statusCode, jsonResult.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForGetDataServiceList))]
        public async Task GetDataServiceList_Test(DataServiceListRequest Request, string searchValue, string start, string length, string sortColumn, string sortDirection, int draw)
        {
            List<FacilityDataServiceModel> list = new List<FacilityDataServiceModel>();
            list.Add(new FacilityDataServiceModel()
            {
                Id = 1,
                FacilityName = "Rest",
                FacilityID = 1,
                LocationId = "111",
                AccountNumber = "1234"
            });
            int total = list.Count();
            _unitOfWorkMock.Setup(u => u.DataServiceRepo.GetDataServiceList(It.IsAny<GetDataServiceListRequest>(), out total)).Returns(list);
            var result = await _dataServiceController.GetDataServiceList(Request, searchValue, start, length, sortColumn, sortDirection, draw);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(total, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewWithData()
        {
            // Arrange
            var testData = new List<FacilityLocationAccess>
            {
                new FacilityLocationAccess
                {
                OrganizationID = 1,
                OrganizationName = "OrganizationOne",
                MasterAccountId = "5009107",
                SubAccountId = "5009107",
                FacilityID = 1,
                FacilityName = "DemoFacilityTwo",
                LocationId = "withtele"
                }
            };

            var serviceList = new List<DomainModels.Entities.DataServiceType>
            {
                new DomainModels.Entities.DataServiceType
                {
                    Id=1,
                    ServiceName="Demo",
                    IsActive=true
                },
                new DomainModels.Entities.DataServiceType
                {
                    Id=1,
                    ServiceName="abc",
                    IsActive=true
                }
            };

            _unitOfWorkMock.Setup(x => x.FacilityRepo.GetAllFacilitiesLocationIdByOrgId()).ReturnsAsync(testData);
            _unitOfWorkMock.Setup(x => x.DataServiceTypeRepo.GetAll()).Returns(serviceList);

            // Act
            var result = await _dataServiceController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DataServiceModel>(result.Model);

            var model = result.Model as DataServiceModel;
            Assert.NotNull(model);
            Assert.Equal(testData, model.facilityLocationAccesses);

            for (int i = 0; i < testData.Count; i++)
            {
                Assert.Equal(serviceList[i].ServiceName, model.ServiceList[i].Text);
                Assert.Equal(serviceList[i].Id.ToString(), model.ServiceList[i].Value);
            }
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewWithDataThorowsException()
        {
            // Arrange
            var testData = new List<FacilityLocationAccess>
            {
                new FacilityLocationAccess
                {
                OrganizationID = 1,
                OrganizationName = "OrganizationOne",
                MasterAccountId = "5009107",
                SubAccountId = "5009107",
                FacilityID = 1,
                FacilityName = "DemoFacilityTwo",
                LocationId = "withtele"
                }
            };

            var serviceList = new List<DomainModels.Entities.DataServiceType>
            {
                new DomainModels.Entities.DataServiceType
                {
                    Id=1,
                    ServiceName="Demo",
                    IsActive=true
                },
                new DomainModels.Entities.DataServiceType
                {
                    Id=1,
                    ServiceName="abc",
                    IsActive=true
                }
            };

            _unitOfWorkMock.Setup(x => x.FacilityRepo.GetAllFacilitiesLocationIdByOrgId()).Throws(new Exception("An error occured while getting facilities by location id."));

            // Act
            var result = await _dataServiceController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DataServiceModel>(result.Model);

            var model = result.Model as DataServiceModel;
            Assert.NotNull(model);
            Assert.Empty(model.facilityLocationAccesses);
            Assert.Empty(model.ServiceList);
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewWithDataThorowsExceptionInServiceList()
        {
            // Arrange
            var testData = new List<FacilityLocationAccess>
            {
                new FacilityLocationAccess
                {
                OrganizationID = 1,
                OrganizationName = "OrganizationOne",
                MasterAccountId = "5009107",
                SubAccountId = "5009107",
                FacilityID = 1,
                FacilityName = "DemoFacilityTwo",
                LocationId = "withtele"
                }
            };

            _unitOfWorkMock.Setup(x => x.FacilityRepo.GetAllFacilitiesLocationIdByOrgId()).ReturnsAsync(testData);
            _unitOfWorkMock.Setup(x => x.DataServiceTypeRepo.GetAll()).Throws(new Exception("An error occured while getting service list."));


            // Act
            var result = await _dataServiceController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DataServiceModel>(result.Model);

            var model = result.Model as DataServiceModel;
            Assert.NotNull(model);
            Assert.Equal(testData, model.facilityLocationAccesses);
            Assert.Empty(model.ServiceList);
        }

        private static string GetProjectRootPath(Assembly assembly)
        {
            string assemblyLocation = assembly.Location;
            string projectRootPath = Path.GetFullPath(Path.Combine(assemblyLocation, "../../../../"));
            return projectRootPath;
        }

        //Object Creation
        public static IEnumerable<object[]> MockTestDataForAddDataService()
        {
            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
   }, null,new DataServiceResponse { StatusCode=1003, Message="SUCCESS" } ,false,true,false,false};

            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
   }, null,new DataServiceResponse { StatusCode=1004, Message="FAILED" } ,false,false,false,false};

            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
   }, "Limestone_outptil1 idghtDivid.csv",new DataServiceResponse { StatusCode=1001, Message="INVALID_FILE" },false ,true,false,false};

            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
   }, "blank.pdf",new DataServiceResponse { StatusCode=1002, Message="FILE_EXIST" },false,true,true,false };

            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
  }, null,new DataServiceResponse { StatusCode=1005, Message="ACCOUNTNUMBER_EXIST" },true,true,false,true };

            yield return new object[] { new DataServiceRequest { AccountNumber = "1000",ProviderName="TestData",Id = "0",FacilityId = "1",LocalIP = "10.2.12",ServicesId = 1,SupportNumber = "1000000000",ContractDocument="",ContractEndDate=DateTime.Now,ContractStatus= true
   }, "blank.pdf",new DataServiceResponse { StatusCode=1006, Message="UPLOAD_FILE_ERROR" },false,true,false,false };



        }

        public static IEnumerable<object[]> MockTestDataForDeleteDataService()
        {
            yield return new object[] { "1", true, 200 };

            yield return new object[] { "", false, 404 };
        }

        public static IEnumerable<object[]> MockTestDataForGetDataServiceById()
        {
            yield return new object[] { "1" };
            yield return new object[] { "" };
        }

        public static IEnumerable<object[]> MockTestDataForGetDataServiceList()
        {
            yield return new object[] { new DataServiceListRequest { facilityId = "1", serviceId = "0" }, "", "0", "0", "", "", 1 };

        }
    }
}
