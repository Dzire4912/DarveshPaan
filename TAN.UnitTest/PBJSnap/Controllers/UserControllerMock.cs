using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Security.Claims;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Areas.TANUserManagement.Controllers;
using TANWeb.Helpers;
using TANWeb.Models;
using IEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class UserControllerMock
    {
        private readonly UserController _controller;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<RoleManager<AspNetRoles>> _roleManager;
        private readonly Mock<IMailContentHelper> _mailContentHelper;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IEmailSender> _emailSender;
        private Mock<IEmailHelper> _emailHelper;
        private readonly IPAddress _trueIpAddress;
        IPAddress trueIpAddress = null;
        public UserControllerMock()
        {

            _userManager = GetUserManagerMock();
            _roleManager = GetRoleManagerMock();

            _emailSender = new Mock<IEmailSender>();
            _mailContentHelper = new Mock<IMailContentHelper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configuration = new Mock<IConfiguration>();
            _emailHelper = new Mock<IEmailHelper>();
            _controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object)
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



        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsync()
        {

            // Arrange
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "1", UserEmail = "user1@example.com", UserName = "user1", ActiveStatus = true ,FirstName="m",LastName="k",Role="Admin"},
                new User { Id = "2", UserEmail = "user2@example.com", UserName = "user2", ActiveStatus = true ,FirstName="k",LastName= "m",Role="Admin"},
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "user3", ActiveStatus = false ,FirstName="p",LastName="p",Role="Admin"}
            };
            var mylist = new List<User>
            {
                new User { Id = "1", UserEmail = "user1@example.com", UserName = "user1", ActiveStatus = true ,FirstName="m",LastName="k",Role="Admin"},
                new User { Id = "2", UserEmail = "user2@example.com", UserName = "user2", ActiveStatus = true ,FirstName="k",LastName= "m",Role="Admin"},
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "user3", ActiveStatus = false ,FirstName="p",LastName="p",Role="Admin"}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5" };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    //new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                     Id=1,
                     Name="Other"
                    }
            }.AsQueryable();

            var userOrgfacilities = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility()
                {
                    Id=1,UserId="1",FacilityID=1,OrganizationID=1
                },
                 new UserOrganizationFacility()
                {
                    Id=2,UserId="2",FacilityID=1,OrganizationID=1
                }
            }.AsQueryable();

            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgfacilities);
            //  _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);

            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);

            controller.TempData = tempDataMock.Object;
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;

            // Act
            var result = await controller.UserList(null, null, null, null) as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.RoleList.Count(), viewModel.RoleList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
            Assert.Equal(expectedViewModel.users.Count(), viewModel.users.Count());
        }

        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsyncWithSearchbyFirstName()
        {

            // Arrange
            var mylist = new List<User>
            {
               new User { Id = "3", UserEmail = "user3@example.com", UserName = "user3", ActiveStatus = false ,FirstName="Demo",LastName="p",Role="Admin"}

            };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="Demo",LastName="p"}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "user3", ActiveStatus = false ,FirstName="Demo",LastName="p",Role="Admin"}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5" };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    //new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                   Id=1,
                   Name="Other"
                    }
            }.AsQueryable();
            var userOrgfacilities = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility()
                {
                    Id=1,UserId="1",FacilityID=1,OrganizationID=1
                },
                 new UserOrganizationFacility()
                {
                    Id=2,UserId="2",FacilityID=1,OrganizationID=1
                }
            }.AsQueryable();
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgfacilities);
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);

            controller.TempData = tempDataMock.Object;
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;

            // Act
            var result = await controller.UserList("Demo", null, null, null) as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.RoleList.Count(), viewModel.RoleList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
            Assert.Equal(expectedViewModel.users.Count(), viewModel.users.Count());
        }

        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsyncWithSearchbyUserName()
        {

            // Arrange
            var mylist = new List<User>
            {
                     new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin"}
   };

            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="Demo",LastName="user"}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin"}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5" };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    //new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                   Id=1,
                   Name="Other"
                    }
            }.AsQueryable();

            var userOrgfacilities = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility()
                {
                    Id=1,UserId="1",FacilityID=1,OrganizationID=1
                },
                 new UserOrganizationFacility()
                {
                    Id=2,UserId="2",FacilityID=1,OrganizationID=1
                }
            }.AsQueryable();
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgfacilities);
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);
            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UserList("Demo user", null, null, null) as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.RoleList.Count(), viewModel.RoleList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
            Assert.Equal(expectedViewModel.users.Count(), viewModel.users.Count());
        }

        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsyncWithSearchbyUserRole()
        {

            // Arrange

            var mylist = new List<User>
            {
                        new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin"}
  };

            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="Demo",LastName="user"}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin"}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5" };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com", UserType = 1 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    //new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                   Id=1,
                   Name="Other"
                    }
            }.AsQueryable();
            var userOrgfacilities = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility()
                {
                    Id=1,UserId="1",FacilityID=1,OrganizationID=1
                },
                 new UserOrganizationFacility()
                {
                    Id=2,UserId="2",FacilityID=1,OrganizationID=1
                }
            }.AsQueryable();
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrgfacilities);
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);

            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UserList("Demo user", "Admin", null, null) as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.RoleList.Count(), viewModel.RoleList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
            Assert.Equal(expectedViewModel.users.Count(), viewModel.users.Count());
        }

        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsyncWithSearchbyUserOrganization()
        {

            // Arrange

            var mylist = new List<User>
            {
               new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin",UserType=2}

             };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", Email = "ambition4912@gmail.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="Demo",LastName="user", UserType = 2}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin",UserType=2}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5", UserType = 2 };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", UserName = "ambition4912@gmail.com", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    //new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true,
                     OrganizationId=1
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                   Id=1,
                   Name="Other"
                    }
            }.AsQueryable();

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);

            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);

            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UserList("Demo user", "Admin", "1", null) as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.RoleList.Count(), viewModel.RoleList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
        }

        [Fact]
        public async Task UserList_ReturnsViewWithUserViewModelAsyncWithSearchbyUserOrganizationAndFacility()
        {

            // Arrange

            var mylist = new List<User>
            {
               new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin",UserType=2,OrgId=1,FacilityId=10}

             };
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", Email = "ambition4912@gmail.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="Demo",LastName="user", UserType = 2}
            }.AsQueryable();
            var list = new List<User>
            {
                new User { Id = "3", UserEmail = "user3@example.com", UserName = "Demo user", ActiveStatus = false ,FirstName="Demo",LastName="user",Role="Admin",UserType=2}
            };
            var appUser = new AspNetUser() { UserName = "TestUser", Id = "5", UserType = 2 };
            var roleClaims = new[] { "Admin", "Local" };
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();
            var currentUser = new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", UserName = "ambition4912@gmail.com", UserType = 2 };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(currentUser.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            var expectedViewModel = new UserViewModel
            {
                users = list,
                OrgList = new List<Itemlist>
                {
                    new Itemlist { Text = "OrganizationOne1", Value = 1 }
                },
                FacilityList = new List<Itemlist>
                {
                    new Itemlist { Text = "DemoFacility", Value = 10 }
                },
                RoleList = new List<roleItemlist>
                {

                    new roleItemlist { Text = "Admin", Value ="1" }
                },
                ApplicationList = new List<Itemlist>
                {
                    new Itemlist { Text = "PBJSnap", Value = 1 }
                },
                UserTypesList = new List<Itemlist>
                {
                    new Itemlist { Text = "Other", Value = 1 }
                }
            };
            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     FacilityID="10",
                     FacilityName="DemoFacility",
                     IsActive=true,
                     OrganizationId=1
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap",
                    IsActive=true
                    }
            }.AsQueryable();
            var OrgList = new List<OrganizationModel>
            {
                    new OrganizationModel()
                    {
                    OrganizationID=1,
                    OrganizationName="OrganizationOne1",
                    IsActive=true
                    }
            }.AsQueryable();
            var userTypeList = new List<UserType>
            {
                    new UserType()
                    {
                   Id=1,
                   Name="Other"
                    }
            }.AsQueryable();

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.filterorgUsers(It.IsAny<List<User>>())).ReturnsAsync(list);

            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(currentUser);
            _userManager.Setup(_ => _.GetRolesAsync(It.IsAny<AspNetUser>())).ReturnsAsync(roleClaims);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(OrgList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            tempDataDictionary["AddUserId"] = "btnAddUser";
            tempDataDictionary["fromFacility"] = "10";
            httpContextMock.SetupGet(c => c.Items)
              .Returns(new Dictionary<object, object> { { "Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory", tempDataDictionary } });

            var btnAddUser = (dynamic)null;
            var fromFacility = (dynamic)null;
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.Setup(td => td.TryGetValue("AddUserId", out btnAddUser)).Returns(true);
            tempDataMock.Setup(td => td.TryGetValue("fromFacility", out fromFacility)).Returns(true);

            var sessionMock = new Mock<ISession>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
            _unitOfWorkMock.Setup(_ => _.EmployeeRepo.listUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(mylist);

            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempDataMock.Object;

            // Act
            var result = await controller.UserList(null, null, "1", "10") as ViewResult;
            var viewModel = result?.Model as UserViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/UserList.cshtml", result.ViewName);
            Assert.Equal(expectedViewModel.OrgList.Count(), viewModel.OrgList.Count());
            Assert.Equal(expectedViewModel.FacilityList.Count(), viewModel.FacilityList.Count());
            Assert.Equal(expectedViewModel.ApplicationList.Count(), viewModel.ApplicationList.Count());
            Assert.Equal(expectedViewModel.UserTypesList.Count(), viewModel.UserTypesList.Count());
            Assert.Equal(expectedViewModel.users.Count,viewModel.users.Count());
        }

        [Fact]
        public async Task AddTanUsers_returnSuccess()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "1",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=101,ApplicationId=1}, new FacilityApplicationModel{Id=1,FacilityId=102,ApplicationId=2}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "1", new string[] { "1" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("success", viewresult.Value);

        }

        [Fact]
        public async Task AddOtherUsers_HandleMailException()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).Throws(new Exception("An Error occured while sending mail"));


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "2", new string[] { "10" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("", viewresult.Value);

        }

        [Fact]
        public async Task AddOtherUsers_UserExists()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).Throws(new Exception("An Error occured while sending mail"));
            _userManager.Setup(um => um.FindByEmailAsync("ambition4912@gmail.com"))
                   .ReturnsAsync(new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255" });


            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "ambition4912@gmail.com", "2", new string[] { "10" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("exist", viewresult.Value);

        }

        [Fact]
        public async Task AddTanUsers_HandleException()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "1",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=101,ApplicationId=1}, new FacilityApplicationModel{Id=1,FacilityId=102,ApplicationId=2}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail
            _unitOfWorkMock.Setup(_ => _.SaveChanges()).Throws(new Exception("An error occured"));

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "1", new string[] { "1" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("", viewresult.Value);

        }

        [Fact]
        public async Task AddOtherUsers_CreateUserNotSucceeded()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "1",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=101,ApplicationId=1}, new FacilityApplicationModel{Id=1,FacilityId=102,ApplicationId=2}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(um => um.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Create user failed" }));
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "1", new string[] { "1" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("", viewresult.Value);

        }

        [Fact]
        public async Task AddOtherUsers_HandleException()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _unitOfWorkMock.Setup(_ => _.SaveChanges()).Throws(new Exception("An error occured"));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "2", new string[] { "10" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("", viewresult.Value);

        }

        [Fact]
        public async Task UpdateTANUser_returnsuccess()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 1

            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=101,ApplicationId=1}, new FacilityApplicationModel{Id=1,FacilityId=102,ApplicationId=2}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                }
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            // Create a mock of TempData
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();

            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success).Verifiable();
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object)
            {
                TempData = tempData.Object,
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                },
            };
            string[] apps = new string[1];
            apps[0] = "1";

            var result = await controller.UpdateUser(user.UserName, user.FirstName, user.LastName, user.Email, apps, user.DefaultAppId.ToString(), true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("UserList", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_returnsuccessMyPeofile()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                }
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success).Verifiable();
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser(user.UserName, user.FirstName, user.LastName, user.Email, apps, user.DefaultAppId.ToString(), true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("MyProfile", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_returnsuccessUserList()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var resultuser = new AspNetUser()
            {
                Id = "24817521",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                }
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(resultuser)).ReturnsAsync(IdentityResult.Success).Verifiable();
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser(user.UserName, user.FirstName, user.LastName, user.Email, apps, user.DefaultAppId.ToString(), true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("UserList", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_returnsuccessWithDifferentParameters()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                UserType = 2
            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=2,UserId=user.Id
                },
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success).Verifiable();
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser("ambition4912@gmail.com", "Demo", "user", "ambition4912@gmail.com", apps, "2", true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("MyProfile", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_ExceptionOccures()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                UserType = 2
            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=2,UserId=user.Id
                },
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success).Verifiable();
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges()).Throws(new Exception("An error occured"));
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser("ambition4912@gmail.com", "Demo", "user", "ambition4912@gmail.com", apps, "2", true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("UserList", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_UpdateUserFailedMyprofile()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                }
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(It.IsAny<AspNetUser>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update user failed" }));
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser(user.UserName, user.FirstName, user.LastName, user.Email, apps, user.DefaultAppId.ToString(), true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("MyProfile", result.ActionName);
        }

        [Fact]
        public async Task UpdateOtherUser_UpdateUserFailedUserList()
        {
            var user = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var resultuser = new AspNetUser()
            {
                Id = "24817521-30ff-4bf4-8512-c81743260255",
                FirstName = "Demo",
                LastName = "user",
                Email = "ambition4912@gmail.com",
                UserName = "ambition4912@gmail.com",
                DefaultAppId = 2,
                UserType = 2

            };
            var facAppList = new List<FacilityApplicationModel>()
            {
                 new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var facilityList = new List<FacilityModel>()
            {
                new FacilityModel{Id=10,FacilityName = "DemoFacility",IsActive=true,OrganizationId=1},
                new FacilityModel{Id=11,FacilityName = "DemoFacility1",IsActive=true,OrganizationId=1},
            }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId=user.Id
                }
            };

            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };
            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("1"); // Mocking _userManager.GetUserIdAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(resultuser);
            _userManager.Setup(_ => _.FindByIdAsync(user.Id)).ReturnsAsync(resultuser);
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _userManager.Setup(_ => _.UpdateAsync(It.IsAny<AspNetUser>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update user failed" }));
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.SaveChanges());
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            string[] apps = new string[2];
            apps[0] = "10";
            apps[1] = "11";

            var result = await controller.UpdateUser(user.UserName, user.FirstName, user.LastName, user.Email, apps, user.DefaultAppId.ToString(), true, user.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("MyProfile", result.ActionName);
        }

        [Fact]
        public async Task UpdateActiveStatus_returnsSuccess()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.UpdateAsync(It.IsAny<AspNetUser>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            var result = await controller.UpdateActiveStatus("1", "1");
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("success", jsonResult.Value);
        }

        [Fact]
        public async Task UpdateActiveStatus_ExceptionOccures()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.UpdateAsync(It.IsAny<AspNetUser>())).Throws(new Exception("An error occured while updating user status")).Verifiable();
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            var result = await controller.UpdateActiveStatus("1", "1");
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("", jsonResult.Value);
        }

        [Fact]
        public async Task EditUser_returnsView()
        {
            //Arrange
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k",},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId=2,FirstName="Demo",LastName="user",UserType=2}
            }.AsQueryable();

            var currentUser = new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255" };

            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     Id=11,
                     FacilityName="DemoFacility1",OrganizationId =1,IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap"
                    }, new Application()
                    {
                    ApplicationId=2,
                    Name="Inventory"
                    }
            }.AsQueryable();
            var userAppList = new List<UserApplication>
            {
                    new UserApplication()
                    {
                      ApplicationId=1,
                       UserId="1"
                    }
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=11,
                IsActive=true
            }
            }.AsQueryable();

            var orgList = new List<OrganizationModel>
            { new OrganizationModel
            {
                OrganizationID=1,
                OrganizationName="OrganizationOne1",
                IsActive=true
            }
            }.AsQueryable();

            var facilityApplicationList = new List<FacilityApplicationModel>
            { new FacilityApplicationModel
            {
                Id=2,
                FacilityId=11,
                ApplicationId=1,
                IsActive=true
            }
            }.AsQueryable();

            var userTypeList = new List<UserType>
            {new UserType
            {
                Id=1,
                Name="ThinkAnew"
            },
            new UserType
            {
                Id=2,
                Name="Other"
            }
            }.AsQueryable();

            _userManager.Setup(_ => _.Users).Returns(users);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(orgList);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityApplicationList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
               .ReturnsAsync(currentUser);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            //Act
            var result = await controller.EditUser("24817521-30ff-4bf4-8512-c81743260255") as ViewResult;
            var viewModel = result?.Model as EditUserViewModel;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/EditUser.cshtml", result.ViewName);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task EditMyProfile_returnsView()
        {
            //Arrange
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId=2,FirstName="Demo",LastName="user",UserType=2}
            }.AsQueryable();

            var currentUser = new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255" };

            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     Id=11,
                     FacilityName="DemoFacility1",OrganizationId =1,IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap"
                    }, new Application()
                    {
                    ApplicationId=2,
                    Name="Inventory"
                    }
            }.AsQueryable();
            var userAppList = new List<UserApplication>
            {
                    new UserApplication()
                    {
                      ApplicationId=1,
                       UserId="24817521-30ff-4bf4-8512-c81743260255"
                    }
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=11,
                IsActive=true
            }
            }.AsQueryable();

            var orgList = new List<OrganizationModel>
            { new OrganizationModel
            {
                OrganizationID=1,
                OrganizationName="OrganizationOne1",
                IsActive=true
            }
            }.AsQueryable();

            var facilityApplicationList = new List<FacilityApplicationModel>
            { new FacilityApplicationModel
            {
                Id=2,
                FacilityId=11,
                ApplicationId=1,
                IsActive=true
            }
            }.AsQueryable();

            var userTypeList = new List<UserType>
            {new UserType
            {
                Id=1,
                Name="ThinkAnew"
            },
            new UserType
            {
                Id=2,
                Name="Other"
            }
            }.AsQueryable();

            _userManager.Setup(_ => _.Users).Returns(users);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(orgList);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityApplicationList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
               .ReturnsAsync(currentUser);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            //Act
            var result = await controller.MyProfile() as ViewResult;
            var viewModel = result?.Model as EditUserViewModel;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/EditUser.cshtml", result.ViewName);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task EditMyProfile_ExceptionOccures()
        {
            //Arrange
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255", Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId=2,FirstName="Demo",LastName="user",UserType=2}
            }.AsQueryable();

            var currentUser = new AspNetUser { Id = "24817521-30ff-4bf4-8512-c81743260255" };

            var facilityList = new List<FacilityModel>
            {
                    new FacilityModel()
                    {
                     Id=11,
                     FacilityName="DemoFacility1",OrganizationId =1,IsActive=true
                    }
            }.AsQueryable();
            var AppList = new List<Application>
            {
                    new Application()
                    {
                    ApplicationId=1,
                    Name="PBJSnap"
                    }, new Application()
                    {
                    ApplicationId=2,
                    Name="Inventory"
                    }
            }.AsQueryable();
            var userAppList = new List<UserApplication>
            {
                    new UserApplication()
                    {
                      ApplicationId=1,
                       UserId="24817521-30ff-4bf4-8512-c81743260255"
                    }
            }.AsQueryable();
            var roleItemlist = new List<AspNetRoles>
            {
                    new AspNetRoles()
                    {
                        Name = "Admin",
                        Id ="1"
                    }
            }.AsQueryable();

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=11,
                IsActive=true
            }
            }.AsQueryable();

            var orgList = new List<OrganizationModel>
            { new OrganizationModel
            {
                OrganizationID=1,
                OrganizationName="OrganizationOne1",
                IsActive=true
            }
            }.AsQueryable();

            var facilityApplicationList = new List<FacilityApplicationModel>
            { new FacilityApplicationModel
            {
                Id=2,
                FacilityId=11,
                ApplicationId=1,
                IsActive=true
            }
            }.AsQueryable();

            var userTypeList = new List<UserType>
            {new UserType
            {
                Id=1,
                Name="ThinkAnew"
            },
            new UserType
            {
                Id=2,
                Name="Other"
            }
            }.AsQueryable();

            _userManager.Setup(_ => _.Users).Returns(users);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Throws(new Exception("An error occured while getting user applications."));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.FacilityRepo.GetAll()).Returns(facilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.OrganizationRepo.GetAll()).Returns(orgList);
            _roleManager.Setup(_ => _.Roles).Returns(roleItemlist);
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityApplicationList);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypeList);
            _userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
               .ReturnsAsync(currentUser);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            //Act
            var result = await controller.MyProfile() as ViewResult;
            var viewModel = result?.Model as EditUserViewModel;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("~/Areas/TANUserManagement/Views/User/EditUser.cshtml", result.ViewName);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void GetValues_RedirectsToUserList()
        {
            // Arrange
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "1",
                //Organization = "1",
                DefaultAppId = 1
            };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;

            var addUserId = "btnAddUser";
            var fromFacility = "10";

            // Act
            var result = controller.GetValues(addUserId, fromFacility) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UserList", result.ActionName);

        }

        [Fact]
        public async Task UpdateInActiveStatus_returnsSuccess()
        {
            //Arrange
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            _userManager.Setup(_ => _.Users).Returns(users);
            _userManager.Setup(_ => _.UpdateAsync(It.IsAny<AspNetUser>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            //Act
            var result = await controller.UpdateActiveStatus("1", "0");

            //Assert
            Assert.NotNull(result);
            var mockResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("success", mockResult.Value);

        }

        [Fact]
        public async Task AddOtherUsers_returnSuccess()
        {
            //Arrange
            //  string fName, string lName, string email, string UserType, string[] apps, string defaultApp, string defApp
            var user = new AspNetUser()
            {
                Id = "1",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1
            };
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            { new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();

            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.AddTANUserInfo("kapil", "dev", "dev@mailinator.com", "2", new string[] { "10" }, "1", "1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, null);
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("success", viewresult.Value);

        }

        [Fact]
        public async Task CheckForUniqueEmail_UserWithEmailExists_ReturnsTrueForDifferentUser()
        {
            // Arrange
            string emailId = "ambition4912@gmail.com";
            var userId = "24817521-30ff-4bf4-8512-c81743260255";
            var otherUserId = "24b7b08b-32b9-4d66-8892-f0357fd45169";
            _userManager.Setup(um => um.FindByEmailAsync(emailId))
                          .ReturnsAsync(new AspNetUser { Id = userId });

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = await controller.CheckForUniqueEmail(emailId, otherUserId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.True((bool)result.Value);
        }

        [Fact]
        public async Task CheckForUniqueEmail_UserWithEmailExists_ReturnsFalseForSameUser()
        {
            // Arrange
            string emailId = "ambition4912@gmail.com";
            var userId = "24817521-30ff-4bf4-8512-c81743260255";
            _userManager.Setup(um => um.FindByEmailAsync(emailId))
                          .ReturnsAsync(new AspNetUser { Id = userId });

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = await controller.CheckForUniqueEmail(emailId, userId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result.Value);
        }

        [Fact]
        public async Task CheckForUniqueEmail_UserNotExists_ReturnsFalse()
        {
            // Arrange
            string emailId = "test@example.com";
            var userId = "123";
            _userManager.Setup(um => um.FindByEmailAsync(emailId))
                          .ReturnsAsync((AspNetUser)null);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = await controller.CheckForUniqueEmail(emailId, userId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result.Value);
        }

        [Fact]
        public async Task CheckForUniqueEmail_HandleException()
        {
            // Arrange
            string emailId = "ambition4912@gmail.com";
            var userId = "24817521-30ff-4bf4-8512-c81743260255";

            _userManager.Setup(um => um.FindByEmailAsync(emailId))
                           .Throws(new Exception("An error occured while checking valid email"));

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = await controller.CheckForUniqueEmail(emailId, userId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result.Value);

            // You may want to add additional assertions here to verify the logging behavior.
        }

        [Fact]
        public void GetFacilityList_ReturnsFacilityList()
        {
            // Arrange
            var orgId = "1";
            var facilityListData = new[]
            {
                new FacilityModel { Id = 10, OrganizationId = 1, FacilityName = "DemoFacility", IsActive = true },
                new FacilityModel { Id = 11, OrganizationId = 1, FacilityName = "DemoFacility1", IsActive = true }
            }.AsQueryable();


            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll())
                          .Returns(facilityListData);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetFacilityList(orgId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var facilityList = result.Value as List<Itemlist>;
            Assert.NotNull(facilityList);
            Assert.Equal(2, facilityList.Count);

            var facility1 = facilityList.FirstOrDefault(f => f.Text == "DemoFacility");
            Assert.NotNull(facility1);
            Assert.Equal(10, facility1.Value);

            var facility2 = facilityList.FirstOrDefault(f => f.Text == "DemoFacility1");
            Assert.NotNull(facility2);
            Assert.Equal(11, facility2.Value);
        }

        [Fact]
        public void GetFacilityList_ReturnsEmptyFacilityListExceptionOccures()
        {
            // Arrange
            var orgId = "1";
            var facilityListData = new[]
            {
                new FacilityModel { Id = 10, OrganizationId = 1, FacilityName = "DemoFacility", IsActive = true },
                new FacilityModel { Id = 11, OrganizationId = 1, FacilityName = "DemoFacility1", IsActive = true }
            }.AsQueryable();


            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll()).Throws(new Exception("An error occured while getting facilities."));

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetFacilityList(orgId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var facilityList = result.Value as List<Itemlist>;
            Assert.Null(facilityList);
        }

        [Fact]
        public void GetFacilityApplicationList_ReturnsFilteredAppList()
        {
            // Arrange
            var apps = new string[] { "10", "11" };
            var facilityAppListData = new[]
            {
                new FacilityApplicationModel { Id = 1, FacilityId = 10, ApplicationId = 1, IsActive = true },
                new FacilityApplicationModel { Id = 2, FacilityId = 11, ApplicationId = 1, IsActive = true },
                new FacilityApplicationModel { Id = 3, FacilityId = 11, ApplicationId = 2, IsActive = true }
            }.AsQueryable();

            var appListData = new[]
            {
                new Application { ApplicationId = 1, Name = "PBJSnap" },
                new Application { ApplicationId = 2, Name = "Inventory" }
            }.AsQueryable();

            _unitOfWorkMock.Setup(uow => uow.FacilityApplicationRepo.GetAll())
                          .Returns(facilityAppListData);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll())
                          .Returns(appListData);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetFacilityApplicationList(apps) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var appList = result.Value as List<Itemlist>;
            Assert.NotNull(appList);
            Assert.Equal(2, appList.Count);

            var app1 = appList.FirstOrDefault(a => a.Text == "PBJSnap");
            Assert.NotNull(app1);
            Assert.Equal(1, app1.Value);

            var app2 = appList.FirstOrDefault(a => a.Text == "Inventory");
            Assert.NotNull(app2);
            Assert.Equal(2, app2.Value);
        }

        [Fact]
        public void GetFacilityApplicationList_ThrowsException()
        {
            // Arrange
            var apps = new string[] { "10", "11" };
            var facilityAppListData = new[]
            {
                new FacilityApplicationModel { Id = 1, FacilityId = 10, ApplicationId = 1, IsActive = true },
                new FacilityApplicationModel { Id = 2, FacilityId = 11, ApplicationId = 1, IsActive = true },
                new FacilityApplicationModel { Id = 3, FacilityId = 11, ApplicationId = 2, IsActive = true }
            }.AsQueryable();

            var appListData = new[]
            {
                new Application { ApplicationId = 1, Name = "PBJSnap" },
                new Application { ApplicationId = 2, Name = "Inventory" }
            }.AsQueryable();

            _unitOfWorkMock.Setup(uow => uow.FacilityApplicationRepo.GetAll())
                          .Throws(new Exception("An error occured in UserController while getting facility applications"));
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll())
                          .Returns(appListData);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetFacilityApplicationList(apps) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var appList = result.Value as List<Itemlist>;
            Assert.NotNull(appList);
            Assert.Equal(0, appList.Count);
        }

        [Fact]
        public void GetApplication_ReturnsJsonResultWithAppList()
        {
            // Arrange
            var appList = new List<Application>
            {
                new Application { ApplicationId = 1, Name = "App1" },
                new Application { ApplicationId = 2, Name = "App2" }
            };

            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(appList);

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetApplication() as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var appListResult = result.Value as List<Itemlist>;
            Assert.NotNull(appListResult);
            Assert.Equal(2, appListResult.Count);
            Assert.Equal("App1", appListResult[0].Text);
            Assert.Equal(1, appListResult[0].Value);
            Assert.Equal("App2", appListResult[1].Text);
            Assert.Equal(2, appListResult[1].Value);
        }

        [Fact]
        public void GetApplication_ReturnsEmptyJsonOnException()
        {
            // Arrange
            var appList = new List<Application>
            {
                new Application { ApplicationId = 1, Name = "App1" },
                new Application { ApplicationId = 2, Name = "App2" }
            };
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Throws(new Exception("An error occured while getting applications."));

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            // Act
            var result = controller.GetApplication() as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var appResult = result.Value as Application;
            Assert.NotNull(appResult);
        }

        [Fact]
        public void GetRoles_ReturnsJsonResultWithRoleList()
        {
            // Arrange
            var roles = new List<AspNetRoles>
            {
                new AspNetRoles { Id = "1", Name = "Role1", ApplicationId = "App1" },
                new AspNetRoles { Id = "2", Name = "Role2", ApplicationId = "App1" }
            };

            _unitOfWorkMock.Setup(uow => uow.AspNetRolesRepo.GetAll()).Returns(roles.AsQueryable());

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            var orgId = "App1";

            // Act
            var result = controller.GetRoles(orgId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var roleListResult = result.Value as List<roleItemlist>;
            Assert.NotNull(roleListResult);
            Assert.Equal(2, roleListResult.Count);
            Assert.Equal("Role1", roleListResult[0].Text);
            Assert.Equal("1", roleListResult[0].Value);
            Assert.Equal("Role2", roleListResult[1].Text);
            Assert.Equal("2", roleListResult[1].Value);
        }

        [Fact]
        public void GetRoles_ReturnsEmptyJsonOnException()
        {
            // Arrange
            var roles = new List<AspNetRoles>
            {
                new AspNetRoles { Id = "1", Name = "Role1", ApplicationId = "App1" },
                new AspNetRoles { Id = "2", Name = "Role2", ApplicationId = "App1" }
            };
            _unitOfWorkMock.Setup(uow => uow.AspNetRolesRepo.GetAll()).Throws(new Exception("An error occured while getting roles."));

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);

            var orgId = "App1";

            // Act
            var result = controller.GetRoles(orgId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var roleResult = result.Value as AspNetRoles;
            Assert.NotNull(roleResult);
        }

        [Fact]
        public async Task ResendReturnsSuccessAsync()
        {
            var user = new AspNetUser()
            {
                Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,

            };

            var resultuser = new List<AspNetUser>()
            {
                new AspNetUser
                {
                      Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,
                },
                 new AspNetUser
                {
                  Id = "15",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,
                 }
            }.AsQueryable();
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();
            _userManager.Setup(_ => _.GeneratePasswordResetTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("12345");
            _userManager.Setup(_ => _.ResetPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.Users).Returns(resultuser);
            Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> urlHelperMock = new Mock<Microsoft.AspNetCore.Mvc.IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(x => x);
            urlHelperMock.Setup(_ => _.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("callbackUrl");
            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail

            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.Resend("017ee98c-93f2-4aeb-8e13-45e4e3950c34");
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("New password has been sent", viewresult.Value);



        }


        public async Task ResendReturnsForgotPasswordLinkSuccessAsync()
        {
            var user = new AspNetUser()
            {
                Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,
                IsUsingTempPassword = 1
            };

            var resultuser = new List<AspNetUser>()
            {
                new AspNetUser
                {
                      Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,IsUsingTempPassword=1
                },
                 new AspNetUser
                {
                  Id = "15",
                FirstName = "kapil",
                LastName = "dev",
                Email = "kapil@mailinator.com",
                UserName = "kapil@mailinator.com",
                //Facility = "10",
                //Organization = "1",
                DefaultAppId = 1,
                 }
            }.AsQueryable();
            var facilityAppList = new List<FacilityApplicationModel>()
            {
                new FacilityApplicationModel{Id=1,FacilityId=10,ApplicationId=1,IsActive=true}, new FacilityApplicationModel{Id=1,FacilityId=11,ApplicationId=2,IsActive=true}
            };
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            };
            var AppList = new List<Application>()
            {
                new Application
                {
                    ApplicationId=1,
                    Name="PBJSnap"
                },
                new Application
                {
                    ApplicationId=2,
                    Name="Inventory"
                }
            };

            var userOrganizationFacilityList = new List<UserOrganizationFacility>
            {
                new UserOrganizationFacility
            {
                Id=1,
                UserId="24817521-30ff-4bf4-8512-c81743260255",
                OrganizationID=1,
                FacilityID=10,
                IsActive=true
            }
            }.AsQueryable();


            // Set up the mock to return the desired URL when Url.Page is called
            string code = "12345"; // Replace with your code value
                                   //urlHelperMock
                                   //    .Setup(url => url.Page("/Account/ResetPassword", null, new { area = "Identity", code }, It.IsAny<string>()))
                                   //    .Returns("http://example.com/resetpassword?code=" + code);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(url => url.Action(
                    "ResetPassword",
                    "Account",
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .Returns("/Identity/Account/ResetPassword?code=your_reset_code");




            _userManager.Setup(_ => _.GeneratePasswordResetTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("12345");
            _userManager.Setup(_ => _.ResetPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            //var currentUser = new AspNetUser { Id = "5", UserName = "testuser@example.com" };
            var httpContextMock = new Mock<HttpContext>();
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            userMock.Setup(u => u.Identity.Name).Returns(user.UserName);
            httpContextMock.Setup(h => h.User).Returns(userMock.Object);
            _userManager.Setup(_ => _.Users).Returns(resultuser);

            _userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), It.IsAny<Claim>())); // Mocking _userManager.AddClaimAsync
            _userManager.Setup(_ => _.GetUserIdAsync(It.IsAny<AspNetUser>())).ReturnsAsync("4"); // Mocking _userManager.GetUserIdAsync
            _userManager.Setup(_ => _.GenerateEmailConfirmationTokenAsync(It.IsAny<AspNetUser>())).ReturnsAsync("confirmationCode"); // Mocking _userManager.GenerateEmailConfirmationTokenAsync
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AspNetUser)null);
            _userManager.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            //_userManager.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetUser>(), new Claim("UserName", user.FirstName + " " + user.LastName)));
            _userManager.Setup(_ => _.GetUserAsync(httpContextMock.Object.User)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChanges());
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            _unitOfWorkMock.Setup(_ => _.FacilityApplicationRepo.GetAll()).Returns(facilityAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>()));
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>()));
            _configuration.Setup(_ => _.GetSection(It.IsAny<string>())).Returns(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "DomainPath"));
            _configuration.Setup(c => c.GetSection("DomainPath")["Server"]).Returns("https://pbjsnapweb01-qa.azurewebsites.net/");
            _configuration.Setup(c => c.GetSection("DomainPath")["LogoPath"]).Returns("~/assets/images/");
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.GetAll()).Returns(userOrganizationFacilityList);
            _unitOfWorkMock.Setup(_ => _.UserOrganizationFacilitiesRepo.Add(It.IsAny<UserOrganizationFacility>()));
            _emailHelper.Setup(_ => _.SendEmailAsync(It.IsAny<SendMail>())).ReturnsAsync(true); // Mocking _mailHelper.SendMail




            var controller = new UserController(_unitOfWorkMock.Object, _userManager.Object, _roleManager.Object, _emailSender.Object, _mailContentHelper.Object, _configuration.Object, _emailHelper.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;
            controller.TempData = tempData.Object;
            controller.Url = urlHelperMock.Object;


            //Act
            var result = await controller.Resend("017ee98c-93f2-4aeb-8e13-45e4e3950c34");
            //Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("New password has been sent", viewresult.Value);



        }
    }
}




