using AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TAN.UnitTest.Helpers;
using TANWeb.Controllers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Helpers.Permissions;


namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class HistoryControllerMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly HistoryController _historyController;
        private readonly Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<HttpContext> _mockHttpContext;
        public HistoryControllerMock()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManager = GetUserManagerMock();
            _mockHttpContext = new Mock<HttpContext>();
            _historyController = new HistoryController(_unitOfWorkMock.Object, _userManager.Object);
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
        public async Task GetFileDetails_ValidMock()
        {
            // Arrange
            var fileName = "DemoFacility20230721180730.xml";

            var expectedSubmissionHistoryViewModel = new SubmissionHistoryViewModel() { Id = 1, FileName = fileName, Status = 0, CreateDate = DateTime.UtcNow };

            _unitOfWorkMock.Setup(h => h.HistoryRepo.GetDetails(fileName)).ReturnsAsync(expectedSubmissionHistoryViewModel);

            // Act
            var result = await _historyController.GetFileDetails(fileName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockSubmissionHistoryViewModell = Assert.IsType<SubmissionHistoryViewModel>(jsonResult.Value);
            Assert.NotNull(mockSubmissionHistoryViewModell);
            Assert.Equal(fileName, mockSubmissionHistoryViewModell.FileName);
        }

        [Fact]
        public async Task GetFileDetails_InValidMock()
        {
            // Arrange
            var fileName = "";
            var expectedSubmissionHistoryViewModel = new SubmissionHistoryViewModel();
            _unitOfWorkMock.Setup(h => h.HistoryRepo.GetDetails(fileName)).ReturnsAsync(expectedSubmissionHistoryViewModel);
            // Act
            var result = await _historyController.GetFileDetails(fileName);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockSubmissionHistoryViewModel = Assert.IsType<SubmissionHistoryViewModel>(jsonResult.Value);
            Assert.NotNull(mockSubmissionHistoryViewModel);
        }

        [Fact]
        public async Task UpdateSubmissionData_Should_Return_JsonResult()
        {
            // Arrange
            string fileId = "10";
            string? fileDate = DateTime.UtcNow.ToString();
            string fileName = "DemoFacility20230721180730.xml";
            string fileStatus = "Accepted";
            var expectedResult = "success";
            _unitOfWorkMock.Setup(h => h.HistoryRepo.UpdateSubmissionStatus(fileId, fileDate, fileName, fileStatus)).ReturnsAsync(expectedResult);
            // Act
            var result = await _historyController.UpdateSubmissionData(fileId, fileDate.ToString(), fileName, fileStatus);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }
        [Fact]
        public async Task UpdateSubmissionData_Should_Return_JsonResult_Failed()
        {
            // Arrange
            string fileId = "123";
            string? fileDate = DateTime.UtcNow.ToString();
            string fileName = "DemoFacility20230721180730.xml";
            string fileStatus = "Accepted";
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(h => h.HistoryRepo.UpdateSubmissionStatus(fileId, fileDate, fileName, fileStatus)).ReturnsAsync(expectedResult);
            // Act
            var result = await _historyController.UpdateSubmissionData(fileId, fileDate.ToString(), fileName, fileStatus);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteSubmissionHistory_Should_Return_JsonResult_Sucess()
        {
            // Arrange
            string fileName = "DemoFacility20230721180730.xml";
            var expectedResult = "success";
            _unitOfWorkMock.Setup(h => h.HistoryRepo.DeleteSubmissionHistoryDetail(fileName)).ReturnsAsync(expectedResult);
            // Act
            var result = await _historyController.DeleteSubmissionHistory(fileName);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteSubmissionHistory_Should_Return_JsonResult_Failed()
        {
            // Arrange
            string fileName = "";
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(h => h.HistoryRepo.DeleteSubmissionHistoryDetail(fileName)).ReturnsAsync(expectedResult);
            // Act
            var result = await _historyController.DeleteSubmissionHistory(fileName);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public void DownloadFile_Should_Return_FileContentResult()
        {
            // Arrange
            string fileName = "DemoFacility20230724130786";
            var xmlContent = "<nursingHomeData>\r\n  <header fileSpecVersion=\"4.00.0\">\r\n    <facilityId>45564</facilityId>\r\n    <stateCode>AK</stateCode>\r\n    <reportQuarter>2</reportQuarter>\r\n    <federalFiscalYear>2023</federalFiscalYear>\r\n    <softwareVendorName>Think Anew</softwareVendorName>\r\n    <softwareVendorEmail>sales@pbjsnap.com</softwareVendorEmail>\r\n    <softwareProductName>PBJSNAP</softwareProductName>\r\n    <softwareProductVersion>1.4.2</softwareProductVersion>\r\n  </header>\r\n  <employees />\r\n  <staffingHours processType=\"merge\" />\r\n</nursingHomeData>";
            // Set up the mock repository to return the test data
            _unitOfWorkMock.Setup(h => h.UploadHistoryRepo.GetAll())
                .Returns(new List<UploadHistory>
                {
                new UploadHistory { FileName = "DemoFacility20230724130786",UploadXMLFile = xmlContent }
                });
            var result = _historyController.DownloadFile(fileName);
            // Assert
            // Ensure that the result is of type FileContentResult
            Assert.IsType<FileContentResult>(result);
            // Check the content type and file name
            Assert.Equal("application/zip", result.ContentType);
            Assert.Equal(fileName + ".zip", result.FileDownloadName);
            // Verify the file content inside the zip
            using (MemoryStream zipStream = new MemoryStream(result.FileContents))
            using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                var zipEntry = zipArchive.GetEntry(fileName + ".xml");
                using (StreamReader reader = new StreamReader(zipEntry.Open(), Encoding.UTF8))
                {
                    string xmlContentInZip = reader.ReadToEnd();
                    Assert.Equal(xmlContent, xmlContentInZip);
                }
            }
        }

        [Fact]
        public void DownloadFile_Should_Return_Error_FileContentResult_On_Exception()
        {

            // Test data
            string fileName = "nonexistentfile.txt"; // File name that doesn't exist
            _unitOfWorkMock.Setup(h => h.UploadHistoryRepo.GetAll())
                .Returns(new List<UploadHistory>()); // Empty list to simulate file not found
            var result = _historyController.DownloadFile(fileName);
            // Assert
            // Ensure that the result is of type FileContentResult
            Assert.IsType<FileContentResult>(result);
            // Check the content type and file name
            Assert.Equal("application/zip", result.ContentType);
            Assert.Equal("error.zip", result.FileDownloadName);
        }

        [Fact]
        public async Task GetFacilities_Should_Return_JsonResult_With_FacilityList()
        {
            // Arrange
            // Test data
            string organizationName = "OrganizationOne1";
            int organizationId = 1; // Replace with a valid organization ID
            var organizationDetails = new OrganizationModel { OrganizationID = organizationId, OrganizationName = organizationName };
            var facilityList = new List<FacilityModel>
        {
            new FacilityModel { Id = 10, FacilityName = "DemoFacility", OrganizationId = organizationId, IsActive = true },
            new FacilityModel { Id = 11, FacilityName = "DemoFacility1", OrganizationId = organizationId, IsActive = true }
        };
            // Set up the mock repositories to return the test data
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.GetAll())
                .Returns(new List<OrganizationModel> { organizationDetails });
            _unitOfWorkMock.Setup(f => f.FacilityRepo.GetAllFacilitiesByOrgId(organizationId))
                .Returns(Task.FromResult(facilityList));
            // Act
            var result = await _historyController.GetFacilities(organizationName);
            // Assert
            // Ensure that the result is of type JsonResult
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<FacilityModel>>(jsonResult.Value);
            Assert.NotNull(jsonResult);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count()); // Number of facilities in the test data
            // Verify the data in the JsonResult
            var facilityNames = model.Select(item => item.FacilityName).ToList();
            var facilityIds = model.Select(item => item.Id.ToString()).ToList();
            Assert.Contains("DemoFacility", facilityNames);
            Assert.Contains("DemoFacility1", facilityNames);
            Assert.Contains("10", facilityIds);
            Assert.Contains("11", facilityIds);
        }

        [Fact]
        public async Task GetFacilities_Should_Return_Null_JsonResult_For_NonExisting_Organization()
        {
            // Arrange
            // Test data
            string organizationName = "NonExistingOrganization";
            // Set up the mock organization repository to return an empty list, simulating non-existing organization
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.GetAll())
                .Returns(new List<OrganizationModel>());
            // Act
            var result = await _historyController.GetFacilities(organizationName);
            // Assert
            // Ensure that the result is of type JsonResult
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<FacilityModel>>(jsonResult.Value);
            Assert.NotNull(jsonResult);
            Assert.NotNull(model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_ReturnsViewWithViewModelData()
        {
            // Arrange
            const string selectedApp = "PBJSnap";
            var organizations = new List<Itemlist>
        {
            new Itemlist { Value = 1, Text = "OrganizationOne1"},
            new Itemlist { Value = 2, Text = "OrganizationTwo" }
        };
            // Test data for facilities
            var facilities = new List<FacilityModel>
        {
            new FacilityModel { Id = 10, FacilityName = "DemoFacility", IsActive = true },
            new FacilityModel { Id = 11, FacilityName = "DemoFacility2", IsActive = true }
        };

            var sessionData = new Dictionary<string, byte[]>
    {
        { "SelectedApp", Encoding.UTF8.GetBytes("PBJSnap") }
    };

            // Create a mock HttpContext and set up the behavior
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.Session)
                .Returns(new MockHttpSession(sessionData));

            // Set the mock HttpContext for the controller
            _historyController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            _unitOfWorkMock.Setup(u => u.OrganizationRepo.GetApplicationWiseOrganizations(selectedApp))
                    .ReturnsAsync(organizations);
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(facilities);
            var mockUser = new AspNetUser { UserType = 1 };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Act
            var result = await _historyController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionHistoryViewModel>(viewResult.ViewData.Model);
            Assert.Null(model.OrganizationList);
            Assert.Equal(facilities, model.FacilityList);
            Assert.Equal(1, mockUser.UserType);
        }

        [Fact]
        public void GetSubmissionHistory_ReturnsJsonResult()
        {
            // Arrange
            string orgName = "";
            string facilityName = "";
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "Date"; // Replace with your desired column name
            string sortDirection = "asc"; // Replace with your desired sort direction
            string draw = "0";
            // Mock the repository method GetAllSubmissionHistory
            var submissionHistoryData = new SubmissionHistoryViewModel[]
            {
                new SubmissionHistoryViewModel { Id=8,FileName="DemoFacility120230721160776.xml",FacilityId=11,Status = 1, IsActive = true,SubmissionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),OrganizationId = 1,FacilityName = "DemoFacility",OrganizationName="OrganizationOne1" },
                new SubmissionHistoryViewModel { Id=9,FileName="DemoFacility20230721180730.xml",FacilityId=10,Status = 1, IsActive = false,SubmissionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),OrganizationId = 1,FacilityName = "DemoFacility1",OrganizationName="OrganizationOne1" },
            };
            int submissionCount = submissionHistoryData.Length;
            _unitOfWorkMock.Setup(h => h.HistoryRepo.GetAllSubmissionHistory(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), out submissionCount))
                .Returns(submissionHistoryData);
            // Act
            var result = _historyController.GetSubmissionHistory(orgName, facilityName, searchValue, start, length, sortColumn, sortDirection, draw) as JsonResult;
            // Assert
            Assert.NotNull(result);
            var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(Convert.ToInt32(draw), (int)jsonData.draw);
            Assert.Equal(submissionCount, (int)jsonData.recordsTotal);
            Assert.Equal(submissionCount, (int)jsonData.recordsFiltered);
            Assert.NotNull(jsonData.data);
            Assert.Equal(submissionHistoryData.Length, jsonData.data.Count);
        }
    }
}
