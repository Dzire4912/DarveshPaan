using Azure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Areas.PBJSnap.Controllers;
using TANWeb.Areas.PBJSnap.PBJSnapServices;
using TANWeb.Areas.PBJSnap.Controllers;
using TANWeb.Interface;
using Newtonsoft.Json.Linq;
using TANWeb.Areas.TelecomReporting.Controllers;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class ReviewControllerMock
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<UserManager<AspNetUser>> _userManagerMock;
        private Mock<ClaimsPrincipal> _claimsMock;
        private Mock<IPdfExportHelper> mockPdfExportHelper;
        private Mock<IUpload> _iUpload;
        private readonly ReviewController _reviewControllerMock;
        private readonly Mock<IDataProtectionProvider> _dataProtectionProviderMock;
        public ReviewControllerMock()
        {
            _claimsMock = new Mock<ClaimsPrincipal>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManagerMock = GetUserManagerMock();
            mockPdfExportHelper = new Mock<IPdfExportHelper>();
            _iUpload = new Mock<IUpload>();
            _dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "TestUserId")
        });
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _reviewControllerMock = new ReviewController(_unitOfWorkMock.Object, _userManagerMock.Object, mockPdfExportHelper.Object, _iUpload.Object, _dataProtectionProviderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
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
        public async Task LoadIndexPageReturnViewResult()
        {

            //Arrange
            var facilityId = "100";
            var fileId = "100";
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(new List<FacilityModel>());
            _unitOfWorkMock.Setup(u => u.PayTypeCodesRepo.GetAllPayTypeCodes()).ReturnsAsync(new List<PayTypeCodes>());
            _unitOfWorkMock.Setup(u => u.JobCodesRepo.GetAll()).Returns(new List<JobCodes>());
            var result = await _reviewControllerMock.Index(facilityId, fileId) as ViewResult;

            var viewModel = result?.Model as ReviewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);

        }

        [Fact]
        public async Task LoadIndexPage_Handles_Exception()
        {
            // Arrange
            var facilityId = "100";
            var fileId = "100";
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId())
                .ThrowsAsync(new Exception("An error occured in FacilityRepo to get facilities data"));
            _unitOfWorkMock.Setup(u => u.PayTypeCodesRepo.GetAllPayTypeCodes())
                .ThrowsAsync(new Exception("Test PayTypeCodesRepo Exception"));
            _unitOfWorkMock.Setup(u => u.JobCodesRepo.GetAll())
                .Throws(new Exception("Test JobCodesRepo Exception"));
            // Act
            var result = await _reviewControllerMock.Index(facilityId, fileId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewModel = ((ViewResult)result).Model as ReviewModel;
            Assert.Null(viewModel.FacilityList);
            Assert.Equal(0, viewModel.EmployeeDetailModel.employeePopupModel.FacilityList.Count);
            Assert.Equal(0, viewModel.EmployeeDetailModel.employeePopupModel.PayType.Count);
            Assert.Equal(0, viewModel.EmployeeDetailModel.employeePopupModel.JobCode.Count);
        }


        [Fact]
        public async Task AddCencus_ValidCensus_Success()
        {
            // Arrange

            Census cencus = new Census()
            {
                CensusId = 1,
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                Medicare = 10,
                Medicad = 10,
                Other = 10,
                CreateDate = DateTime.Now,
                CreateBy = "",
                UpdateDate = DateTime.Now,
                UpdateBy = "",
                IsActive = true,
            };

            _unitOfWorkMock.Setup(uow => uow.CensusRepo.AddCencus(cencus)).ReturnsAsync(true);

            // Act
            var result = await _reviewControllerMock.AddCencus(cencus);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task AddCencus_NullCensus_BadRequest()
        {
            Census cencus = null;
            // Act
            var result = await _reviewControllerMock.AddCencus(cencus);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async Task AddCencus_Throws_Exception()
        {

            Census cencus = new Census()
            {
                CensusId = 1,
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                Medicare = 10,
                Medicad = 10,
                Other = 10,
                CreateDate = DateTime.Now,
                CreateBy = "",
                UpdateDate = DateTime.Now,
                UpdateBy = "",
                IsActive = true,
            };

            _unitOfWorkMock.Setup(uow => uow.CensusRepo.AddCencus(cencus)).Throws(new Exception("An error occured while adding new census data"));

            // Act
            var result = await _reviewControllerMock.AddCencus(cencus);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Equal("Record failed to insert", response.Message);
        }

        [Fact]
        public async Task DeleteCencus_Return_Success()
        {
            // Arrange

            string CensusId = "1";
            _unitOfWorkMock.Setup(uow => uow.CensusRepo.DeleteCencus(Convert.ToInt32(CensusId))).ReturnsAsync(true);

            // Act
            var result = await _reviewControllerMock.DeleteCencus(CensusId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCencus_NullCensus_BadRequest()
        {
            string censusId = "";
            // Act
            var result = await _reviewControllerMock.DeleteCencus(censusId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public void SaveEmployeeData_Return_Success()
        {
            // Arrange

            EmployeeData employeeData = new EmployeeData()
            {
                Id = 0,
                EmployeeId = "123",
                FacilityId = "1",
                JobTitle = "2",
                PayType = "2",
                FirstName = "ABC",
                LastName = "XYZ",
                HireDate = DateTime.Now.ToString(),
                TerminationDate = DateTime.Now.ToString()
            };

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.AddEmployee(It.IsAny<Employee>())).ReturnsAsync(true);

            // Act
            var result = _reviewControllerMock.SaveEmployeeeData(employeeData);

            // Assert
            Assert.NotNull(result);

            var jsonResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", jsonResult.Value);

        }

        [Fact]
        public void SaveEmployeeData_Throws_Exception()
        {
            // Arrange 
            EmployeeData employeeData = new EmployeeData()
            {
                Id = 0,
                EmployeeId = "123",
                FacilityId = "1",
                JobTitle = "2",
                PayType = "2",
                FirstName = "ABC",
                LastName = "XYZ",
                HireDate = DateTime.Now.ToString(),
                TerminationDate = DateTime.Now.ToString()
            };

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.AddEmployee(It.IsAny<Employee>())).Throws(new Exception("An error occured while adding new employee"));

            // Act
            var result = _reviewControllerMock.SaveEmployeeeData(employeeData);

            // Assert
            Assert.NotNull(result);

            var jsonResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, Convert.ToInt32(jsonResult.StatusCode));

        }

        [Fact]
        public void UpdateEmployeeData_Return_Success()
        {
            // Arrange 
            EmployeeData employeeData = new EmployeeData()
            {
                Id = 1,
                EmployeeId = "123",
                FacilityId = "1",
                JobTitle = "2",
                PayType = "2",
                FirstName = "ABC",
                LastName = "XYZ",
                HireDate = DateTime.Now.ToString(),
                TerminationDate = DateTime.Now.ToString()
            };

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.AddEmployee(It.IsAny<Employee>())).ReturnsAsync(true);

            // Act
            var result = _reviewControllerMock.SaveEmployeeeData(employeeData);

            // Assert
            Assert.NotNull(result);

            var jsonResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", jsonResult.Value);

        }

        [Fact]
        public void EmployeeIDLengthMoreThen30Characters_Returns_Message()
        {
            // Arrange 

            EmployeeData employeeData = new EmployeeData()
            {
                Id = 1,
                EmployeeId = "122333444455555666666777777788888888999999999",
                FacilityId = "1",
                JobTitle = "2",
                PayType = "2",
                FirstName = "ABC",
                LastName = "XYZ",
                HireDate = DateTime.Now.ToString(),
                TerminationDate = DateTime.Now.ToString()
            };

            // Act
            var result = _reviewControllerMock.SaveEmployeeeData(employeeData);

            // Assert
            Assert.NotNull(result);

            var jsonResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Employee Id must be smaller then 30 characters", jsonResult.Value);

        }

        [Fact]
        public void SaveEmployeeData_Returns_StatusCde500()
        {
            // Arrange 

            EmployeeData employeeData = null;

            // Act
            var result = _reviewControllerMock.SaveEmployeeeData(employeeData);

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, jsonResult.StatusCode);

        }

        [Theory]
        [InlineData("1", 200)]
        [InlineData("0", 200)]
        public async Task SaveEmployeeWorkingDetails_Return_StatusCode(string TimesheetId, int ExpectedResult)
        {

            EmployeeTimesheetSaveDataRequest request = new EmployeeTimesheetSaveDataRequest
            {
                TimesheetId = TimesheetId,
                EmployeeId = "1001",
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                PayCode = 1,
                JobTitle = 11,
                UploadType = 0,
                TotalHours = "10.0",
                WorkDate = "2023-07-10",
                WorkDateList = null
            };

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.AddTimesheet(It.IsAny<Timesheet>())).ReturnsAsync(true);
            // Act
            var result = _reviewControllerMock.SaveEmployeeWorkingDetails(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result.Result);
            Assert.Equal(ExpectedResult, jsonResult.Value);
        }

        [Theory]
        [InlineData("1", 404)]
        [InlineData("0", 404)]
        public async Task SaveEmployeeWorkingDetails_Throws_Exception(string TimesheetId, int ExpectedResult)
        {
            EmployeeTimesheetSaveDataRequest request = new EmployeeTimesheetSaveDataRequest
            {
                TimesheetId = TimesheetId,
                EmployeeId = "1001",
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                PayCode = 1,
                JobTitle = 11,
                UploadType = 0,
                TotalHours = "10.0",
                WorkDate = "2023-07-10",
                WorkDateList = null
            };

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.AddTimesheet(It.IsAny<Timesheet>())).Throws(new Exception("An error occured while adding new timesheet"));
            // Act
            var result = _reviewControllerMock.SaveEmployeeWorkingDetails(request);

            // Assert
            var jsonResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(ExpectedResult, jsonResult.StatusCode);
        }


        [Theory]
        [MemberData(nameof(MockTestDataForListWorkingDatesAndStatusCode))]
        public async Task EmployeeMultiDatesSave_Returns_StatusCode(List<string> workingDates, int ExpectedResult)
        {
            EmployeeMultiDatesSaveRequest request = new EmployeeMultiDatesSaveRequest()
            {
                EmployeeId = "12",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                PayCode = 1,
                JobTitle = 1,
                WorkingHours = "10.0",
                MultipleDates = workingDates
            };

            Timesheet timesheet = new Timesheet();

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetEmpTimesheetData(It.IsAny<EmployeeTimesheetDataRequest>())).ReturnsAsync(timesheet);
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.AddTimesheet(It.IsAny<Timesheet>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.InsertTimesheet(It.IsAny<List<Timesheet>>())).ReturnsAsync(true);


            var result = await _reviewControllerMock.EmployeeMultiDatesSave(request);
            Assert.NotNull(result);
            if (ExpectedResult == 200)
            {
                var jsonResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(ExpectedResult, jsonResult.StatusCode);
            }
            else
            {
                var jsonResult = Assert.IsType<NotFoundResult>(result);
                Assert.Equal(ExpectedResult, jsonResult.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(MockTestDataForListWorkingDatesAndStatusCodeForException))]
        public async Task EmployeeMultiDatesSave_Throws_Exception(List<string> workingDates, int ExpectedResult)
        {

            EmployeeMultiDatesSaveRequest request = new EmployeeMultiDatesSaveRequest()
            {
                EmployeeId = "12",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                PayCode = 1,
                JobTitle = 1,
                WorkingHours = "10.0",
                MultipleDates = workingDates
            };

            Timesheet timesheet = new Timesheet();

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetEmpTimesheetData(It.IsAny<EmployeeTimesheetDataRequest>())).Throws(new Exception("An error occured while getting timesheet details"));
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.AddTimesheet(It.IsAny<Timesheet>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.InsertTimesheet(It.IsAny<List<Timesheet>>())).Throws(new Exception("An error occured while saving timesheet"));


            var result = await _reviewControllerMock.EmployeeMultiDatesSave(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(ExpectedResult, jsonResult.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForListWorkingDatesAndStatusCode))]
        public async Task DeleteMultipleWorkingHoursDates_Returns_StatusCode(List<string> workingDates, int ExpectedResult)
        {
            EmployeeMultiDatesSaveRequest request = new EmployeeMultiDatesSaveRequest()
            {
                EmployeeId = "12",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                PayCode = 1,
                JobTitle = 1,
                WorkingHours = "10.0",
                MultipleDates = workingDates
            };
            Timesheet timesheet = new Timesheet();
            timesheet.Id = 1;
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetEmpTimesheetData(It.IsAny<EmployeeTimesheetDataRequest>())).ReturnsAsync(timesheet);
            var result = await _reviewControllerMock.DeleteMultipleWorkingHoursDates(request);
            Assert.NotNull(result);
            if (ExpectedResult == 200)
            {
                var jsonResult = Assert.IsType<OkResult>(result);
                Assert.Equal(ExpectedResult, jsonResult.StatusCode);
            }
            else
            {
                var jsonResult = Assert.IsType<NotFoundResult>(result);
                Assert.Equal(ExpectedResult, jsonResult.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(MockTestDataForListWorkingDatesAndStatusCodeForException))]
        public async Task DeleteMultipleWorkingHoursDates_Returns_StatusCode_Throws_Exception(List<string> workingDates, int ExpectedResult)
        {
            EmployeeMultiDatesSaveRequest request = new EmployeeMultiDatesSaveRequest()
            {
                EmployeeId = "12",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                PayCode = 1,
                JobTitle = 1,
                WorkingHours = "10.0",
                MultipleDates = workingDates,
            };
            Timesheet timesheet = new Timesheet
            {
                Id = 1,
                EmployeeId = "12",
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                Workday = DateTime.UtcNow,
                IsActive = true,

            };
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetEmpTimesheetData(It.IsAny<EmployeeTimesheetDataRequest>())).ReturnsAsync(timesheet);
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.AddTimesheet(It.IsAny<Timesheet>())).Throws(new Exception("An error occured while saving employee timesheet data"));
            var result = await _reviewControllerMock.DeleteMultipleWorkingHoursDates(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(ExpectedResult, jsonResult.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForApproveTimesheetRequestAndStatusCode))]
        public async Task ApproveUploadData_Return_StatusCode(ApproveTimesheetRequest approve, int ExpectedResult)
        {
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.ApproveTimesheetData(It.IsAny<ApproveTimesheetRequest>())).ReturnsAsync(new ApproveTimesheetResponse() { Success = true });
            var result = await _reviewControllerMock.ApproveUploadData(approve);
            Assert.NotNull(result);
            if (ExpectedResult == 200)
            {
                var jsonResult = Assert.IsType<JsonResult>(result);
                var importResponse = Assert.IsType<ApproveTimesheetResponse>(jsonResult.Value);
                Assert.True(importResponse.Success);
            }
            else
            {
                var jsonResult = Assert.IsType<NotFoundResult>(result);
                Assert.Equal(ExpectedResult, jsonResult.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(MockTestDataForApproveTimesheetRequestAndStatusCodeForException))]
        public async Task ApproveUploadData_Return_StatusCode_Throws_Exception(ApproveTimesheetRequest approve, int ExpectedResult)
        {
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.ApproveTimesheetData(It.IsAny<ApproveTimesheetRequest>())).Throws(new Exception("An error occured while approving timesheet data"));
            var result = await _reviewControllerMock.ApproveUploadData(approve);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(ExpectedResult, jsonResult.StatusCode);
        }

        [Fact]
        public async Task AddMissingNurseHoursData_Returns_404()
        {
            EmployeeTimesheetSaveDataRequest request = new EmployeeTimesheetSaveDataRequest()
            {
                TimesheetId = "123",
                EmployeeId = "Emp005",
                FacilityId = 0,
                ReportQuarter = 0,
                Month = 0,
                Year = 0,
                PayCode = 0,
                JobTitle = 0,
                UploadType = 0,
                TotalHours = "10.0",
                WorkDate = "2023--7-10",
                WorkDateList = new List<string>(),
            };
            List<Employee> employee = new List<Employee>();
            employee = null;
            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.GetAll()).Returns(employee);
            var result = await _reviewControllerMock.AddMissingNurseHoursData(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, jsonResult.StatusCode);
        }

        [Fact]
        public async Task AddMissingNurseHoursData_Returns_200()
        {
            EmployeeTimesheetSaveDataRequest request = new EmployeeTimesheetSaveDataRequest()
            {
                TimesheetId = "123",
                EmployeeId = "2002",
                FacilityId = 10,
                ReportQuarter = 0,
                Month = 0,
                Year = 0,
                PayCode = 0,
                JobTitle = 0,
                UploadType = 0,
                TotalHours = "10.0",
                WorkDate = "2023--7-10",
                WorkDateList = new List<string>() { "2023-07-10", "2023-07-11" },
            };

            Employee employee = new Employee()
            {
                Id = 2002,
                EmployeeId = "2002",
                FacilityId = 10,
                JobTitleCode = 0,
                PayTypeCode = 0,
                FirstName = "",
                LastName = "",
                HireDate = DateTime.Now,
                TerminationDate = DateTime.Now,
                IsActive = true
            };

            var employee1 = new List<Employee>() { employee }.AsQueryable();

            _unitOfWorkMock.Setup(r => r.EmployeeRepo.GetAll()).Returns(employee1);
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.InsertTimesheet(It.IsAny<List<Timesheet>>())).ReturnsAsync(true);
            var result = await _reviewControllerMock.AddMissingNurseHoursData(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, jsonResult.StatusCode);
        }

        [Fact]
        public async Task AddMissingNurseHoursData_WithoutWorkDateList_Returns_404()
        {
            EmployeeTimesheetSaveDataRequest request = new EmployeeTimesheetSaveDataRequest()
            {
                TimesheetId = "123",
                EmployeeId = "Emp005",
                FacilityId = 1,
                ReportQuarter = 0,
                Month = 0,
                Year = 0,
                PayCode = 0,
                JobTitle = 0,
                UploadType = 0,
                TotalHours = "10.0",
                WorkDate = "2023--7-10",
            };

            Employee employee = new Employee()
            {
                Id = 123,
                EmployeeId = "Emp005",
                FacilityId = 1,
                JobTitleCode = 0,
                PayTypeCode = 0,
                FirstName = "",
                LastName = "",
                HireDate = DateTime.Now,
                TerminationDate = DateTime.Now,
                IsActive = true
            };

            var employee1 = new List<Employee>() { employee }.AsQueryable();

            _unitOfWorkMock.Setup(r => r.EmployeeRepo.GetAll()).Returns(employee1);
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.InsertTimesheet(It.IsAny<List<Timesheet>>())).ReturnsAsync(true);
            var result = await _reviewControllerMock.AddMissingNurseHoursData(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, jsonResult.StatusCode);
        }

        [Fact]
        public async Task MissingDates_Returns_List()
        {
            NursesMissingDates missingDates = new NursesMissingDates()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                JobTitle = "9"
            };
            List<DateTime> dateTimes = new List<DateTime>() { Convert.ToDateTime("2023-07-01"), Convert.ToDateTime("2023-07-02"), Convert.ToDateTime("2023-07-03") };
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetMissingDatesByJobTitle(missingDates.FacilityId, missingDates.StartDate, missingDates.EndDate, 9)).ReturnsAsync(dateTimes);
            var result = await _reviewControllerMock.GetMissingNursesDates(missingDates);
            Assert.NotNull(result);
            var DateResponse = Assert.IsType<JsonResult>(result);
            List<NursesMissingDatesResponse> list = (List<NursesMissingDatesResponse>)DateResponse.Value;
            Assert.True(list.Any());
        }

        [Fact]
        public async Task MissingDates_Throws_Exception()
        {

            NursesMissingDates missingDates = new NursesMissingDates()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 2,
                Month = 0,
                JobTitle = "9"
            };
            List<DateTime> dateTimes = new List<DateTime>() { Convert.ToDateTime("2023-07-01"), Convert.ToDateTime("2023-07-02"), Convert.ToDateTime("2023-07-03") };
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetMissingDatesByJobTitle(missingDates.FacilityId, missingDates.StartDate, missingDates.EndDate, 9)).Throws(new Exception("An error occured while getting missing dates"));
            var result = await _reviewControllerMock.GetMissingNursesDates(missingDates);
            Assert.NotNull(result);
            var DateResponse = Assert.IsType<JsonResult>(result);
            List<NursesMissingDatesResponse> list = (List<NursesMissingDatesResponse>)DateResponse.Value;
            Assert.True(list.IsNullOrEmpty());
        }

        [Fact]
        public void ExportCensusToCsv_Returns_FileType()
        {
            CensusCsvRequest csvRequest = new CensusCsvRequest()
            {
                FacilityID = 1,
                Year = 2023,
                ReportQuarter = 1
            };
            _unitOfWorkMock.Setup(r => r.CensusRepo.GetCencusByFacilityAndQuarter(csvRequest.FacilityID, csvRequest.ReportQuarter, csvRequest.Year))
                .Returns(ExportCsv());

            var result = _reviewControllerMock.ExportCensusToCsv(csvRequest);
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
        }

        [Fact]
        public void ExportCensusToCsv_Throws_Exception()
        {
            CensusCsvRequest csvRequest = new CensusCsvRequest()
            {
                FacilityID = 1,
                Year = 2023,
                ReportQuarter = 1
            };
            _unitOfWorkMock.Setup(r => r.CensusRepo.GetCencusByFacilityAndQuarter(csvRequest.FacilityID, csvRequest.ReportQuarter, csvRequest.Year))
                .Throws(new Exception("An error occured while exporting census data to csv"));

            var result = _reviewControllerMock.ExportCensusToCsv(csvRequest);
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
        }

        [Fact]
        public void ExportStaffingDeptToCsv_Returns_FileType()
        {
            StaffingDepartmentCsvRequest csvRequest = new StaffingDepartmentCsvRequest()
            {
                FacilityID = 1,
                Year = 2023,
                ReportQuarter = 1
            };
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetStaffingDepartmentDetailsForCsv(csvRequest.FacilityID, csvRequest.ReportQuarter, csvRequest.Year))
                .Returns(ExportStaffingDeptDatatable());

            var result = _reviewControllerMock.ExportStaffingDeptToCsv(csvRequest);
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
        }

        [Fact]
        public void ExportStaffingDeptToCsv_Throws_Exception()
        {

            StaffingDepartmentCsvRequest csvRequest = new StaffingDepartmentCsvRequest()
            {
                FacilityID = 1,
                Year = 2023,
                ReportQuarter = 1
            };
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetStaffingDepartmentDetailsForCsv(csvRequest.FacilityID, csvRequest.ReportQuarter, csvRequest.Year))
                .Throws(new Exception("An error occured while exporting staffing data to csv"));

            var result = _reviewControllerMock.ExportStaffingDeptToCsv(csvRequest);
            Assert.NotNull(result);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
            Assert.Equal(".csv", file.FileDownloadName);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("")]
        public void EditCencus_Returns_JsonData(string CensusId)
        {
            _unitOfWorkMock.Setup(r => r.CensusRepo.GetAll()).Returns(new List<Census>() { new Census() { CensusId = 1, FacilityId = 1, Other = 1, Medicad = 1, Medicare = 1 } });
            var result = _reviewControllerMock.EditCencus(CensusId);
            Assert.NotNull(result);
            if (CensusId != "")
            {
                var jsonResult = Assert.IsType<JsonResult>(result);
                var response = Assert.IsType<Census>(jsonResult.Value);
                Assert.Equal(CensusId, response.CensusId.ToString());
            }
            else
            {
                var jsonResult = Assert.IsType<NotFoundResult>(result);
                Assert.Equal(404, jsonResult.StatusCode);
            }
        }

        [Theory]
        [InlineData("1")]
        [InlineData("")]
        public void EditCencus_Throws_Exception(string CensusId)
        {
            _unitOfWorkMock.Setup(r => r.CensusRepo.GetAll()).Throws(new Exception("An error occured while getting census data for id 1"));
            var result = _reviewControllerMock.EditCencus(CensusId);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, jsonResult.StatusCode);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("")]
        public void EditEmployee_Returns_JsonData(string EmpId)
        {
            _unitOfWorkMock.Setup(r => r.EmployeeRepo.GetAll()).Returns(new List<Employee>() { new Employee() { Id = 1, EmployeeId = "1", FacilityId = 1 } });
            var result = _reviewControllerMock.EditEmployeeData(EmpId);
            Assert.NotNull(result);
            if (EmpId != "")
            {
                var jsonResult = Assert.IsType<JsonResult>(result);
                var response = Assert.IsType<Employee>(jsonResult.Value);
                Assert.Equal(EmpId, response.Id.ToString());
            }
            else
            {
                var jsonResult = Assert.IsType<NotFoundResult>(result);
                Assert.Equal(404, jsonResult.StatusCode);
            }
        }

        [Fact]
        public async Task GetEmployeeTimesheet_Returns_JsonData()
        {

            EmployeeTimesheetDataRequest request = new EmployeeTimesheetDataRequest()
            {
                EmployeeId = "",
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                Workday = "2023-07-10"

            };
            EmployeeTimesheetData timesheetData = new EmployeeTimesheetData()
            {
                Workday = Convert.ToDateTime("2023-07-01"),
                TimesheetId = 1,
                EmployeeId = "Emp001",
                FacilityId = 1,
                TotalHours = "10.2",
                Editable = true,
                Color = "red"
            };

            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetEmployeeTimesheetData(request)).ReturnsAsync(new List<EmployeeTimesheetData>() { timesheetData });
            var result = await _reviewControllerMock.GetEmployeeTimesheet(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var list = Assert.IsType<List<EmployeeTimesheetData>>(jsonResult.Value);
            Assert.True(list.Any());
        }

        [Fact]
        public async Task GetEmployeeTimesheet_Throws_Exception()
        {


            EmployeeTimesheetDataRequest request = new EmployeeTimesheetDataRequest()
            {
                EmployeeId = "",
                FacilityId = 1,
                ReportQuarter = 2,
                Month = 0,
                Year = 2023,
                Workday = "2023-07-10"
            };
            _unitOfWorkMock.Setup(r => r.TimesheetRepo.GetEmployeeTimesheetData(request)).Throws(new Exception("An error occured while getting employee's timesheet data"));
            var result = await _reviewControllerMock.GetEmployeeTimesheet(request);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var list = Assert.IsType<List<EmployeeTimesheetData>>(jsonResult.Value);
            Assert.True(list.IsNullOrEmpty());
        }

        [Fact]
        public void UpdateEmployeeStatus_ValidEmployeeId_ReturnsSuccess()
        {
            // Arrange
            var employeeId = "1";
            var status = true;


            var expectedResult = 200;

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.UpdateEmployeeStatus(It.IsAny<Int32>(), It.IsAny<bool>())).Returns(true);

            // Act
            var result = _reviewControllerMock.UpdateEmployeeStatus(employeeId, status);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(expectedResult, response.StatusCode);
            Assert.Equal("Employee Status is Updated Successfully", response.Message);
        }

        [Fact]
        public void UpdateEmployeeStatus_InvalidEmployeeId_ReturnsFailure()
        {
            // Arrange
            var employeeId = "-1";
            var status = true;

            var expectedResult = 400;

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.UpdateEmployeeStatus(It.IsAny<Int32>(), It.IsAny<bool>())).Returns(false);

            // Act
            var result = _reviewControllerMock.UpdateEmployeeStatus(employeeId, status);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(expectedResult, response.StatusCode);
            Assert.Equal("Failed to Update Employee Status", response.Message);
        }

        [Fact]
        public void UpdateEmployeeStatus_Throws_Exception()
        {
            // Arrange
            var employeeId = "-1";
            var status = true;


            var expectedResult = 400;

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo.UpdateEmployeeStatus(It.IsAny<Int32>(), It.IsAny<bool>())).Throws(new Exception("An error occured while updating employee status"));

            // Act
            var result = _reviewControllerMock.UpdateEmployeeStatus(employeeId, status);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<DomainModels.Models.Response>(jsonResult.Value);
            Assert.Equal(expectedResult, response.StatusCode);
            Assert.Equal("Failed to Update Employee Status", response.Message);
        }

        //Latest added by Rahul

        [Fact]
        public void GetSearchedEmployees_ReturnsJsonResultWithData()
        {
            // Arrange
            var employeeRepoMock = new Mock<IEmployee>();
            _unitOfWorkMock.Setup(u => u.EmployeeRepo).Returns(employeeRepoMock.Object);

            var employeeListRequest = new EmployeeListRequest
            {
                FacilityId = "1",
                ReportQuarter = "2",
                Year = "2023",
                Month = "0"
            };

            var mockEmployeeDataList = new List<CustomEmployeeData>
            {
                new CustomEmployeeData
                {
                    EmployeeId="101",
                    FacilityId="1",
                    JobTitleCode=1,
                    PayTypeCode=1,
                    FirstName="Rahul",
                    LastName="Rahul Pawar-s",
                   HireDate=DateTime.UtcNow.ToString(),
                   TerminationDate=DateTime.UtcNow.ToString(),
                   CreateDate=DateTime.UtcNow.ToString(),
                   IsActive=true
                },
                new CustomEmployeeData
                {
                    EmployeeId="10255",
                    FacilityId="1",
                    JobTitleCode=1,
                    PayTypeCode=1,
                    FirstName="KP",
                    LastName="KP",
                   HireDate=DateTime.UtcNow.ToString(),
                   TerminationDate=DateTime.UtcNow.ToString(),
                   CreateDate=DateTime.UtcNow.ToString(),
                   IsActive=true
                }
             };
            employeeRepoMock.Setup(repo => repo.GetSearchedEmployeesData(employeeListRequest)).Returns(mockEmployeeDataList);

            // Act
            var result = _reviewControllerMock.GetSearchedEmployees(employeeListRequest);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var employeeModel = Assert.IsType<List<CustomEmployeeData>>(jsonResult.Value);
            Assert.NotNull(employeeModel);
            Assert.Same(mockEmployeeDataList, employeeModel);
        }

        [Fact]
        public void GetSearchedEmployees_ReturnsJsonResultWithDataOnException()
        {
            // Arrange
            var employeeRepoMock = new Mock<IEmployee>();
            _unitOfWorkMock.Setup(u => u.EmployeeRepo).Returns(employeeRepoMock.Object);

            var employeeListRequest = new EmployeeListRequest
            {
                FacilityId = "1",
                ReportQuarter = "2",
                Year = "2023",
                Month = "0"
            };

            var mockEmployeeDataList = new List<CustomEmployeeData>();
            employeeRepoMock.Setup(repo => repo.GetSearchedEmployeesData(employeeListRequest)).Throws(new Exception("An error occured while getting searched employee list"));

            // Act
            var result = _reviewControllerMock.GetSearchedEmployees(employeeListRequest);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var employeeModel = Assert.IsType<List<CustomEmployeeData>>(jsonResult.Value);
            Assert.NotNull(employeeModel);
        }

        [Fact]
        public async Task DeleteTimesheetData_ValidRequest_ReturnsOk()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var request = new DeleteTimesheetDataRequest
            {
                TimesheetId = "1",
                UserID = "061d2430-9d89-4382-a1dd-2fe01e46d435"
            };

            // Mock User.FindFirstValue
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal", UserType = 1 };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            var controller = new ReviewController(_unitOfWorkMock.Object, _userManagerMock.Object, mockPdfExportHelper.Object, _iUpload.Object, _dataProtectionProviderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Set up mock behavior for DeleteTimesheetRecord
            timesheetRepoMock.Setup(repo => repo.DeleteTimesheetRecord(request)).ReturnsAsync(true);

            // Act
            var result = await controller.DeleteTimesheetData(request);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteTimesheetData_ValidRequest_ReturnsNotFoundOnThrowsException()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var request = new DeleteTimesheetDataRequest
            {
                TimesheetId = "1",
                UserID = "061d2430-9d89-4382-a1dd-2fe01e46d435"
            };

            // Mock User.FindFirstValue
            var user = new AspNetUser { Id = "061d2430-9d89-4382-a1dd-2fe01e46d435", FirstName = "Krunal", UserType = 1 };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            var controller = new ReviewController(_unitOfWorkMock.Object, _userManagerMock.Object, mockPdfExportHelper.Object, _iUpload.Object, _dataProtectionProviderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Set up mock behavior for DeleteTimesheetRecord
            timesheetRepoMock.Setup(repo => repo.DeleteTimesheetRecord(request)).Throws(new Exception("An error occured while deleting timesheet record"));

            // Act
            var result = await controller.DeleteTimesheetData(request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStatusApprovedList_ReturnsJsonResultWithData()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockReportApprovedinfo = new List<ReportVerificationViewModel>
            {
                new ReportVerificationViewModel{
                    ApprovedDate=DateTime.UtcNow.ToString(),
                    ApprovedBy="061d2430-9d89-4382-a1dd-2fe01e46d435"

                },
                new ReportVerificationViewModel{
                    ApprovedDate=DateTime.UtcNow.ToString(),
                    ApprovedBy="061d2430-9d89-4382-a1dd-2fe01e46d435"

                }
            };
            timesheetRepoMock.Setup(repo => repo.GetAllReportApprovedStatus(facilityId, reportQuarter, year))
                                 .ReturnsAsync(mockReportApprovedinfo);

            // Act
            var result = await _reviewControllerMock.GetStatusApprovedList(facilityId, reportQuarter, year);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultList = Assert.IsType<List<ValidationResult>>(jsonResult.Value);

            Assert.NotNull(resultList);
            Assert.Equal(mockReportApprovedinfo.Count, resultList.Count);
        }

        [Fact]
        public async Task GetStatusApprovedList_ReturnsJsonResultOnException()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            timesheetRepoMock.Setup(repo => repo.GetAllReportApprovedStatus(facilityId, reportQuarter, year))
                                 .Throws(new Exception("An error occured while getting approved list"));

            // Act
            var result = await _reviewControllerMock.GetStatusApprovedList(facilityId, reportQuarter, year);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultList = Assert.IsType<List<ValidationResult>>(jsonResult.Value);
            Assert.NotNull(resultList);
        }

        [Fact]
        public async Task GetStatusValidatedList_ReturnsJsonResultWithData()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockReportValidatedinfo = new List<ReportVerificationViewModel>
            {
                new ReportVerificationViewModel{
                    ValidationDate=DateTime.UtcNow.ToString(),
                    ValidatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435"

                },
                new ReportVerificationViewModel{
                    ValidationDate=DateTime.UtcNow.ToString(),
                    ValidatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435"

                }
            };
            timesheetRepoMock.Setup(repo => repo.GetAllReportValidationStatus(facilityId, reportQuarter, year))
                                 .ReturnsAsync(mockReportValidatedinfo);

            // Act
            var result = await _reviewControllerMock.GetStatusValidatedList(facilityId, reportQuarter, year);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultList = Assert.IsType<List<ValidationResult>>(jsonResult.Value);

            Assert.NotNull(resultList);
            Assert.Equal(mockReportValidatedinfo.Count, resultList.Count);
        }

        [Fact]
        public async Task GetStatusValidatedList_ReturnsJsonResultWithNoDataOnException()
        {
            // Arrange
            var timesheetRepoMock = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(u => u.TimesheetRepo).Returns(timesheetRepoMock.Object);

            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            timesheetRepoMock.Setup(repo => repo.GetAllReportValidationStatus(facilityId, reportQuarter, year))
                                 .Throws(new Exception("An error occured while getting validated list"));

            // Act
            var result = await _reviewControllerMock.GetStatusValidatedList(facilityId, reportQuarter, year);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultList = Assert.IsType<List<ValidationResult>>(jsonResult.Value);

            Assert.NotNull(resultList);
        }






        #region Modal
        public static IEnumerable<object[]> MockTestDataForListWorkingDatesAndStatusCode()
        {
            yield return new object[] { new List<string> { "2023-07-10", "2023-07-11" }, 200 };
            yield return new object[] { new List<string> { }, 404 };
        }

        public static IEnumerable<object[]> MockTestDataForListWorkingDatesAndStatusCodeForException()
        {
            yield return new object[] { new List<string> { "2023-12-18", "2023-12-19" }, 404 };
            yield return new object[] { new List<string> { }, 404 };
        }
        public static IEnumerable<object[]> MockTestDataForApproveTimesheetRequestAndStatusCode()
        {
            yield return new object[] { new ApproveTimesheetRequest { FacilityId = 1, Year = 2023, ReportQuarter = 2, Month = 0, UserId = "" }, 200 };
            yield return new object[] { null, 404 };
        }

        public static IEnumerable<object[]> MockTestDataForApproveTimesheetRequestAndStatusCodeForException()
        {
            yield return new object[] { new ApproveTimesheetRequest { FacilityId = 1, Year = 2023, ReportQuarter = 2, Month = 0, UserId = "" }, 404 };
            yield return new object[] { null, 404 };
        }

        #endregion  Modal

        public DataTable ExportCsv()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Clear();
            dataTable.Columns.Add(new DataColumn("FacilityName"));
            dataTable.Columns.Add(new DataColumn("Year"));
            dataTable.Columns.Add(new DataColumn("Month"));
            dataTable.Columns.Add(new DataColumn("Medicad"));
            dataTable.Columns.Add(new DataColumn("Medicare"));
            dataTable.Columns.Add(new DataColumn("Other"));
            var dataRow = dataTable.NewRow();
            dataRow["FacilityName"] = "Facility1";
            dataRow["Year"] = 2023;
            dataRow["Month"] = "July";
            dataRow["Medicad"] = 10;
            dataRow["Medicare"] = 20;
            dataRow["Other"] = 30;
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }

        public DataTable ExportStaffingDeptDatatable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Clear();
            dataTable.Columns.Add(new DataColumn("Title"));
            dataTable.Columns.Add(new DataColumn("FacilityName"));
            dataTable.Columns.Add(new DataColumn("TotalHours"));
            dataTable.Columns.Add(new DataColumn("StaffCount"));
            dataTable.Columns.Add(new DataColumn("Exempt"));
            dataTable.Columns.Add(new DataColumn("NonExempt"));
            dataTable.Columns.Add(new DataColumn("Contractors"));
            var dataRow = dataTable.NewRow();
            dataRow["Title"] = "Test";
            dataRow["FacilityName"] = "Facility1";
            dataRow["TotalHours"] = 2074;
            dataRow["StaffCount"] = 60;
            dataRow["Exempt"] = 10;
            dataRow["NonExempt"] = 20;
            dataRow["Contractors"] = 30;
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }

        [Theory]
        [InlineData("1", "text/csv")]
        [InlineData("2", "application/pdf")]
        public async Task ExportStaff_CSV_ReturnsCSV(string FormatType, string ContentType)
        {
            // Arrange


            EmployeeExportRequest employeeExport = new EmployeeExportRequest()
            {
                PayType = "1",
                JobTitle = "1",
                FormatType = FormatType,
                Quarter = "4",
                Year = "2023",
                Month = "0",
                FacilityId = "1",
                Filename = "test",

            };
            StaffExport staff = new StaffExport()
            {
                EmployeeId = "1"
            };
            EmployeeData employee = new EmployeeData()
            {
                EmployeeId = "1"
            };
            StaffExportList staffExportList = new StaffExportList();
            staffExportList.staffExports = new List<StaffExport> { staff };
            staffExportList.EmployeeData = new List<EmployeeData> { employee };
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetStaffExportData(employeeExport)).ReturnsAsync(staffExportList);
            var expectedHtml = "<html><body>Hello, world!</body></html>";
            mockPdfExportHelper.Setup(x => x.RenderToStringAsync("StaffExportPDF", staffExportList))
        .ReturnsAsync(expectedHtml);
            // Act
            var result = _reviewControllerMock.ExportStaff(employeeExport);

            // Assert
            Assert.NotNull(result);
            if (FormatType == "1")
            {
                var file = Assert.IsType<FileContentResult>(result.Result);
                Assert.Equal(ContentType, file.ContentType);
            }
            else
            {
                var file = Assert.IsType<FileStreamResult>(result.Result);
                Assert.Equal(ContentType, file.ContentType);
            }

        }

        [Theory]
        [InlineData("1", "text/csv")]
        [InlineData("2", "application/pdf")]
        public async Task ExportStaff_CSV_Throws_Exception(string FormatType, string ContentType)
        {
            // Arrange
            EmployeeExportRequest employeeExport = new EmployeeExportRequest()
            {
                PayType = "1",
                JobTitle = "1",
                FormatType = FormatType,
                Quarter = "4",
                Year = "2023",
                Month = "0",
                FacilityId = "1",
                Filename = "test",

            };
            StaffExport staff = new StaffExport()
            {
                EmployeeId = "1"
            };
            EmployeeData employee = new EmployeeData()
            {
                EmployeeId = "1"
            };
            StaffExportList staffExportList = new StaffExportList();
            staffExportList.staffExports = new List<StaffExport> { staff };
            staffExportList.EmployeeData = new List<EmployeeData> { employee };
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.GetStaffExportData(employeeExport)).Throws(new Exception("An error occured while exporting staff data"));

            // Act
            var result = _reviewControllerMock.ExportStaff(employeeExport);

            // Assert
            Assert.NotNull(result);
            if (FormatType == "1")
            {
                var file = Assert.IsType<NotFoundResult>(result.Result);
                Assert.Equal(404, file.StatusCode);
            }
            else
            {
                var file = Assert.IsType<NotFoundResult>(result.Result);
                Assert.Equal(404, file.StatusCode);
            }
        }

        [Fact]
        public void ValidateData_Timesheet_ReturnsSuccess()
        {
            ApproveTimesheetRequest approve = new ApproveTimesheetRequest()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0,
                UserId = ""
            };
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.ValidateTimesheetData(approve)).ReturnsAsync(new ApproveTimesheetResponse() { Success = true });
            // Act
            var result = _reviewControllerMock.ValidateData(approve);

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result.Result);
            var response = jsonResult.Value as ApproveTimesheetResponse;
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateData_Timesheet_Throws_Exception()
        {

            var expectedResult = 404;
            ApproveTimesheetRequest approve = new ApproveTimesheetRequest()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0,
                UserId = ""
            };
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo.ValidateTimesheetData(approve)).Throws(new Exception("An error occured while validating timesheet"));
            // Act
            var result = _reviewControllerMock.ValidateData(approve);

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(expectedResult, jsonResult.StatusCode);
        }

        //Rahul's Xunit test Cases's

        [Fact]
        public async Task GetEmployeeIdAndName_Returns_Valid_Data()
        {
            // Arrange
            var searchVal = "Krunal";
            var facilityId = 1;
            var mockEmployeeRepo = new Mock<IEmployee>(); // Assuming you have an employee repository interface
            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            // Create a list of employee data to return
            var employeeData = new List<SearchResponse>
            {
                new SearchResponse { Id = 1, Text = "Krunal" },
                new SearchResponse { Id = 2, Text = "Kapil" }
            };

            // Set up the EmployeeRepo to return the data
            mockEmployeeRepo.Setup(repo => repo.GetEmployeeIdAndName(searchVal, facilityId))
                .ReturnsAsync(employeeData);

            // Act
            var result = await _reviewControllerMock.GetEmployeeIdAndName(searchVal, "1");

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = okObjectResult.Value;
            Assert.True(resultValue.GetType().GetProperty("items") != null);
            var items = resultValue.GetType().GetProperty("items").GetValue(resultValue);
            Assert.IsType<List<SearchResponse>>(items);
        }

        [Fact]
        public async Task GetEmployeeIdAndName_Handles_Exception()
        {
            // Arrange
            var searchVal = "Krunal";
            var facilityId = "1";
            var mockEmployeeRepo = new Mock<IEmployee>(); // Assuming you have an employee repository interface
            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            // Set up the EmployeeRepo to throw an exception when called
            mockEmployeeRepo.Setup(repo => repo.GetEmployeeIdAndName(searchVal, 1))
                .ThrowsAsync(new Exception("An Error occured while getting employee data"));

            // Act
            var result = await _reviewControllerMock.GetEmployeeIdAndName(searchVal, facilityId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEmployeeName_Returns_Valid_Data()
        {
            // Arrange
            var employeeId = "2002";
            var mockEmployeeRepo = new Mock<IEmployee>();

            // Create a mock employee object to return
            var employeeData = new Employee
            {
                EmployeeId = "2002",
                FirstName = "Rahul",
                LastName = "Pawar",
                IsActive = true
            };

            var employeeList = new List<Employee> { employeeData };

            // Set up the EmployeeRepo to return the employee model
            mockEmployeeRepo.Setup(repo => repo.GetAll())
                .Returns(employeeList);

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            // Act
            var result = _reviewControllerMock.GetEmployeeName(employeeId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var employeeName = jsonResult.Value.ToString();
            Assert.Equal("Rahul Pawar", employeeName); // Assuming it returns the full name
        }

        [Fact]
        public void GetEmployeeName_Returns_Empty_When_Employee_Not_Found()
        {
            // Arrange
            var employeeId = "NonExistentEmployeeId";
            var mockEmployeeRepo = new Mock<IEmployee>();

            // Set up the EmployeeRepo to return an empty result (no matching employee)
            mockEmployeeRepo.Setup(repo => repo.GetAll())
                .Returns(new List<Employee>());

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            // Act
            var result = _reviewControllerMock.GetEmployeeName(employeeId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var resultValue = jsonResult.Value.ToString();
            Assert.Equal("Microsoft.AspNetCore.Mvc.EmptyResult", resultValue);
        }

        [Fact]
        public void GetEmployeeName_Handles_Exception()
        {
            // Arrange
            var employeeId = "2002";
            var mockEmployeeRepo = new Mock<IEmployee>();

            // Set up the EmployeeRepo to throw an exception when called
            mockEmployeeRepo.Setup(repo => repo.GetAll())
                .Throws(new Exception("An Error occured while getting employee data"));

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            // Act
            var result = _reviewControllerMock.GetEmployeeName(employeeId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var resultValue = jsonResult.Value.ToString();
            Assert.Equal("Microsoft.AspNetCore.Mvc.EmptyResult", resultValue); // Ensure it returns "Empty" on exception
        }

        [Fact]
        public async Task CheckIfValidated_Returns_Valid_Data()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Create a mock report verification info
            var reportVerificationInfo = new ReportVerificationViewModel
            {
                ValidatedBy = "Rahul Pawar",
                ValidationDate = DateTime.UtcNow.ToString()
            };

            // Set up the TimesheetRepo to return the report verification info
            mockTimesheetRepo.Setup(repo => repo.GetReportValidationStatus(facilityId, reportQuarter, year))
                .ReturnsAsync(reportVerificationInfo);

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfValidated(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.True(data.Success);
            Assert.Equal("Last Validated by: Rahul Pawar", data.FullName);
            Assert.StartsWith("Last Validation Date & Time:", data.ValidationDate);
        }


        [Fact]
        public async Task CheckIfValidated_Returns_Not_Validated()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Create a mock report verification info
            var reportVerificationInfo = new ReportVerificationViewModel
            {
                ValidatedBy = null,
                ValidationDate = null
            };

            // Set up the TimesheetRepo to return the report verification info
            mockTimesheetRepo.Setup(repo => repo.GetReportValidationStatus(facilityId, reportQuarter, year))
        .ReturnsAsync(reportVerificationInfo);

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfValidated(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.False(data.Success);
            Assert.Null(data.FullName);
            Assert.Null(data.ValidationDate);
        }


        [Fact]
        public async Task CheckIfValidated_Handles_Exception()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Set up the TimesheetRepo to throw an exception when called
            mockTimesheetRepo.Setup(repo => repo.GetReportValidationStatus(facilityId, reportQuarter, year))
                .ThrowsAsync(new Exception("An Error occured while getting validation status data"));

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfValidated(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.False(data.Success);
            Assert.Null(data.FullName); // FullName should be null
            Assert.Null(data.ValidationDate); // ValidationDate should be null
            Assert.Equal("Unable to Validate.  A message has been sent to ThinkAnew on your behalf.", data.Message);
        }

        [Fact]
        public async Task CheckIfApproved_Returns_Valid_Data()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Create a mock report verification info
            var reportVerificationInfo = new ReportVerificationViewModel
            {
                ApprovedBy = "Rahul Pawar",
                ApprovedDate = DateTime.UtcNow.ToString()
            };

            // Set up the TimesheetRepo to return the report verification info
            mockTimesheetRepo.Setup(repo => repo.GetReportApprovedStatus(facilityId, reportQuarter, year))
                .ReturnsAsync(reportVerificationInfo);

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfApproved(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.True(data.Success);
            Assert.Equal("Last Approved by: Rahul Pawar", data.FullName);
            Assert.StartsWith("Last Approved Date & Time:", data.ApprovedDate);
        }

        [Fact]
        public async Task CheckIfApproved_Returns_Not_Validated()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Create a mock report verification info
            var reportVerificationInfo = new ReportVerificationViewModel
            {
                ApprovedBy = null,
                ApprovedDate = null
            };

            // Set up the TimesheetRepo to return the report verification info
            mockTimesheetRepo.Setup(repo => repo.GetReportApprovedStatus(facilityId, reportQuarter, year))
        .ReturnsAsync(reportVerificationInfo);

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfApproved(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.False(data.Success);
            Assert.Null(data.FullName);
            Assert.Null(data.ApprovedDate);
        }

        [Fact]
        public async Task CheckIfApproved_Handles_Exception()
        {
            // Arrange
            var facilityId = 1;
            var reportQuarter = 2;
            var year = 2023;

            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Assuming you have a timesheet repository interface

            // Set up the TimesheetRepo to throw an exception when called
            mockTimesheetRepo.Setup(repo => repo.GetReportApprovedStatus(facilityId, reportQuarter, year))
                .ThrowsAsync(new Exception("An Error occured while getting approved status data"));

            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            // Act
            var result = await _reviewControllerMock.CheckIfApproved(facilityId, reportQuarter, year);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as ValidationResult;

            Assert.False(data.Success);
            Assert.Null(data.FullName); // FullName should be null
            Assert.Null(data.ApprovedDate); // ApprovedDate should be null
            Assert.Equal("Unable to Approve.  A message has been sent to ThinkAnew on your behalf.", data.Message);
        }

        [Fact]
        public void GetCensus_Returns_Valid_Data()
        {
            // Arrange
            var censusViewRequest = new CensusViewRequest
            {
                FacilityID = "13",
                ReportQuarter = 4,
                Year = 2023,
            };

            var mockCensusRepo = new Mock<ICensus>(); // Assuming you have a census repository interface

            // Set up mock data and method calls for successful scenario
            var pageSize = 10;
            var start = "0";
            var length = "10";
            var sortOrder = "facilityName_asc";
            var totalRecord = 1;
            var filterRecord = 1;
            var facilityDetails = new FacilityModel
            {
                Id = 13,
                FacilityID = "123123",
                FacilityName = "RahulFacility",
                IsActive = true
            };
            var censusViewModel = new CensusViewModel
            {
                CensusData = new List<CensusData>
                {
                    new CensusData
                    {
                    CensusId="1",
                    Medicad=10,
                    Medicare=10,
                    Other=10,
                    Year=censusViewRequest.Year,
                    FacilityName="RahulFacility",
                    Month="7"
                    }
                }
            };

            var totalCount = censusViewModel.CensusData.Count;

            mockCensusRepo.Setup(repo => repo.GetCencusList(It.IsAny<int>(), It.IsAny<int>(), out totalCount, censusViewRequest, It.IsAny<string>()))
                .Returns(censusViewModel.CensusData);

            _unitOfWorkMock.Setup(uow => uow.CensusRepo).Returns(mockCensusRepo.Object);

            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("facilityname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            // Act
            var result = _reviewControllerMock.GetCensus(censusViewRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as dynamic;

            Assert.NotNull(data);
            var dataObj = Assert.IsAssignableFrom<List<CensusData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(censusViewModel.CensusData.Count(), dataObj.Count());
        }

        [Fact]
        public void GetCensus_Throws_Exception()
        {
            // Arrange
            var censusViewRequest = new CensusViewRequest
            {
                FacilityID = "13",
                ReportQuarter = 4,
                Year = 2023,
            };

            var totalRecord = 0;
            var filterRecord = 0;
            var censusViewModel = new CensusViewModel();

            var mockCensusRepo = new Mock<ICensus>(); // Assuming you have a census repository interface

            // Set up mock data and method calls for throwing an exception
            var pageSize = 10;
            var start = "0";
            var length = "10";
            var sortOrder = "facilityName_asc";

            // Configure the mock to throw an exception when GetCencusList is called
            mockCensusRepo.Setup(repo => repo.GetCencusList(It.IsAny<int>(), It.IsAny<int>(), out It.Ref<int>.IsAny, censusViewRequest, It.IsAny<string>()))
                .Throws(new Exception("An error occured while getting census data"));

            _unitOfWorkMock.Setup(uow => uow.CensusRepo).Returns(mockCensusRepo.Object);


            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("facilityname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            // Act and Assert
            var result = _reviewControllerMock.GetCensus(censusViewRequest) as JsonResult;

            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as dynamic;

            Assert.NotNull(data);
            var dataObj = Assert.IsAssignableFrom<List<CensusData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(censusViewModel.CensusData.Count(), dataObj.Count());
        }

        [Fact]
        public void GetStaffingData_Returns_JsonResult()
        {
            // Arrange
            var csvRequest = new StaffingDepartmentCsvRequest
            {
                FacilityID = 13,
                ReportQuarter = 4,
                Year = 2023,
            };

            // Set up mock data and method calls for successful scenario
            var pageSize = 10;
            var start = "0";
            var length = "10";
            var sortOrder = "facilityName_asc";
            var totalRecord = 2;
            var filterRecord = 2;
            var facilityDetails = new FacilityModel
            {
                Id = 13,
                FacilityID = "123123",
                FacilityName = "RahulFacility",
                IsActive = true
            };

            var mockTimesheetRepo = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            int StaffDeptTotalCount = 2; // Replace with the expected total count
            var expectedData = new List<StaffingDepartmentDetails> {
                new StaffingDepartmentDetails
                {
                    Title = "Administrator",
                    TotalHours = "100",
                    StaffCount = "2",
                    Exempt = "1", NonExempt = "0", Contractors = "1", FacilityId = "13", FacilityName = "RahulFacility"
                },
                new StaffingDepartmentDetails
                {
                    Title = "Medical Director",
                    TotalHours = "200",
                    StaffCount = "2",
                    Exempt = "0", NonExempt = "1", Contractors = "1", FacilityId = "13", FacilityName = "RahulFacility"
                },
            };

            mockTimesheetRepo.Setup(repo => repo.GetStaffingDepartmentDetails(
                csvRequest,
                It.IsAny<int>(),
                It.IsAny<int>(),
                out StaffDeptTotalCount,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(expectedData);


            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("facilityname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            // Act
            var result = _reviewControllerMock.GetStaffingData(csvRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as dynamic;

            Assert.NotNull(data);
            var dataObj = Assert.IsAssignableFrom<List<StaffingDepartmentDetails>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
        }

        [Fact]
        public void GetStaffingData_Throws_Exception()
        {
            // Arrange
            var csvRequest = new StaffingDepartmentCsvRequest
            {
                FacilityID = 13,
                ReportQuarter = 4,
                Year = 2023,
            };

            // Set up mock data and method calls for successful scenario
            var pageSize = 10;
            var start = "0";
            var length = "10";
            var sortOrder = "facilityName_asc";
            var totalRecord = 0;
            var filterRecord = 0;
            var facilityDetails = new FacilityModel
            {
                Id = 13,
                FacilityID = "123123",
                FacilityName = "RahulFacility",
                IsActive = true
            };

            var mockTimesheetRepo = new Mock<ITimesheetRepository>();
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            int StaffDeptTotalCount = 0;
            mockTimesheetRepo.Setup(repo => repo.GetStaffingDepartmentDetails(
                csvRequest,
                It.IsAny<int>(),
                It.IsAny<int>(),
                out StaffDeptTotalCount,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Throws(new Exception("An error occured while getting staff department details data"));



            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("facilityname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            // Act
            var result = _reviewControllerMock.GetStaffingData(csvRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as dynamic;

            Assert.NotNull(data);
            var dataObj = Assert.IsAssignableFrom<List<StaffingDepartmentDetails>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
        }

        [Fact]
        public void GetEmployeeData_Returns_JsonResult()
        {
            // Arrange
            var employeeList = new EmployeeListRequest
            {
                FacilityId = "10",
                ReportQuarter = "4",
                Year = "2023",
                Month = "7"
            };

            // Set up mock data and method calls for successful scenario
            var pageSize = 10;
            var start = "0";
            var length = "10";
            var sortOrder = "facilityName_asc";
            var totalRecord = 2;
            var filterRecord = 2;

            var mockEmployeeRepo = new Mock<IEmployee>();
            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);

            var expectedEmployeeData = new List<EmployeeData>
            {
                new EmployeeData()
                {
                    Id = 1,
                    EmployeeId = "2002",
                    FacilityId = "10",
                    JobTitle = "16",
                    PayType = "1",
                    FirstName = "Rahul",
                    LastName = "Pawar",
                    HireDate = DateTime.UtcNow.ToString(),
                    TerminationDate = DateTime.UtcNow.ToString(),
                    CreateDate = DateTime.UtcNow.ToString(),
                    IsActive = true
                },
                new EmployeeData()
                {
                    Id = 4,
                    EmployeeId = "1001",
                    FacilityId = "10",
                    JobTitle = "16",
                    PayType = "2",
                    FirstName = "abc",
                    LastName = "xyz",
                    HireDate = DateTime.UtcNow.ToString(),
                    TerminationDate = DateTime.UtcNow.ToString(),
                    CreateDate = DateTime.UtcNow.ToString(),
                    IsActive = true
                },
            };

            var totalEmployeeCount = expectedEmployeeData.Count;
            mockEmployeeRepo.Setup(repo => repo.GetActiveEmployeesData(
                It.IsAny<int>(),
                It.IsAny<int>(),
                employeeList,
                out totalEmployeeCount,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(expectedEmployeeData);



            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("facilityname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            // Act
            var result = _reviewControllerMock.EmployeeData(employeeList);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var data = jsonResult.Value as dynamic;

            Assert.NotNull(data);
            var dataObj = Assert.IsAssignableFrom<List<EmployeeData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(expectedEmployeeData.Count, dataObj.Count);
        }


        [Fact]
        public void EmployeeData_Throws_Exception()
        {
            // Arrange
            var employeeListRequest = new EmployeeListRequest
            {
                FacilityId = "10",
                ReportQuarter = "4",
                Year = "2023",
                Month = "7"
            };

            var start = "0";
            var length = "10";

            var mockEmployeeRepo = new Mock<IEmployee>();

            int totalRecord = 0;
            int filterRecord = 0;

            var employeeData = new List<EmployeeData>();

            var totalEmployeeCount = employeeData.Count;

            mockEmployeeRepo.Setup(repo => repo.GetActiveEmployeesData(
                It.IsAny<int>(),
                It.IsAny<int>(),
                employeeListRequest,
                out totalEmployeeCount,
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Throws(new Exception("An error occured while getting employees data"));

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);


            // Simulate request data
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues(start) },
                { "length", new StringValues(length) },
                { "search[value]", new StringValues("") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "columns[0][data]", new StringValues("firstname") }
            });

            _reviewControllerMock.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Form = formCollection } }
            };

            var result = _reviewControllerMock.EmployeeData(employeeListRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<EmployeeData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(employeeData.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetEmployeeData_Returns_Valid_JsonResult()
        {
            // Arrange
            var timesheetDataRequest = new EmployeeTimesheetDataRequest
            {
                FacilityId = 13,
                ReportQuarter = 4,
                Year = 2023,
                Month = 7
            };
            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Replace with your repository interface
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            var expectedTimesheet = new Timesheet
            {
                EmployeeId = "1001",
                FacilityId = 13,
                AgencyId = 0,
                ReportQuarter = 4,
                Month = 7,
                FirstName = "Rahul",
                LastName = "Pawar",
                UploadType = 3,
                THours = 15,
                JobTitleCode = 1,
                PayTypeCode = 1,
                FileDetailId = 8,
                Status = 2,
                CreateDate = DateTime.UtcNow,
                Createby = "061d2430-9d89-4382-a1dd-2fe01e46d435",
                UpdateBy = "061d2430-9d89-4382-a1dd-2fe01e46d435",
                UpdateDate = DateTime.UtcNow,
                IsActive = true
            };
            mockTimesheetRepo.Setup(repo => repo.GetEmpTimesheetData(timesheetDataRequest)).ReturnsAsync(expectedTimesheet);

            // Act
            var result = await _reviewControllerMock.GetEmployeeData(timesheetDataRequest);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var timesheet = Assert.IsType<Timesheet>(jsonResult.Value);
            Assert.Same(expectedTimesheet, timesheet);
        }

        [Fact]
        public async Task GetEmployeeData_ThrowsException()
        {
            // Arrange
            var timesheetDataRequest = new EmployeeTimesheetDataRequest
            {
                FacilityId = 13,
                ReportQuarter = 4,
                Year = 2023,
                Month = 7
            };
            var mockTimesheetRepo = new Mock<ITimesheetRepository>(); // Replace with your repository interface
            _unitOfWorkMock.Setup(uow => uow.TimesheetRepo).Returns(mockTimesheetRepo.Object);

            var expectedTimesheet = new Timesheet();
            mockTimesheetRepo.Setup(repo => repo.GetEmpTimesheetData(timesheetDataRequest)).Throws(new Exception("An error occured while getting employees data"));

            // Act
            var result = await _reviewControllerMock.GetEmployeeData(timesheetDataRequest);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var timesheet = Assert.IsType<Timesheet>(jsonResult.Value);
            Assert.NotSame(expectedTimesheet, timesheet);
        }
    }
}