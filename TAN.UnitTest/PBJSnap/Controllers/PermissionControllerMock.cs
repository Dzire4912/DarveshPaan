using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Areas.TANUserManagement.Controllers;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class PermissionControllerMock
    {
        private readonly PermissionController _controller;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<RoleManager<AspNetRoles>> _roleManagerMock;
        private readonly Mock<IMemoryCache> _cache;
        public PermissionControllerMock() 
        {
            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            _userManager = GetUserManagerMock();
            _roleManagerMock = GetRoleManagerMock();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cache = new Mock<IMemoryCache>();
            _controller = new PermissionController(_unitOfWorkMock.Object, _roleManagerMock.Object, _cache.Object) { TempData = tempData.Object };
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
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var module=new List<Module>() { new Module { ApplicationId=1,ModuleId=1,Name= "UserManager" }, new Module { ApplicationId = 1, ModuleId = 2, Name = "RoleManager" } }.AsQueryable();
            var AppList = new List<Application>() { new Application { ApplicationId = 1, Name = "PBjsnap" }, new Application { ApplicationId = 2, Name = "inventory" } }.AsQueryable();
            var permList=new List<Permission>() { new Permission {ModuleId=1,Id=1,Name= "Permissions.UserManagement.PBJsnapView", IsActive=true }, new Permission { ModuleId = 2, Id = 2, Name = "Permissions.RoleMangement.PBJsnapView", IsActive = true } }.AsQueryable() ;
            var claimCollection = new[] { new Claim("Permissions", "Permissions.UserManagement.PBJsnapView"), new Claim("Permissions", "Permissions.RoleMangement.PBJsnapView") };
            _roleManagerMock.Setup(_=>_.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.ModuleRepo.GetAll()).Returns(module);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_=>_.PermissionsRepo.GetPermissions(It.IsAny<int>(),It.IsAny<int>())).Returns(permList);
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            var result = await _controller.Index("1") as ViewResult ;

            var viewModel = result?.Model as PermissionViewModel;
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal(2, viewModel.AppList.Count);
            Assert.Equal(2, viewModel.ModuleList.Count);
            Assert.Equal(1, viewModel.RoleList.Count);
        }

        [Fact]
        public async Task Index_returnsViewWithDifferentAppId()
        {
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" } }.AsQueryable();
            var module = new List<Module>() { new Module { ApplicationId = 1, ModuleId = 1, Name = "UserManager" }, new Module { ApplicationId = 1, ModuleId = 2, Name = "RoleManager" } }.AsQueryable();
            var AppList = new List<Application>() { new Application { ApplicationId = 1, Name = "PBjsnap" }, new Application { ApplicationId = 2, Name = "inventory" } }.AsQueryable();
            var permList = new List<Permission>() { new Permission { ModuleId = 1, Id = 1, Name = "Permissions.UserManagement.PBJsnapView", IsActive = true }, new Permission { ModuleId = 2, Id = 2, Name = "Permissions.RoleMangement.PBJsnapView", IsActive = true } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.UserManagement.PBJsnapView"), new Claim("Permissions", "Permissions.RoleMangement.PBJsnapView") };
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.ModuleRepo.GetAll()).Returns(module);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.PermissionsRepo.GetPermissions(It.IsAny<int>(), It.IsAny<int>())).Returns(permList);
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            var result = await _controller.Index("1") as ViewResult;

            var viewModel = result?.Model as PermissionViewModel;
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal(2, viewModel.AppList.Count);
            Assert.Equal(2, viewModel.ModuleList.Count);
            Assert.Equal(1, viewModel.RoleList.Count);
        }

        [Fact]
        public async Task Index_returnsViewWithModuleId()
        {
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" } }.AsQueryable();
            var module = new List<Module>() { new Module { ApplicationId = 1, ModuleId = 1, Name = "UserManager" }, new Module { ApplicationId = 1, ModuleId = 2, Name = "RoleManager" } }.AsQueryable();
            var AppList = new List<Application>() { new Application { ApplicationId = 1, Name = "PBjsnap" }, new Application { ApplicationId = 2, Name = "inventory" } }.AsQueryable();
            var permList = new List<Permission>() { new Permission { ModuleId = 1, Id = 1, Name = "Permissions.UserManagement.PBJsnapView", IsActive = true }, new Permission { ModuleId = 2, Id = 2, Name = "Permissions.RoleMangement.PBJsnapView", IsActive = true } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.UserManagement.PBJsnapView"), new Claim("Permissions", "Permissions.RoleMangement.PBJsnapView") };
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.ModuleRepo.GetAll()).Returns(module);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.PermissionsRepo.GetPermissions(It.IsAny<int>(), It.IsAny<int>())).Returns(permList);
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            var result = await _controller.Index("1",1) as ViewResult;

            var viewModel = result?.Model as PermissionViewModel;
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal(2, viewModel.AppList.Count);
            Assert.Equal(2, viewModel.ModuleList.Count);
            Assert.Equal(1, viewModel.RoleList.Count);
            Assert.Equal(2, viewModel.RoleClaims.Count);
        }

        [Fact]
        public async Task Index_ExceptionOccurs()
        {
            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "0" } }.AsQueryable();
            var module = new List<Module>() { new Module { ApplicationId = 1, ModuleId = 1, Name = "UserManager" }, new Module { ApplicationId = 1, ModuleId = 2, Name = "RoleManager" } }.AsQueryable();
            var AppList = new List<Application>() { new Application { ApplicationId = 1, Name = "PBjsnap" }, new Application { ApplicationId = 2, Name = "inventory" } }.AsQueryable();
            var permList = new List<Permission>() { new Permission { ModuleId = 1, Id = 1, Name = "Permissions.UserManagement.PBJsnapView", IsActive = true }, new Permission { ModuleId = 2, Id = 2, Name = "Permissions.RoleMangement.PBJsnapView", IsActive = true } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.UserManagement.PBJsnapView"), new Claim("Permissions", "Permissions.RoleMangement.PBJsnapView") };
            _roleManagerMock.Setup(_ => _.Roles).Returns(roles);
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception("An Error Occured in Permission Controller"));
            _unitOfWorkMock.Setup(_ => _.ModuleRepo.GetAll()).Returns(module);
            _unitOfWorkMock.Setup(_ => _.ApplicationRepo.GetAll()).Returns(AppList);
            _unitOfWorkMock.Setup(_ => _.PermissionsRepo.GetPermissions(It.IsAny<int>(), It.IsAny<int>())).Returns(permList);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            var result = await _controller.Index("1", 1) as ViewResult;

            var viewModel = result?.Model as PermissionViewModel;
            Assert.NotNull(result);
            Assert.NotNull(viewModel);
            Assert.Equal(2, viewModel.AppList.Count);
            Assert.Equal(2, viewModel.ModuleList.Count);
            Assert.Equal(1, viewModel.RoleList.Count);
        }


        [Fact]
        public async Task Update_returnsValid()
        {
            var roleClaim = new List<RoleClaimsViewModel>()
                {
                    new RoleClaimsViewModel()
                    {
                       Type = "Permissions",
                       Value = "Permissions.UserManagement.Admin",
                       Selected = true,
                       ModuleId = 1
                    }
            };

            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.UserManagement.PBJsnapView"), new Claim("Permissions", "Permissions.RoleMangement.PBJsnapView") };
            var model = new PermissionViewModel { RoleId = "1", RoleClaims = roleClaim };
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            _roleManagerMock.Setup(_=>_.RemoveClaimAsync(It.IsAny<AspNetRoles>(),It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            _roleManagerMock.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetRoles>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var result = await _controller.Update(model);
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Update_returnsValidWithDifferentParameters()
        {
            var roleClaim = new List<RoleClaimsViewModel>()
                {
                    new RoleClaimsViewModel()
                    {
                       Type = "Permissions",
                       Value = "Permissions.RoleManagement.PBJsnapView.View",
                       Selected = true,
                       ModuleId = 1
                    }
            };

            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.RoleManagement.PBJsnapView.View"), new Claim("Permissions", "Permissions.RoleManagement.PBJsnapView.View") };
            var model = new PermissionViewModel { RoleId = "1", RoleClaims = roleClaim };
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(claimCollection);
            _roleManagerMock.Setup(_ => _.RemoveClaimAsync(It.IsAny<AspNetRoles>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            _roleManagerMock.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetRoles>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var result = await _controller.Update(model);
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Update_ExceptionOccures()
        {
            var roleClaim = new List<RoleClaimsViewModel>()
                {
                    new RoleClaimsViewModel()
                    {
                       Type = "Permissions",
                       Value = "Permissions.RoleManagement.PBJsnapView.View",
                       Selected = true,
                       ModuleId = 1
                    }
            };

            var role = new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = "1", Name = "RoleName", ApplicationId = "1" } }.AsQueryable();
            var claimCollection = new[] { new Claim("Permissions", "Permissions.RoleManagement.PBJsnapView.View"), new Claim("Permissions", "Permissions.RoleManagement.PBJsnapView.View") };
            var model = new PermissionViewModel { RoleId = "1", RoleClaims = roleClaim };
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.GetClaimsAsync(It.IsAny<AspNetRoles>())).ThrowsAsync(new Exception("An Error Occured in Permission Controller while getting role claims"));
            _roleManagerMock.Setup(_ => _.RemoveClaimAsync(It.IsAny<AspNetRoles>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            _roleManagerMock.Setup(_ => _.AddClaimAsync(It.IsAny<AspNetRoles>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var result = await _controller.Update(model);
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
    }
}
