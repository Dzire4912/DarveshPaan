using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;
using TANWeb.Areas.PBJSnap.Controllers;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class SubmitControllerMock
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SubmitController _submitControllerMock;
        private Mock<UserManager<AspNetUser>> _userManagerMock;
        private Mock<ClaimsPrincipal> _claimsMock;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        public SubmitControllerMock()
        {
            _claimsMock = new Mock<ClaimsPrincipal>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManagerMock = GetUserManagerMock();
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
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            mockUow.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(new List<FacilityModel>());
            var result = await controller.Index() as ViewResult;
            Assert.NotNull(result);
            var viewModel = result?.Model as SubmitModel;
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task LoadIndexPageReturnViewResult_Throws_Exception()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            mockUow.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).Throws(new Exception("An error occured while getting facility details"));
            var result = await controller.Index() as ViewResult;
            Assert.NotNull(result);
            var viewModel = result?.Model as SubmitModel;
            Assert.NotNull(viewModel);
            Assert.Empty(viewModel.FacilityList);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForNursingHomeData))]
        public void GenerateXMLFile_Returns_File(NursingHomeData nursing)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            GenerateXml generateXml = new GenerateXml()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0,
                FileName = "Testing"
            };

            History history = new History()
            {
                FileName = "Testing",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0
            };

            mockUow.Setup(r => r.TimesheetRepo.GenerateSubmitXMLFile(generateXml)).ReturnsAsync(nursing);
            mockUow.Setup(r => r.HistoryRepo.AddHistory(history)).ReturnsAsync(true);
            var mockFacilityRepo = new Mock<IFacilityRepository>();

            mockFacilityRepo.Setup(repo => repo.GetAll()).Returns(new List<FacilityModel>
{
    new FacilityModel { Id = 1, OrganizationId = 100 }
}.AsQueryable());
            mockUow.Setup(uow => uow.FacilityRepo).Returns(mockFacilityRepo.Object);
            var result = controller.GenerateXMLFile(generateXml).Result;
            Assert.NotNull(result);
            var file = Assert.IsType<FileStreamResult>(result);
            Assert.NotEqual(0, file.FileStream.Length);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForNursingHomeData))]
        public void GenerateXMLFile_Throws_Exception_Returns_FileEmptyFile(NursingHomeData nursing)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            GenerateXml generateXml = new GenerateXml()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0,
                FileName = "Testing"
            };

            History history = new History()
            {
                FileName = "Testing",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0
            };

            mockUow.Setup(r => r.TimesheetRepo.GenerateSubmitXMLFile(generateXml)).Throws(new Exception("An error occured while generating xml"));
            mockUow.Setup(r => r.HistoryRepo.AddHistory(history)).ReturnsAsync(true);
            var mockFacilityRepo = new Mock<IFacilityRepository>();

            mockFacilityRepo.Setup(repo => repo.GetAll()).Returns(new List<FacilityModel>
{
    new FacilityModel { Id = 1, OrganizationId = 100 }
}.AsQueryable());
            mockUow.Setup(uow => uow.FacilityRepo).Returns(mockFacilityRepo.Object);
            var result = controller.GenerateXMLFile(generateXml).Result;
            Assert.NotNull(result);
            var file = Assert.IsType<VirtualFileResult>(result);
            Assert.Empty(file.FileDownloadName);
            Assert.Empty(file.FileName);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForNursingHomeData))]
        public void GenerateXMLFile_ThrowsException(NursingHomeData nursing)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            GenerateXml generateXml = new GenerateXml()
            {
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0,
                FileName = "Testing"
            };

            History history = new History()
            {
                FileName = "Testing",
                FacilityId = 1,
                Year = 2023,
                ReportQuarter = 4,
                Month = 0
            };

            mockUow.Setup(r => r.TimesheetRepo.GenerateSubmitXMLFile(generateXml)).ReturnsAsync(nursing);
            mockUow.Setup(r => r.HistoryRepo.AddHistory(history)).ReturnsAsync(true);
            var mockFacilityRepo = new Mock<IFacilityRepository>();

            mockFacilityRepo.Setup(repo => repo.GetAll()).Returns(new List<FacilityModel>
{
    new FacilityModel { Id = 1, OrganizationId = 100 }
}.AsQueryable());
            mockUow.Setup(uow => uow.FacilityRepo).Throws(new Exception("An error occured while getting facility details"));
            var result = controller.GenerateXMLFile(generateXml).Result;
            Assert.NotNull(result);
            var file = Assert.IsType<FileStreamResult>(result);
            Assert.NotEqual(0, file.FileStream.Length);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForAddFileNameAndStatusCode))]
        public async Task AddFileName_Returns_BoolStatus(GenerateXml xml, bool ExpectedStatus)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            mockUow.Setup(r => r.HistoryRepo.AddHistory(It.IsAny<History>())).ReturnsAsync(ExpectedStatus);

            var mockFacilityRepo = new Mock<IFacilityRepository>();
            
            mockFacilityRepo.Setup(repo => repo.GetAll()).Returns(new List<FacilityModel>
{
    new FacilityModel { Id = 1, OrganizationId = 100 }
}.AsQueryable());
            mockUow.Setup(uow => uow.FacilityRepo).Returns(mockFacilityRepo.Object);

            var result = await controller.AddFileName(xml);
            Assert.NotNull(result);
            Assert.Equal(ExpectedStatus, result);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForAddFileNameAndStatusCodeForException))]
        public async Task AddFileName_Returns_BoolStatus_Throws_Exception(GenerateXml xml, bool ExpectedStatus)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser")
            }));
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            var controller = new SubmitController(mockUow.Object, _userManagerMock.Object, httpContextAccessorMock.Object);

            mockUow.Setup(r => r.HistoryRepo.AddHistory(It.IsAny<History>())).ReturnsAsync(ExpectedStatus);

            var mockFacilityRepo = new Mock<IFacilityRepository>();

            mockFacilityRepo.Setup(repo => repo.GetAll()).Returns(new List<FacilityModel>
{
    new FacilityModel { Id = 1, OrganizationId = 100 }
}.AsQueryable());
            mockUow.Setup(uow => uow.FacilityRepo).Throws(new Exception("An error occured while getting facility deatails"));

            var result = await controller.AddFileName(xml);
            Assert.NotNull(result);
            Assert.Equal(ExpectedStatus, result);
        }

        public static IEnumerable<object[]> MockTestDataForNursingHomeData()
        {
            yield return new object[] { SetupNursingHomeData() };
        }

        public static IEnumerable<object[]> MockTestDataForAddFileNameAndStatusCode()
        {
            yield return new object[] { new GenerateXml() { FacilityId = 1, Year = 2023, ReportQuarter = 4, Month = 0, FileName = "Testing" }, true };
        }

        public static IEnumerable<object[]> MockTestDataForAddFileNameAndStatusCodeForException()
        {
            yield return new object[] { new GenerateXml() { FacilityId = 1, Year = 2023, ReportQuarter = 4, Month = 0, FileName = "Testing" }, false };
        }
        public static NursingHomeData SetupNursingHomeData()
        {
            NursingHomeData nursing = new NursingHomeData();
            nursing.Header.StateCode = "AL";
            nursing.Header.FacilityId = "Facility101";
            nursing.Header.ReportQuarter = 3;
            nursing.Header.FederalFiscalYear = 2023;
            nursing.Header.SoftwareVendorName = "Think Anew";
            nursing.Header.SoftwareVendorEmail = "sales@pbjsnap.com";
            nursing.Header.SoftwareProductName = "PBJSNAP";
            nursing.Header.SoftwareProductVersion = "1.4.2";

            EmployeeIdData employeeIdData = new EmployeeIdData();
            employeeIdData.EmployeeId = "Emp001";
            nursing.Employees = new List<EmployeeIdData> { employeeIdData };

            HourEntry hourEntry = new HourEntry();
            hourEntry.JobTitleCode = 10;
            hourEntry.Hours = 10.0;
            hourEntry.PayTypeCode = 1;

            HourEntries hourEntries = new HourEntries();
            hourEntries.HourEntry = new List<HourEntry> { hourEntry };

            WorkDay workDay = new WorkDay();
            workDay.HourEntries = hourEntries;
            workDay.Date = DateTime.Now;

            WorkDays workDays = new WorkDays();
            workDays.WorkDayList = new List<WorkDay> { workDay };

            StaffHours staffHours = new StaffHours();
            staffHours.WorkDays = workDays;
            staffHours.EmployeeId = "Emp001";

            nursing.StaffingHours = new List<StaffHours> { staffHours };

            return nursing;
        }

    }
}
