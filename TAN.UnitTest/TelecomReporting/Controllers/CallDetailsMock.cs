using AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Moq;
using NuGet.ContentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Areas.TelecomReporting.Controllers;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.TelecomReporting.Controllers
{
    public class CallDetailsMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CallDetailsController _callDetailsController;
        private readonly Mock<UserManager<AspNetUser>> _userManager;

        public CallDetailsMock()
        {
            _userManager = GetUserManagerMock();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _callDetailsController = new CallDetailsController(_unitOfWorkMock.Object, _userManager.Object);
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
        public async Task CostAnalysisReport_ReturnsViewWithModel()
        {
            // Arrange
            var organizations = new List<OrganizationModel> { new OrganizationModel { IsActive = true, OrganizationID = 2, OrganizationName = "Org1" } }.AsQueryable();
            var organizationServices = new List<OrganizationServices> { new OrganizationServices { IsActive = true, OrganizationId = 2, ServiceType = 0 } }.AsQueryable();
            var organizationRepoMock = new Mock<IOrganizationRepository>();
            var bandwidthCallEventsRepoMock = new Mock<ICalls>();
            var organizationServiceRepoMock = new Mock<IOrganizationServiceRepository>();

            _unitOfWorkMock.Setup(u => u.OrganizationRepo).Returns(organizationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BandwidthCallEventsRepo).Returns(bandwidthCallEventsRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.OrganizationServiceRepo).Returns(organizationServiceRepoMock.Object);
            organizationRepoMock.Setup(repo => repo.GetAll())
                .Returns(organizations);
            bandwidthCallEventsRepoMock.Setup(repo => repo.GetLoggedInUserInfo())
                .Returns(true); // or false depending on the test case
            organizationServiceRepoMock.Setup(repo => repo.GetAll())
                .Returns(organizationServices);
            var mockUser = new AspNetUser { Id = "ce658637-9dc8-4c78-a81e-db4ee4058971", FirstName = "Krunal", UserType = 1 };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            var controller = new CallDetailsController(_unitOfWorkMock.Object, _userManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            // Act
            var result = await controller.CostAnalysisReport() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CostAnalysisViewModel>(result.Model);

            var model = result.Model as CostAnalysisViewModel;

            // Add more assertions based on your requirements
            Assert.NotNull(model.CallDirections);
            Assert.NotEmpty(model.CallDirections);

            Assert.NotNull(model.CallTypes);
            Assert.NotEmpty(model.CallTypes);

            Assert.NotNull(model.CallResults);
            Assert.NotEmpty(model.CallResults);

            Assert.NotNull(model.Organizations);
            Assert.NotEmpty(model.Organizations);
            Assert.Equal(model.Organizations.Count, organizations.Count());
        }

        [Fact]
        public async Task CostAnalysisReport_ReturnsViewWithModelForOrganizationUser()
        {
            // Arrange
            var organizationId = "2";
            var organizations = new List<OrganizationModel> { new OrganizationModel { IsActive = true, OrganizationID = 2, OrganizationName = "Org1" } }.AsQueryable();
            var organizationServices = new List<OrganizationServices> { new OrganizationServices { IsActive = true, OrganizationId = 2, ServiceType = 0 } }.AsQueryable();
            var organizationRepoMock = new Mock<IOrganizationRepository>();
            var bandwidthCallEventsRepoMock = new Mock<ICalls>();
            var organizationServiceRepoMock = new Mock<IOrganizationServiceRepository>();

            _unitOfWorkMock.Setup(u => u.OrganizationRepo).Returns(organizationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BandwidthCallEventsRepo).Returns(bandwidthCallEventsRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.OrganizationServiceRepo).Returns(organizationServiceRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.BandwidthCallEventsRepo.GetOrganizationUserDetails()).Returns(organizationId);
            organizationRepoMock.Setup(repo => repo.GetAll())
                .Returns(organizations);
            bandwidthCallEventsRepoMock.Setup(repo => repo.GetLoggedInUserInfo())
                .Returns(false); // or false depending on the test case
            organizationServiceRepoMock.Setup(repo => repo.GetAll())
                .Returns(organizationServices);
            var mockUser = new AspNetUser { Id = "ce658637-9dc8-4c78-a81e-db4ee4058971", FirstName = "Krunal", UserType = 1 };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            var controller = new CallDetailsController(_unitOfWorkMock.Object, _userManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            // Act
            var result = await controller.CostAnalysisReport() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CostAnalysisViewModel>(result.Model);

            var model = result.Model as CostAnalysisViewModel;

            // Add more assertions based on your requirements
            Assert.NotNull(model.CallDirections);
            Assert.NotEmpty(model.CallDirections);

            Assert.NotNull(model.CallTypes);
            Assert.NotEmpty(model.CallTypes);

            Assert.NotNull(model.CallResults);
            Assert.NotEmpty(model.CallResults);

            Assert.NotNull(model.Organizations);
            Assert.NotEmpty(model.Organizations);
            Assert.Equal(model.Organizations.Count, organizations.Count());
        }

        [Fact]
        public void GetCostAnalysisReportList_Returns_JsonResult_With_Correct_Data()
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
            int callCount = 10;
            IEnumerable<CostAnalysisViewModel> data = new List<CostAnalysisViewModel>
            {
                new CostAnalysisViewModel
                {
                        AccountId="5009107",
                        SubAccount="98953",
                        StartTime=DateTime.UtcNow.ToString(),
                        EndTime=DateTime.UtcNow.ToString(),
                        Duration="40690",
                        CallingNumber="+17158282769",
                        CalledNumber="+17154716366",
                        CallDirection="inbound",
                        CallType="local",
                        Cost="0.004900",
                        CallResult="completed"
                },
                new CostAnalysisViewModel
                {
                        AccountId="5009107",
                        SubAccount="98953",
                        StartTime=DateTime.UtcNow.ToString(),
                        EndTime=DateTime.UtcNow.ToString(),
                        Duration="",
                        CallingNumber="+17344747393",
                        CalledNumber="+17154716366",
                        CallDirection="inbound",
                        CallType="interstate",
                        Cost="",
                        CallResult="incomplete"
                }
            };
            _unitOfWorkMock.Setup(uow => uow.BandwidthCallEventsRepo.GetAllCallsCostAnalysis(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out callCount))
                    .Returns(data);
            // Act
            var result = _callDetailsController.GetCostAnalysisReportList(searchValue, start, length, sortColumn, sortDirection, draw, "", "", "", "");
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<CostAnalysisViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public void ExportCostSummaryToCsv_ReturnsFileResult()
        {
            // Arrange
            var costSummaryRequest = new CostSummaryCsvRequest
            {
                OrganizationName = "",
                FileName = "DemoFile",
                CallDirection = "",
                CallType = "",
                CallResult = "",
                SearchedValue = ""
            };

            var bandwidthCallEventsRepoMock = new Mock<ICalls>();
            _unitOfWorkMock.Setup(u => u.BandwidthCallEventsRepo).Returns(bandwidthCallEventsRepoMock.Object);

            var costSummaryDataTable = new DataTable();

            // Add columns to the DataTable as needed for testing
            costSummaryDataTable.Columns.Add("Column1", typeof(string));
            costSummaryDataTable.Columns.Add("Column2", typeof(int));
            costSummaryDataTable.Rows.Add("Value1", 123);
            costSummaryDataTable.Rows.Add("Value2", 456);
            bandwidthCallEventsRepoMock.Setup(repo => repo.GetCostSummaryByOrganization(costSummaryRequest))
                .Returns(costSummaryDataTable);
            // Act
            var result = _callDetailsController.ExportCostSummaryToCsv(costSummaryRequest) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileContentResult>(result);

            var fileContentResult = result as FileContentResult;

            Assert.Equal("text/csv", fileContentResult.ContentType);
            Assert.Equal(costSummaryRequest.FileName + ".csv", fileContentResult.FileDownloadName);
            costSummaryDataTable.Dispose();
        }

        [Fact]
        public void ExportCostSummaryToCsv_ReturnsNonExistingFileExceptionThorows()
        {
            // Arrange
            var costSummaryRequest = new CostSummaryCsvRequest
            {
                OrganizationName = "",
                FileName = "DemoFile",
                CallDirection = "",
                CallType = "",
                CallResult = "",
                SearchedValue = ""
            };

            var bandwidthCallEventsRepoMock = new Mock<ICalls>();
            _unitOfWorkMock.Setup(u => u.BandwidthCallEventsRepo).Returns(bandwidthCallEventsRepoMock.Object);

            var costSummaryDataTable = new DataTable();

            // Add columns to the DataTable as needed for testing
            costSummaryDataTable.Columns.Add("Column1", typeof(string));
            costSummaryDataTable.Columns.Add("Column2", typeof(int));
            costSummaryDataTable.Rows.Add("Value1", 123);
            costSummaryDataTable.Rows.Add("Value2", 456);
            bandwidthCallEventsRepoMock.Setup(repo => repo.GetCostSummaryByOrganization(costSummaryRequest))
                .Throws(new Exception("An error occured while getting datatable for exporting"));
            // Act
            var result = _callDetailsController.ExportCostSummaryToCsv(costSummaryRequest) as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileContentResult>(result);

            var fileContentResult = result as FileContentResult;

            Assert.Equal("text/csv", fileContentResult.ContentType);
            Assert.Equal(costSummaryRequest.FileName + ".csv", fileContentResult.FileDownloadName);
            costSummaryDataTable.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsViewWithModel()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.OrganizationRepo.GetBandwidthAccessList())
                          .ReturnsAsync(new List<BandwidthAccessList>()); // Adjust as needed
            var user = new AspNetUser { Id = "ad2f8671-ad61-4427-ae16-312c0c4ab792", FirstName = "Krunal", UserType = 1 };
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            var controller = new CallDetailsController(_unitOfWorkMock.Object, _userManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BandwidthCallSearchViewModel>(viewResult.Model);

            // Add more assertions based on your specific requirements
            Assert.NotNull(model.CallDirection);
            Assert.NotNull(model.CallType);
            Assert.NotNull(model.CallResult);
            Assert.NotNull(model.HangUpSource);
            Assert.Equal(user.UserType, model.UserType);
            Assert.NotNull(model.bandwidthAccessLists);
        }

        [Fact]
        public async Task Index_ReturnsViewWithModelThrowsException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.OrganizationRepo.GetBandwidthAccessList())
                          .Throws(new Exception("An error occured while getting bandwidth access list"));
            var user = new AspNetUser { Id = "ad2f8671-ad61-4427-ae16-312c0c4ab792", FirstName = "Krunal", UserType = 1 };
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            var controller = new CallDetailsController(_unitOfWorkMock.Object, _userManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BandwidthCallSearchViewModel>(viewResult.Model);

            // Add more assertions based on your specific requirements
            Assert.NotNull(model.CallDirection);
            Assert.NotNull(model.CallType);
            Assert.NotNull(model.CallResult);
            Assert.NotNull(model.HangUpSource);
            Assert.Equal(user.UserType, model.UserType);
        }

        [Fact]
        public async Task GetCallDetailsData_ValidModelMock()
        {
            //Arrange 
            string searchValue = "";
            string start = "0";
            string length = "10";
            int draw = 1;
            string sortColumn = "organizationName";
            string sortdirection = "asc";
            int bandwidthCallCount = 10;
            int totalRecord = 10;
            int filterRecord = 10;

            IEnumerable<BandwidthCallEventResponse> data = new List<BandwidthCallEventResponse>
            {
                new BandwidthCallEventResponse {OrganizationName="DemoOrgVoice",StartTime=DateTime.UtcNow.ToString(),EndTime=DateTime.UtcNow.ToString(),Duration="00:01:42",CallingNumber="+17152715739",CalledNumber="+17154716366",CallDirection="Inbound",CallType="Local",CallResult="Completed"},
                new BandwidthCallEventResponse {OrganizationName="DemoOrgVoice1",StartTime=DateTime.UtcNow.ToString(),EndTime=DateTime.UtcNow.ToString(),Duration="00:00:32",CallingNumber="+17155795349",CalledNumber="+17154716366",CallDirection="Inbound",CallType="Local",CallResult="Completed"},
            };
            _unitOfWorkMock.Setup(uow => uow.BandwidthCallEventsRepo.CallDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out bandwidthCallCount, It.IsAny<BandWidthCallFilterViewModel>()))
                .Returns(data);

            //Act
            var result = _callDetailsController.GetCallDetailsData(searchValue, start, length, draw, sortColumn, sortdirection, null);

            //Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<BandwidthCallEventResponse>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetCallDetailsData_Throw_Exception_Mock()
        {
            //Arrange 
            string searchValue = "";
            string start = "0";
            string length = "10";
            int draw = 1;
            string sortColumn = "organizationName";
            string sortdirection = "asc";
            int bandwidthCallCount = 0;
            int totalRecord = 0;
            int filterRecord = 0;
            var mockBandwidthCallEventsRepo = new Mock<ICalls>();
            var callDetails = new List<BandwidthCallEventResponse>();

            IEnumerable<BandwidthCallEventResponse> data = new List<BandwidthCallEventResponse>
            {
                new BandwidthCallEventResponse {OrganizationName="DemoOrgVoice",StartTime=DateTime.UtcNow.ToString(),EndTime=DateTime.UtcNow.ToString(),Duration="00:01:42",CallingNumber="+17152715739",CalledNumber="+17154716366",CallDirection="Inbound",CallType="Local",CallResult="Completed"},
                new BandwidthCallEventResponse {OrganizationName="DemoOrgVoice1",StartTime=DateTime.UtcNow.ToString(),EndTime=DateTime.UtcNow.ToString(),Duration="00:00:32",CallingNumber="+17155795349",CalledNumber="+17154716366",CallDirection="Inbound",CallType="Local",CallResult="Completed"},
            };
            mockBandwidthCallEventsRepo.Setup(uow => uow.CallDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out bandwidthCallCount, It.IsAny<BandWidthCallFilterViewModel>()))
                .Throws(new Exception("An error occured while extracting call details data to view"));
            _unitOfWorkMock.Setup(uow => uow.BandwidthCallEventsRepo).Returns(mockBandwidthCallEventsRepo.Object);

            //Act
            var result = _callDetailsController.GetCallDetailsData(searchValue, start, length, draw, sortColumn, sortdirection, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<BandwidthCallEventResponse>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(callDetails.Count(), dataObj.Count());
        }


        [Fact]
        public void ExportCallDetailsToCsv_Returns_FileType()
        {
            //Arrange
            ExportCallHistoryRequest csvRequest = new ExportCallHistoryRequest()
            {
                OrganizationName = "Test Organization",
                CallDirection = "Inbound",
                CallDetails = "test",
                CallType = "local",
                CallResult = "completed",
                HangUpSource = "called_party",
                SearchValue = "",
                FileName = ""
            };
            _unitOfWorkMock.Setup(r => r.BandwidthCallEventsRepo.CallDetailsExportList(csvRequest))
            .Returns(ExportCsv());

            //Act
            var result = _callDetailsController.ExportCallDetails(csvRequest);

            //Assert
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
        }

        [Fact]
        public void ExportCallDetailsToCsv_Throws_Exception()
        {
            //Arrange
            ExportCallHistoryRequest csvRequest = new ExportCallHistoryRequest()
            {
                OrganizationName = "Test Organization",
                CallDirection = "Inbound",
                CallDetails = "test",
                CallType = "local",
                CallResult = "completed",
                HangUpSource = "called_party",
                SearchValue = "",
                FileName = ""
            };
            _unitOfWorkMock.Setup(r => r.BandwidthCallEventsRepo.CallDetailsExportList(csvRequest))
                .Throws(new Exception("An error occured while exporting call details data to csv"));

            //Act
            var result = _callDetailsController.ExportCallDetails(csvRequest);

            //Assert
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
        }

        public DataTable ExportCsv()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Clear();
            dataTable.Columns.Add(new DataColumn("OrganizationName"));
            dataTable.Columns.Add(new DataColumn("StartTime"));
            dataTable.Columns.Add(new DataColumn("EndTime"));
            dataTable.Columns.Add(new DataColumn("Duration"));
            dataTable.Columns.Add(new DataColumn("CallingNumber"));
            dataTable.Columns.Add(new DataColumn("CalledNumber"));
            dataTable.Columns.Add(new DataColumn("CallDirection"));
            dataTable.Columns.Add(new DataColumn("CallType"));
            dataTable.Columns.Add(new DataColumn("Cost"));
            dataTable.Columns.Add(new DataColumn("CallResult"));
            var dataRow = dataTable.NewRow();
            dataRow["OrganizationName"] = "abc";
            dataRow["StartTime"] = "bcd";
            dataRow["EndTime"] = "cde";
            dataRow["Duration"] = "def";
            dataRow["CallingNumber"] = "efg";
            dataRow["CalledNumber"] = "fgh";
            dataRow["CallDirection"] = "ghi";
            dataRow["CallType"] = "hij";
            dataRow["Cost"] = "ijk";
            dataRow["CallResult"] = "jkl";
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }

        [Fact]
        public async Task GetCallDetailsById_validMock()
        {
            // Arrange
            var callDetailsId = new BandwidthCallEventResponse { CallId = "121897429_132090747@216.82.227.122" };
            var expectedCallDetails = new BandwidthCallEventResponse
            {
                OrganizationName = "DemoOrgVoice",
                StartTime = DateTime.UtcNow.ToString(),
                EndTime = DateTime.UtcNow.ToString(),
                Duration = "00:01:42",
                CallingNumber = "+17152715739",
                CalledNumber = "+17154716366",
                CallDirection = "Inbound",
                CallType = "Local",
                CallResult = "Completed"
            };
            var mockBandwidthCallEventsRepo = new Mock<ICalls>();
            _unitOfWorkMock.Setup(c => c.BandwidthCallEventsRepo).Returns(mockBandwidthCallEventsRepo.Object);
            mockBandwidthCallEventsRepo.Setup(cd => cd.CallDetailsById(callDetailsId.CallId)).ReturnsAsync(expectedCallDetails);


            //Act 

            var result = _callDetailsController.GetCallDetailsById(callDetailsId.CallId);

            //Assert
            var jsonResult = Assert.IsType<JsonResult>(result.Result);
            var callDetails = Assert.IsType<BandwidthCallEventResponse>(jsonResult.Value);
            Assert.Equal(expectedCallDetails.CallId, callDetails.CallId);
        }

        [Fact]
        public async Task GetCallDetailsById_Throw_Exception_Mock()
        {

            // Arrange
            var callDetailsId = new BandwidthCallEventResponse { CallId = "121897429_132090747@216.82.227.122" };
            var expectedStatusCode = 404;

            var mockBandwidthCallEventsRepo = new Mock<ICalls>();
            _unitOfWorkMock.Setup(u => u.BandwidthCallEventsRepo).Returns(mockBandwidthCallEventsRepo.Object);
            mockBandwidthCallEventsRepo.Setup(repo => repo.CallDetailsById(callDetailsId.CallId)).Throws(new Exception("An error occured while getting call details by Id"));

            // Act
            var result = await _callDetailsController.GetCallDetailsById(callDetailsId.CallId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(expectedStatusCode, jsonResult.StatusCode);
        }
    }
}
