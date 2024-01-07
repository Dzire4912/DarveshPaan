using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Areas.TANUserManagement.Controllers;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class UserRoleControllerMock
    {
        private readonly UserRolesController _controller;
        private readonly Mock<SignInManager<AspNetUser>> _signInManagerMock;
        private readonly Mock<UserManager<AspNetUser>> _userManagerMock;
        private readonly Mock<RoleManager<AspNetRoles>> _roleManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMemoryCache> _cache;
        public UserRoleControllerMock()
        {
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManagerMock = GetUserManagerMock();
            _roleManagerMock = GetRoleManagerMock();
            _signInManagerMock = GetMockSignInManager();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cache = new Mock<IMemoryCache>();
            _controller = new UserRolesController(_unitOfWorkMock.Object, _userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object, _cache.Object) { TempData = tempData.Object };

        }
        private Mock<SignInManager<AspNetUser>> GetMockSignInManager()
        {

            var ctxAccessor = new HttpContextAccessor();
            var mockClaimsPrinFact = new Mock<IUserClaimsPrincipalFactory<AspNetUser>>();
            return new Mock<SignInManager<AspNetUser>>(_userManagerMock.Object, ctxAccessor, mockClaimsPrinFact.Object, null, null, null, null);
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
        public async Task Index_returnsView()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k" };
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", null, null, null) as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(2, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForTANUser()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 1 };
            var role = new AspNetRoles { Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34", Name = "Admin", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34", Name = "Admin", ApplicationId = "1", RoleType = "TANApplicationUser", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);

            var result = await _controller.Index("1", null, null, "1") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(0, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForTANUserWithRoleIdParameter()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 1 };
            var role = new AspNetRoles { Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34", Name = "Admin", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "017ee98c-93f2-4aeb-8e13-45e4e3950c34", Name = "Admin", ApplicationId = "1", RoleType = "TANApplicationUser", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", "017ee98c-93f2-4aeb-8e13-45e4e3950c34", null, "1") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(0, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForOtherUser()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 2 };
            var role = new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", null, null, "2") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(2, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForOtherUserWithAppId()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 2 };
            var role = new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", null, "2", "2") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(2, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForOtherUserWithRoleId()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 2 };
            var role = new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", "b9fb917f-774a-454a-a935-c93be7b54de9", null, "2") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(2, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_returnsViewForOtherUserWithRoleIdAppId()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k", UserType = 2 };
            var role = new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2", NormalizedName = "ADMIN" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", "b9fb917f-774a-454a-a935-c93be7b54de9", "2", "2") as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Single(viewModel.UserRoles);
            Assert.Single(viewModel.RoleItems);
            Assert.Equal(2, viewModel.AppItems.Count);
            Assert.Equal(2, viewModel.UserTypesItems.Count);
        }

        [Fact]
        public async Task Index_ExceptionOccures()
        {
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = "3", Email = "user3@example.com", UserName = "user3", IsActive = false, DefaultAppId=1,FirstName="p",LastName="p"}
            }.AsQueryable();
            var user = new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true, DefaultAppId = 1, FirstName = "m", LastName = "k" };
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="1"
                },
                new  UserApplication
                {
                    Id= 1,ApplicationId=1,UserId="2"
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "1", UserId = "1" } }.AsQueryable();
            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception("An error occured while getting user details."));
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            var result = await _controller.Index("1", null, null, null) as ViewResult;

            var viewModel = result?.Model as ManageUserRolesViewModel;

            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.UserRoles);
            Assert.Null(viewModel.RoleItems);
            Assert.Null(viewModel.AppItems);
            Assert.Null(viewModel.UserTypesItems);
        }

        [Fact]
        public async Task Update_returnsredirect()
        {
            string userId = "24817521-30ff-4bf4-8512-c81743260255";
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = false, DefaultAppId=2,FirstName="Demo",LastName="user"}
            }.AsQueryable();
            var user = new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId = 2, FirstName = "Demo", LastName = "user" };
            var role = new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" }, new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 38,ApplicationId=1,UserId=userId
                },
                new  UserApplication
                {
                    Id= 39,ApplicationId=2,UserId=userId
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "b0563416-3a04-419e-98ff-650aa94f5086", UserId = userId } }.AsQueryable();
            var roleItems = roles.Select(role => new roleItemlist
            {
                Text = role.Name,
                Value = role.Id
            }).ToList();
            var appItems = AppList.Select(app => new appItemlist
            {
                Text = app.Name,
                Value = app.ApplicationId
            }).ToList();
            var userrolesModel = userRoles.Select(userrole => new UserRolesViewModel
            {
                RoleId = userrole.RoleId,
                Selected = true,
                AppId = "1"
            }).ToList();
            var roleModel = new ManageUserRolesViewModel
            {
                UserId = userId,
                RoleItems = roleItems,
                AppItems = appItems,
                UserRoles = userrolesModel
            };

            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles.ToList());
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);

            // Act
            var result = await _controller.Update(userId, roleModel);

            // Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserList", viewresult.ActionName);
            Assert.Equal("User", viewresult.ControllerName);
        }

        [Fact]
        public async Task UpdateUserRole_ExceptionOccures()
        {
            string userId = "24817521-30ff-4bf4-8512-c81743260255";
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = false, DefaultAppId=2,FirstName="Demo",LastName="user"}
            }.AsQueryable();
            var user = new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId = 2, FirstName = "Demo", LastName = "user" };
            var role = new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" }, new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 38,ApplicationId=1,UserId=userId
                },
                new  UserApplication
                {
                    Id= 39,ApplicationId=2,UserId=userId
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "b0563416-3a04-419e-98ff-650aa94f5086", UserId = userId } }.AsQueryable();
            var roleItems = roles.Select(role => new roleItemlist
            {
                Text = role.Name,
                Value = role.Id
            }).ToList();
            var appItems = AppList.Select(app => new appItemlist
            {
                Text = app.Name,
                Value = app.ApplicationId
            }).ToList();
            var userrolesModel = userRoles.Select(userrole => new UserRolesViewModel
            {
                RoleId = userrole.RoleId,
                Selected = true,
                AppId = "1"
            }).ToList();
            var roleModel = new ManageUserRolesViewModel
            {
                UserId = userId,
                RoleItems = roleItems,
                AppItems = appItems,
                UserRoles = userrolesModel
            };

            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception("An error occured while getting user details."));
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles.ToList());
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);

            // Act
            var result = await _controller.Update(userId, roleModel);

            // Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserList", viewresult.ActionName);
            Assert.Equal("User", viewresult.ControllerName);
        }

        [Fact]
        public async Task Update_ReturnsRedirectWithExceptionHandling()
        {
            string userId = "24817521-30ff-4bf4-8512-c81743260255";
            var users = new List<AspNetUser>
            {
                new AspNetUser { Id = "1", Email = "user1@example.com", UserName = "user1", IsActive = true ,DefaultAppId=1,FirstName="m",LastName="k"},
                new AspNetUser { Id = "2", Email = "user2@example.com", UserName = "user2", IsActive = true,DefaultAppId =1,FirstName="k",LastName= "m"},
                new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = false, DefaultAppId=2,FirstName="Demo",LastName="user"}
            }.AsQueryable();
            var user = new AspNetUser { Id = userId, Email = "ambition4912@gmail.com", UserName = "ambition4912@gmail.com", IsActive = true, DefaultAppId = 2, FirstName = "Demo", LastName = "user" };
            var role = new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "b0563416-3a04-419e-98ff-650aa94f5086", Name = "Admin1", ApplicationId = "1" }, new AspNetRoles { Id = "b9fb917f-774a-454a-a935-c93be7b54de9", Name = "Admin", ApplicationId = "2" } }.AsQueryable();
            var userAppList = new List<UserApplication>() {
                new  UserApplication
                {
                    Id= 38,ApplicationId=1,UserId=userId
                },
                new  UserApplication
                {
                    Id= 39,ApplicationId=2,UserId=userId
                }
            }.AsQueryable();
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
            }.AsQueryable();
            var userRoles = new List<AspNetUserRoles>() { new AspNetUserRoles { Id = 1, RoleId = "b0563416-3a04-419e-98ff-650aa94f5086", UserId = userId } }.AsQueryable();
            var roleItems = roles.Select(role => new roleItemlist
            {
                Text = role.Name,
                Value = role.Id
            }).ToList();
            var appItems = AppList.Select(app => new appItemlist
            {
                Text = app.Name,
                Value = app.ApplicationId
            }).ToList();
            var userrolesModel = userRoles.Select(userrole => new UserRolesViewModel
            {
                RoleId = userrole.RoleId,
                Selected = true,
                AppId = "1"
            }).ToList();
            var roleModel = new ManageUserRolesViewModel
            {
                RoleItems = roleItems,
                AppItems = appItems,
                UserRoles = userrolesModel
            };

            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.GetAll()).Returns(userRoles.ToList());
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.GetAll()).Returns(userAppList);
            _unitOfWorkMock.Setup(_ => _.AspNetUserRoleRepo.Add(It.IsAny<AspNetUserRoles>())).Throws(new Exception("An error occured while adding new roles for user"));
            _unitOfWorkMock.Setup(_ => _.UserApplicationRepo.Add(It.IsAny<UserApplication>())).Throws(new Exception("An error occured while adding new application for role of user"));

            // Act
            var result = await _controller.Update(userId, roleModel);

            // Assert
            Assert.NotNull(result);
            var viewresult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserList", viewresult.ActionName);
            Assert.Equal("User", viewresult.ControllerName);
        }

        [Fact]
        public void GetTanRoles_ReturnsJsonResultWithRoleList()
        {
            // Arrange
            var roleList = new List<AspNetRoles>
            {
                new AspNetRoles { Id = "d86525a5-777a-40f3-af32-098caabaf487", Name = "SuperAdmin", RoleType = RoleTypes.TANApplicationUser.ToString() },
                new AspNetRoles { Id = "b45410a0-425c-46af-9b69-ddadae73953d", Name = "Admin", RoleType = RoleTypes.TANApplicationUser.ToString() }
            };

            _roleManagerMock.Setup(repo => repo.Roles).Returns(roleList.AsQueryable());

            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roleList);

            // Act
            var result = _controller.GetTanRoles() as JsonResult;
            var roleListResult = result.Value as List<roleItemlist>;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            Assert.NotNull(roleListResult);
            Assert.Equal(roleList.Count, roleListResult.Count);
        }

        [Fact]
        public void GetTanRoles_ExceptionOccures()
        {
            // Arrange
            var roleList = new List<AspNetRoles>
            {
        new AspNetRoles { Id = "d86525a5-777a-40f3-af32-098caabaf487", Name = "SuperAdmin", RoleType = RoleTypes.TANApplicationUser.ToString() },
        new AspNetRoles { Id = "b45410a0-425c-46af-9b69-ddadae73953d", Name = "Admin", RoleType = RoleTypes.TANApplicationUser.ToString() }
            };

            // Simulate an exception when calling GetAll on AspNetRolesRepo
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Throws(new Exception("An error occured while getting roles."));

            _roleManagerMock.Setup(repo => repo.Roles).Returns(roleList.AsQueryable());

            // Act
            var result = _controller.GetTanRoles() as JsonResult;
            var roleListResult = result.Value as List<roleItemlist>;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            Assert.Null(roleListResult); // Expecting a null result due to the exception
        }
    }
}
