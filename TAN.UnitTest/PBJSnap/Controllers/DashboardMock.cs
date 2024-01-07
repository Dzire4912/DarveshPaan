using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using Moq;
using SimpleFeedReader;
using System.Net;
using System.Security.Claims;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.UnitTest.Helpers;
using TANWeb.Areas.TelecomReporting.Controllers;
using TANWeb.Controllers;
using HomeController = TANWeb.Controllers.HomeController;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using ViewResult = Microsoft.AspNetCore.Mvc.ViewResult;
using TANWeb.Resources;


namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class DashboardMock
    {
        private readonly HomeController _controller;
        private readonly Mock<SignInManager<AspNetUser>> _signInManagerMock;
        private readonly Mock<UserManager<AspNetUser>> _userManagerMock;
        private readonly Mock<RoleManager<AspNetRoles>> _roleManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConfiguration> _configuration;

        public DashboardMock()
        {
            _userManagerMock = GetUserManagerMock();
            _signInManagerMock = GetMockSignInManager();
            _roleManagerMock = GetRoleManagerMock();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configuration = new Mock<IConfiguration>();
            _controller = new HomeController(_userManagerMock.Object, _signInManagerMock.Object, _unitOfWorkMock.Object, _roleManagerMock.Object, _configuration.Object);
        }
        #region Primary
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
        private Mock<RoleManager<AspNetRoles>> GetRoleManagerMock()
        {
            var roleStoreMock = new Mock<IRoleStore<AspNetRoles>>();
            var roleValidators = new List<IRoleValidator<AspNetRoles>>();

            return new Mock<RoleManager<AspNetRoles>>(
                roleStoreMock.Object,
                roleValidators,
                null,
                null,
                null
            );
        }

        private Mock<SignInManager<AspNetUser>> GetMockSignInManager()
        {

            var ctxAccessor = new HttpContextAccessor();
            var mockClaimsPrinFact = new Mock<IUserClaimsPrincipalFactory<AspNetUser>>();
            return new Mock<SignInManager<AspNetUser>>(_userManagerMock.Object, ctxAccessor, mockClaimsPrinFact.Object, null, null, null, null);
        }
        #endregion

        #region Index
        [Fact]
        public async Task IndexReturnsThreeGraphs()
        {
            //Arrange
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            var org = new UserOrganizationFacility();
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test" } };
            var paytypes = new List<PayTypeCodes>() { new PayTypeCodes { PayTypeCode=1,PayTypeDescription="Exempt"},
                new PayTypeCodes{PayTypeCode=2,PayTypeDescription="NonExemot"},
                new PayTypeCodes{PayTypeDescription="Contract",PayTypeCode=3}
            };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            //_unitOfWorkMock.Setup(o => o.UserOrganizationFacilitiesRepo.GetAll()).Returns(org);
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };

            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);

            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.PayTypeCodesRepo.GetAll()).Returns(paytypes);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourData(It.IsAny<int>())).ReturnsAsync(labourcode);
            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["ChartData"]);
            Assert.NotNull(result.ViewData["BarChartData"]);
            Assert.NotNull(result.ViewData["BarChartLabourData"]);


        }

        [Fact]
        public async Task IndexReturnsThreeGraphs_UserOrg()
        {
            //Arrange
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            var org = new UserOrganizationFacility();
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test", OrganizationId = 1, IsActive = true } };
            var paytypes = new List<PayTypeCodes>() { new PayTypeCodes { PayTypeCode=1,PayTypeDescription="Exempt"},
                new PayTypeCodes{PayTypeCode=2,PayTypeDescription="NonExemot"},
                new PayTypeCodes{PayTypeDescription="Contract",PayTypeCode=3}
            };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            //_unitOfWorkMock.Setup(o => o.UserOrganizationFacilitiesRepo.GetAll()).Returns(org);
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };
            var userOrgFacility = new List<UserOrganizationFacility>()
            {
                new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1,IsActive=true }
            };
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);

            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.PayTypeCodesRepo.GetAll()).Returns(paytypes);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourData(It.IsAny<int>())).ReturnsAsync(labourcode);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["ChartData"]);
            Assert.NotNull(result.ViewData["BarChartData"]);
            Assert.NotNull(result.ViewData["BarChartLabourData"]);


        }
        #endregion

        #region LabourCodetable

        [Fact]
        public async Task GetLabourcodeTableReturnspartialView()
        {
            var orgid = 0;
            var fId = 0;
            var yr = 0;
            var fiscal = 0;
            var month = 0;
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var userorg = new UserOrganizationFacility();
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test", IsActive = true } };
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };
            var expResult = new List<Itemlist>();
            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="test"}
            };
            // Object myobj = testfacility;
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(labourcode);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);

            //Act 
            var result = await _controller.GetLabourcodeTable() as PartialViewResult;
            var ar = result.ViewData["FacilityList"] as List<Itemlist>;
            var rModel = result.Model as List<Itemlist>;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("_LabourCodeTable", result.ViewName);
            Assert.IsType<PartialViewResult>(result);
            var check = Assert.IsType<List<Itemlist>>(ar);
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), ar.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Count, check.Count);

        }
        [Fact]
        public async Task GetLabourcodeTableReturnspartialView_OrgUser()
        {
            var orgid = 0;
            var fId = 0;
            var yr = 0;
            var fiscal = 0;
            var month = 0;
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var userorg = new UserOrganizationFacility();
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test", IsActive = true, OrganizationId = 1 } };
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };
            var expResult = new List<Itemlist>();
            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="test"}
            };
            // Object myobj = testfacility;
            var userOrgFacility = new List<UserOrganizationFacility>()
            {
                new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1,IsActive=true }
            };
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(labourcode);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);

            //Act 
            var result = await _controller.GetLabourcodeTable() as PartialViewResult;
            var ar = result.ViewData["FacilityList"] as List<Itemlist>;
            var rModel = result.Model as List<Itemlist>;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("_LabourCodeTable", result.ViewName);
            Assert.IsType<PartialViewResult>(result);
            var check = Assert.IsType<List<Itemlist>>(ar);
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), ar.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Count, check.Count);

        }
        #endregion

        #region TopJobCode

        [Fact]
        public async Task GetTopJobCodereturnsPartialview()
        {
            var orgid = 0;
            var fId = 0;
            var yr = 0;
            var fiscal = 0;
            var month = 0;
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var topjobs = new List<Itemlist>() { new Itemlist { Text = "nurse", Value = 1 }, new Itemlist { Text = "Admin", Value = 1 } };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test", IsActive = true } };
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetTopJobTitleTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(topjobs);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="test"}
            };

            //Act
            var result = await _controller.GetTopJobCode() as PartialViewResult;
            var viewbag = result.ViewData["FacilityList"] as List<Itemlist>;
            var rModel = result.Model as List<Itemlist>;
            //Assert
            Assert.NotNull(result);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_GetTopJobs", result.ViewName);
            var check = Assert.IsType<List<Itemlist>>(viewbag);
            Assert.Equal(topjobs, result.Model);
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), check.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Count, check.Count);

        }

        [Fact]
        public async Task GetTopJobCodereturnsPartialview_OrgUser()
        {
            var orgid = 0;
            var fId = 0;
            var yr = 0;
            var fiscal = 0;
            var month = 0;
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var topjobs = new List<Itemlist>() { new Itemlist { Text = "nurse", Value = 1 }, new Itemlist { Text = "Admin", Value = 1 } };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test", IsActive = true, OrganizationId = 1 } };
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetTopJobTitleTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(topjobs);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="test"}
            };
            var userOrgFacility = new List<UserOrganizationFacility>()
            {
                new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1,IsActive = true }

            };
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);

            //Act
            var result = await _controller.GetTopJobCode() as PartialViewResult;
            var viewbag = result.ViewData["FacilityList"] as List<Itemlist>;
            var rModel = result.Model as List<Itemlist>;
            //Assert
            Assert.NotNull(result);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_GetTopJobs", result.ViewName);
            var check = Assert.IsType<List<Itemlist>>(viewbag);
            Assert.Equal(topjobs, result.Model);
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), check.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Count, check.Count);

        }
        #endregion

        #region StaffingData
        [Fact]
        public async Task GetStaffingDatareturnsPartialView()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            string[] arr = new string[] { "testFacility", "myFacility" };
            int[] dt = new int[] { 12, 11 };
            var ctstaffdata = new ChartData() { Labels = arr, Data = dt };

            var staffdata = new List<Itemlist>() { new Itemlist { Text = "testFacility", Value = 12 }, new Itemlist { Text = "myFacility", Value = 11 } };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true },
            new FacilityModel { FacilityID = "100012", Id = 2, FacilityName = "myFacility", IsActive = true }};

            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="testFacility"},
                 new Itemlist{ Value = 2,Text="myFacility"},
            };
            var testyear = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="2023"}, new Itemlist{ Value = 2,Text="2022"},new Itemlist{ Value = 3,Text="2021"}
            };
            var testOrg = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="MyOrg"}
            };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };


            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetStaffingData(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(staffdata);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(Orglist);
            //Act
            var result = await _controller.GetStaffingData() as PartialViewResult;
            var viewbag = result.ViewData["Facility"] as List<Itemlist>;
            var viewbag1 = result.ViewData["Year"] as List<Itemlist>;
            var viewbag2 = result.ViewData["Organization"] as List<Itemlist>;
            var rModel = result.Model as ChartData;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_StaffingDataBar", result.ViewName);
            var check = Assert.IsType<List<Itemlist>>(viewbag);
            var check1 = Assert.IsType<List<Itemlist>>(viewbag1);
            var check2 = Assert.IsType<List<Itemlist>>(viewbag2);
            var check3 = Assert.IsType<ChartData>(result.Model);
            Assert.Equal(ctstaffdata.Labels.ToString(), check3.Labels.ToString());
            Assert.Equal(testyear.Select(item => item.Text).ToArray(), check1.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), check.Select(item => item.Text).ToArray());
            Assert.Equal(testOrg.Select(item => item.Text).ToArray(), check2.Select(item => item.Text).ToArray());

            Assert.Equal(testfacility.Count, check.Count);

        }

        [Fact]
        public async Task GetStaffingDatareturnsPartialView_OrgUser()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            string[] arr = new string[] { "testFacility", "myFacility" };
            int[] dt = new int[] { 12, 11 };
            var ctstaffdata = new ChartData() { Labels = arr, Data = dt };
            var userOrgFacility = new List<UserOrganizationFacility>()
            {new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1,IsActive=true }
            };
            var staffdata = new List<Itemlist>() { new Itemlist { Text = "testFacility", Value = 12 }, new Itemlist { Text = "myFacility", Value = 11 } };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true ,OrganizationId=1},
            new FacilityModel { FacilityID = "100012", Id = 2, FacilityName = "myFacility", IsActive = true,OrganizationId=1 }};

            var testfacility = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="testFacility"},
                 new Itemlist{ Value = 2,Text="myFacility"},
            };
            var testyear = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="2023"}, new Itemlist{ Value = 2,Text="2022"},new Itemlist{ Value = 3,Text="2021"}
            };
            var testOrg = new List<Itemlist>()
            {
                new Itemlist{ Value = 1,Text="MyOrg"}
            };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };


            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetStaffingData(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(staffdata);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(Orglist);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            //Act
            var result = await _controller.GetStaffingData() as PartialViewResult;
            var viewbag = result.ViewData["Facility"] as List<Itemlist>;
            var viewbag1 = result.ViewData["Year"] as List<Itemlist>;
            var viewbag2 = result.ViewData["Organization"] as List<Itemlist>;
            var rModel = result.Model as ChartData;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_StaffingDataBar", result.ViewName);
            var check = Assert.IsType<List<Itemlist>>(viewbag);
            var check1 = Assert.IsType<List<Itemlist>>(viewbag1);
            var check2 = Assert.IsType<List<Itemlist>>(viewbag2);
            var check3 = Assert.IsType<ChartData>(result.Model);
            Assert.Equal(ctstaffdata.Labels.ToString(), check3.Labels.ToString());
            Assert.Equal(testyear.Select(item => item.Text).ToArray(), check1.Select(item => item.Text).ToArray());
            Assert.Equal(testfacility.Select(item => item.Text).ToArray(), check.Select(item => item.Text).ToArray());
            Assert.Equal(testOrg.Select(item => item.Text).ToArray(), check2.Select(item => item.Text).ToArray());

            Assert.Equal(testfacility.Count, check.Count);

        }
        #endregion

        #region Pending Staffing Data
        [Fact]
        public async Task GetPendingStaffingdataReturnsPartialview()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            int percent = 67;
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetPendingStaffingdata(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(percent);
            //Act
            var result = await _controller.GetPendingStaffingdata() as PartialViewResult;
            var viewbag = result.ViewData["pendingStaffingdata"];
            var rModel = result.Model as List<Itemlist>;

            //Assert
            Assert.NotNull(viewbag);
            Assert.NotNull(result);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_GetPendingStaffingdata", result.ViewName);
            Assert.Equal(percent, viewbag);


        }
        #endregion

        #region ThinkAnewTilew

        [Fact]
        public async Task GetThinkanewTilesreturnsKPI()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            var userOrgFacility = new List<UserOrganizationFacility>()
            {new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1 }
            };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true }, };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            var agency = new List<AgencyModel>() { new AgencyModel { Id = 1, AgencyId = "100011", FacilityId = 1, AgencyName = "kapil" } };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    },
                      new AspNetRoles()
                    {
                        Name = "Contractor",
                        Id ="1"
                    }
            }.AsQueryable();

            //Act
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(Orglist);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.AgencyRepo.GetAll()).Returns(agency);
            _userManagerMock.Setup(_ => _.Users).Returns(users);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roleItemlist);
            //Act
            var result = await _controller.GetThinkanewTiles() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as List<TileViewModel>;
            //Assert
            Assert.NotNull(rmodel);
            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetThinkanewTiles", result.ViewName);






        }
        [Fact]
        public async Task GetSecondaryTilesreturnsKPI()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            var userOrgFacility = new List<UserOrganizationFacility>()
            {new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1 }
            };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true }, };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            var agency = new List<AgencyModel>() { new AgencyModel { Id = 1, AgencyId = "100011", FacilityId = 1, AgencyName = "kapil" } };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    },
                      new AspNetRoles()
                    {
                        Name = "Contractor",
                        Id ="1"
                    }
            }.AsQueryable();

            //Act
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(Orglist);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.AgencyRepo.GetAll()).Returns(agency);
            _userManagerMock.Setup(_ => _.Users).Returns(users);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roleItemlist);
            //Act
            var result = await _controller.GetSecondaryTiles() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as List<TileViewModel>;
            //Assert
            Assert.NotNull(rmodel);
            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetSecondaryTiles", result.ViewName);

        }
        [Fact]
        public async Task GetThinkanewTilesreturnsKPI_OrgUser()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            var userOrgFacility = new List<UserOrganizationFacility>()
            {new UserOrganizationFacility{
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1 }
            };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true, OrganizationId = 1 }, };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true, } };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            var agency = new List<AgencyModel>() { new AgencyModel { Id = 1, AgencyId = "100011", FacilityId = 1, AgencyName = "kapil" } };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    },
                      new AspNetRoles()
                    {
                        Name = "Contractor",
                        Id ="1"
                    }
            }.AsQueryable();

            //Act
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(Orglist);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.AgencyRepo.GetAll()).Returns(agency);
            _userManagerMock.Setup(_ => _.Users).Returns(users);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roleItemlist);
            //Act
            var result = await _controller.GetThinkanewTiles() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as List<TileViewModel>;
            //Assert
            Assert.NotNull(rmodel);
            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetThinkanewTiles", result.ViewName);



        }
        #endregion

        #region Blogs
        [Fact]
        public async Task GetCMSFeedReturnspartialview()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            List<Blog> blogs = new List<Blog>() { new Blog { date = DateTime.Now.ToString(), Title = "Blog 1", Description = "Content 1" },
            new Blog { date = DateTime.Now.AddDays(-1).ToString(), Title = "Blog 2", Description = "Content 2" }};

            List<string> feeds = new List<string>();
            var mockFeedReader = new Mock<FeedReader>();
            var sampleItems = new List<FeedItem>
            {
            new FeedItem { Title = "Blog 1", Content = "Content 1", LastUpdatedDate=DateTime.Now },
            new FeedItem { Title = "Blog 2", Content = "Content 2", LastUpdatedDate = DateTime.Now.AddDays(-1) }
            };
            // mockFeedReader.Setup(_ => _.RetrieveFeeds(It.IsAny<List<string>>())).Returns(sampleItems);

            //Act
            var result = await _controller.GetCMSFeed() as PartialViewResult;

            var rModel = result.Model as List<Blog>;
            //Assert
            Assert.NotNull(result.Model);
            Assert.NotNull(result);
            Assert.IsType<List<Blog>>(rModel);
            Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_GetCMSFeed", result.ViewName);




        }
        #endregion

        #region Dashboard facilities cards
        [Fact]
        public async Task GetDashboardFacilitiesReturnsPartialview()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", IsActive = true }, };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            //Act
            var result = await _controller.GetDashboardFacilities() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as List<DashboardFacilityViewModel>;
            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.IsType<PartialViewResult>(result);
            Assert.IsType<List<DashboardFacilityViewModel>>(result.Model);

        }


        [Fact]
        public async Task GetDashboardFacilities_FromOrgUser_ReturnsPartialview()
        {
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;
            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            var UserOrgfacility = new List<UserOrganizationFacility>() { new UserOrganizationFacility{
            UserId="64daf745-fab7-4854-bc11-7a22963cd50e",OrganizationID=1,FacilityID=1,IsActive=true},
            };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "testFacility", OrganizationId = 1, IsActive = true }, };
            var Orglist = new List<OrganizationModel>() { new OrganizationModel { OrganizationID = 1, OrganizationName = "MyOrg", IsActive = true } };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(UserOrgfacility);
            //Act
            var result = await _controller.GetDashboardFacilities() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as List<DashboardFacilityViewModel>;
            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.IsType<PartialViewResult>(result);
            Assert.IsType<List<DashboardFacilityViewModel>>(result.Model);

        }
        #endregion

        [Fact]
        public async Task GetLastQuarterSubmitHistory_returnsPartialView()
        {
            int test = 150;
            _unitOfWorkMock.Setup(_ => _.HistoryRepo.GetLastQuarterSubmitHistory(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(test);

            //Act
            var result = await _controller.GetLastQuarterSubmitHistory() as Microsoft.AspNetCore.Mvc.PartialViewResult;


            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetLastQuarterSubmitHistory", result.ViewName);
        }

        [Fact]
        public async Task GetOrgWisePendingSubmission_returnsView()
        {   //Arrange
            List<FacilityModel> facilities = new List<FacilityModel>();
            facilities.Add(new FacilityModel { Id = 1, FacilityID = "10011", FacilityName = "test5Facility", OrganizationId = 1 });
            List<OrganizationModel> organizationModels = new List<OrganizationModel>();
            organizationModels.Add(new OrganizationModel { OrganizationID = 1, OrganizationName = "testOrg" });
            organizationModels.Add(new OrganizationModel { OrganizationID = 2, OrganizationName = "testOrg2" });
            _unitOfWorkMock.Setup(_ => _.HistoryRepo.GetPendingSubmit(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(facilities);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(organizationModels);
            //Act
            var result = await _controller.GetOrgWisePendingSubmission() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as ChartData;

            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetOrgWisePendingSubmission", result.ViewName);

        }
        [Fact]
        public async Task GetOrgWiseSubmissionStatus_returnsView()
        {
            List<Itemlist> items = new List<Itemlist>();
            items.Add(new Itemlist { Text = "submitted", Value = 2 });
            items.Add(new Itemlist { Text = "Accepted", Value = 3 });

            List<OrganizationModel> organizationModels = new List<OrganizationModel>();
            organizationModels.Add(new OrganizationModel { OrganizationID = 1, OrganizationName = "testOrg" });
            organizationModels.Add(new OrganizationModel { OrganizationID = 2, OrganizationName = "testOrg2" });
            _unitOfWorkMock.Setup(_ => _.HistoryRepo.GetSubmitStatus(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(items);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(organizationModels);
            //Act
            var result = await _controller.GetOrgWiseSubmissionStatus() as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as ChartData;

            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetOrgWiseSubmissionStatus", result.ViewName);

        }
        [Fact]
        public async Task GetOrgWiseSubmissionStatus_returnsViewForOrg()
        {
            List<Itemlist> items = new List<Itemlist>();
            items.Add(new Itemlist { Text = "submitted", Value = 2 });
            items.Add(new Itemlist { Text = "Accepted", Value = 3 });

            List<OrganizationModel> organizationModels = new List<OrganizationModel>();
            organizationModels.Add(new OrganizationModel { OrganizationID = 1, OrganizationName = "testOrg" });
            organizationModels.Add(new OrganizationModel { OrganizationID = 2, OrganizationName = "testOrg2" });
            _unitOfWorkMock.Setup(_ => _.HistoryRepo.GetSubmitStatus(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(items);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(organizationModels);
            //Act
            var result = await _controller.GetOrgWiseSubmissionStatus(1) as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as ChartData;

            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetOrgWiseSubmissionStatus", result.ViewName);


        }

        [Fact]
        public async Task GetEmpDistribution_returnsData()
        {
            List<Itemlist> items = new List<Itemlist>();
            items.Add(new Itemlist { Text = "submitted", Value = 2 });
            items.Add(new Itemlist { Text = "Accepted", Value = 3 });

            List<OrganizationModel> organizationModels = new List<OrganizationModel>();
            organizationModels.Add(new OrganizationModel { OrganizationID = 1, OrganizationName = "testOrg" });
            organizationModels.Add(new OrganizationModel { OrganizationID = 2, OrganizationName = "testOrg2" });
            _unitOfWorkMock.Setup(_ => _.HistoryRepo.GetSubmitStatus(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(items);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(organizationModels);
            //Act
            var result = await _controller.GetOrgWiseSubmissionStatus(1) as Microsoft.AspNetCore.Mvc.PartialViewResult;
            var rmodel = result.Model as ChartData;

            Assert.NotNull(result);
            Assert.IsType<Microsoft.AspNetCore.Mvc.PartialViewResult>(result);
            Assert.Equal("_GetOrgWiseSubmissionStatus", result.ViewName);


        }

        [Fact]
        public async Task EmployeeDistributionBarchart_returnsData()
        {
            //Arrange
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            var org = new UserOrganizationFacility();
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test" } };
            var paytypes = new List<PayTypeCodes>() { new PayTypeCodes { PayTypeCode=1,PayTypeDescription="Exempt"},
                new PayTypeCodes{PayTypeCode=2,PayTypeDescription="NonExemot"},
                new PayTypeCodes{PayTypeDescription="Contract",PayTypeCode=3}
            };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            //_unitOfWorkMock.Setup(o => o.UserOrganizationFacilitiesRepo.GetAll()).Returns(org);
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };

            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);

            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.PayTypeCodesRepo.GetAll()).Returns(paytypes);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourData(It.IsAny<int>())).ReturnsAsync(labourcode);
            // Act
            var result = await _controller.EmployeeDistributionBarchart();

            // Assert
            Assert.NotNull(result);



        }

        [Fact]
        public async Task EmployeeLabourTypechart_returnsData()
        {

            //Arrange
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            var userOrgFacility = new List<UserOrganizationFacility>()
            {
                new UserOrganizationFacility
                {
                UserId = currentUser.Id,
                FacilityID = 1,
                OrganizationID = 1,IsActive=true
                }
            };
            var facilities = new List<FacilityModel>() { new FacilityModel { FacilityID = "100011", Id = 1, FacilityName = "test" } };
            var paytypes = new List<PayTypeCodes>() { new PayTypeCodes { PayTypeCode=1,PayTypeDescription="Exempt"},
                new PayTypeCodes{PayTypeCode=2,PayTypeDescription="NonExemot"},
                new PayTypeCodes{PayTypeDescription="Contract",PayTypeCode=3}
            };
            var emp = new List<Employee>() { new Employee { Id = 1, EmployeeId = "100011", FacilityId = 1, FirstName = "kapil", LastName = "test", JobTitleCode = 1, PayTypeCode = 1 } };
            _unitOfWorkMock.Setup(o => o.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgFacility);
            var labourcode = new List<Itemlist>() { new Itemlist { Value = 1, Text = "NursingService" } };

            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);

            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetAll()).Returns(emp);
            _unitOfWorkMock.Setup(_ => _.PayTypeCodesRepo.GetAll()).Returns(paytypes);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.GetEmployeeLabourData(It.IsAny<int>())).ReturnsAsync(labourcode);
            // Act
            var result = await _controller.EmployeeLabourTypechart();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LogoutReturns()
        {
            //Arrange
            var currentUser = new AspNetUser { Id = "64daf745-fab7-4854-bc11-7a22963cd50e", UserName = "mkapil@trellissoft.ai", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

            httpContextMock.Setup(_ => _.Request.Headers[It.IsAny<string>()]).Returns("Other");
            IPAddress ip = IPAddress.Parse("127.0.0.1");

            httpContextMock.Setup(_ => _.Connection.RemoteIpAddress).Returns(ip);
            var log = new List<LoginAudit>
            {
                new LoginAudit
               {
                 IPAddress = ip.ToString(), UserId = currentUser.Id, LoginAuditId = 1, LoginStatus = 1, LoginTime = DateTime.UtcNow.AddMinutes(-70),LogOutTime=null,UserAgent="Other"
               }
            };

            _unitOfWorkMock.Setup(_ => _.loginAuditRepo.GetAll()).Returns(log);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            _userManagerMock.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _signInManagerMock.Setup(_ => _.SignOutAsync());
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.loginAuditRepo.Update(It.IsAny<LoginAudit>()));

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.NotNull(result);

        }

        [Fact]
        public async Task LockScreen_ValidSession_ReturnsLockScreenView()
        {
            // Arrange
            var mockUser = new AspNetUser { Id = "ce658637-9dc8-4c78-a81e-db4ee4058971", FirstName = "Krunal", UserType = 1, DefaultAppId = 1, UserName = "krunalp@trellissoft.ai" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);
            _userManagerMock.Setup(u => u.FindByEmailAsync(mockUser.UserName)).ReturnsAsync(mockUser);

            var mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal();
            mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);
            //_configuration.Setup(x => x[TANResource.PBJSnap]).Returns("pbjsnap");
            //_configuration.Setup(x => x[TANResource.Inventory]).Returns("inventory");

            var controller = new HomeController(_userManagerMock.Object, _signInManagerMock.Object, _unitOfWorkMock.Object, _roleManagerMock.Object, _configuration.Object);

            // Ensure ControllerContext is initialized
            controller.ControllerContext ??= new ControllerContext();

            // Initialize a DefaultHttpContext and configure session
            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Session = new MockSession();
            controller.ControllerContext.HttpContext = defaultHttpContext;

            // Set up session validation to return true
            var sessionValidationResult = true;
            controller.ControllerContext.HttpContext.Items["IsSessionValid"] = sessionValidationResult;

            var apps = new List<Application>()
            {
                new Application
                {
                ApplicationId = 1,
                Name = "PBJSnap",
                IsActive = true
                }
            };

            _unitOfWorkMock.Setup(repo => repo.ApplicationRepo.GetAll()).Returns(apps);

            // Act
            var result = await controller.LockScreen() as RedirectResult;

            // Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal("/TelecomReporting/Home/Index", result.Url);
        }
    }
}

