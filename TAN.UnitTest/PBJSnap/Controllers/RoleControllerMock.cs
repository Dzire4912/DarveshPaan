using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Areas.TANUserManagement.Controllers;
using TANWeb.Helpers;
using static TANWeb.Helpers.IMailHelper;
using Application = TAN.DomainModels.Entities.Application;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class RoleControllerMock
    {
        private readonly RolesController _controller;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<RoleManager<AspNetRoles>> _roleManagerMock;
        public RoleControllerMock()
        {

            Mock<ITempDataDictionary> tempData = new Mock<ITempDataDictionary>();
            var cuser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "TestUser"),
            }));

            _userManager = GetUserManagerMock();
            _roleManagerMock = GetRoleManagerMock();


            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object) { TempData = tempData.Object };


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
        public async Task Index_WithValidParameters_ReturnsView()
        {
            // Arrange
            string appId = "0";
            string roleId = "017ee98c-93f2-4aeb-8e13-45e4e3950c34";
            string id = "1";
            string roleType = "TANApplicationUser";
            string normalizedName = "ADMIN";

            var role = new AspNetRoles { Id = roleId, Name = "Admin", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "Admin", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);

            // Act
            var result = await controller.Index(appId, roleId, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("Admin", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_WithTANUserParameters_ReturnsView()
        {
            // Arrange
            string appId = "0";
            string roleId = "017ee98c-93f2-4aeb-8e13-45e4e3950c34";
            string id = "1";
            string roleType = "TANApplicationUser";
            string normalizedName = "ADMIN";

            var role = new AspNetRoles { Id = roleId, Name = "Admin", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "Admin", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(appId, null, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("Admin", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_WithOtherUserParameters_ReturnsView()
        {
            // Arrange
            string appId = "1";
            string roleId = "2daa0a55-ed5e-4418-ad96-6f25b220f911";
            string id = "2";
            string roleType = string.Empty;
            string normalizedName = "USER1";

            var role = new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(appId, roleId, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("User1", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_WithOtherUserDifferentParameters_ReturnsView()
        {
            // Arrange
            string appId = "1";
            string roleId = "2daa0a55-ed5e-4418-ad96-6f25b220f911";
            string id = "2";
            string roleType = string.Empty;
            string normalizedName = "USER1";

            var role = new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(appId, null, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("User1", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_WithOtherUserWithoutParameters_ReturnsView()
        {
            // Arrange
            string appId = "1";
            string roleId = "2daa0a55-ed5e-4418-ad96-6f25b220f911";
            string id = "2";
            string roleType = string.Empty;
            string normalizedName = "USER1";

            var role = new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(null, null, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("User1", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_WithOtherUserWithRoleIdParameters_ReturnsView()
        {
            // Arrange
            string appId = "1";
            string roleId = "2daa0a55-ed5e-4418-ad96-6f25b220f911";
            string id = "2";
            string roleType = string.Empty;
            string normalizedName = "USER1";

            var role = new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(role);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(null, roleId, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Equal("User1", model.RoleName);
            Assert.Single(model.RolesList);
            Assert.Single(model.ApplicationList);
            Assert.Single(model.AppRoleList);
        }

        [Fact]
        public async Task Index_ExceptionOccures()
        {
            // Arrange
            string appId = "1";
            string roleId = "2daa0a55-ed5e-4418-ad96-6f25b220f911";
            string id = "2";
            string roleType = string.Empty;
            string normalizedName = "USER1";

            var role = new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName };
            var roles = new List<AspNetRoles> { new AspNetRoles { Id = roleId, Name = "User1", ApplicationId = appId, RoleType = roleType, NormalizedName = normalizedName } }.AsQueryable();
            var applications = new List<Application> { new Application { ApplicationId = Convert.ToInt32(appId), Name = "AppName" } };
            var userTypes = new List<UserType> { new UserType { Id = 1, Name = "ThinkAnew" }, new UserType { Id = 2, Name = "Other" } };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ThrowsAsync(new Exception("An error occured in Roles Controller while getting role details"));
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(_ => _.AspNetRolesRepo.GetAll()).Returns(roles);
            _unitOfWorkMock.Setup(_ => _.UserTypeRepo.GetAll()).Returns(userTypes);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(roles);

            var controller = new RolesController(_unitOfWorkMock.Object, _roleManagerMock.Object);


            // Act
            var result = await controller.Index(null, roleId, id, roleId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RolesViewModel>(viewResult.Model);

            // Perform further assertions on the model and view
            Assert.Null(model.RoleName);
            Assert.Null(model.RolesList);
            Assert.Null(model.ApplicationList);
            Assert.Null(model.AppRoleList);
        }

        [Fact]
        public async Task GetRoleDetails_returnsJson()
        {
            string id = "1";
            var role = new AspNetRoles { Id = id, Name = "RoleName", ApplicationId = "1" };
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(id)).ReturnsAsync(role);
            var result = await _controller.GetRoleDetails(id) as JsonResult;
            Assert.NotNull(result);
            var viewModel = result.Value as RolesViewModel;

            Assert.NotNull(viewModel);
            Assert.Equal("RoleName", viewModel.RoleName);
        }

        [Fact]
        public async Task GetRoleDetails_ExceptionOccurs()
        {
            string id = "1";
            var role = new AspNetRoles { Id = id, Name = "RoleName", ApplicationId = "1" };
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(role.Id)).ThrowsAsync(new Exception("An error occured in Roles Controller while getting role details"));
            var result = await _controller.GetRoleDetails(id) as JsonResult;
            Assert.NotNull(result);
            var viewModel = result.Value as RolesViewModel;

            Assert.NotNull(viewModel);
            Assert.Null(viewModel.RoleName);
        }

        [Fact]
        public async Task Addrole_redirectToindex()
        {
            var param = new RolesViewModel()
            {
                RoleName = "Name",
                ApplicationName = "3"
            };
            var role = new AspNetRoles { Name = "Name", ApplicationId = "3" };
            _roleManagerMock.Setup(_ => _.CreateAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var result = await _controller.AddRole(param);
            Assert.NotNull(result);
            var viewresult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewresult.ActionName);
        }

        [Fact]
        public async Task AddRole_DuplicateRoleName_WarningMessage()
        {
            // Arrange
            var roleName = "Admin";
            var applicationName = "1";
            var roleId = "017ee98c-93f2-4aeb-8e13-45e4e3950c34";
            var model = new RolesViewModel
            {
                RoleId = roleId,
                RoleName = roleName,
                ApplicationName = applicationName
            };

            var identityResult = IdentityResult.Failed(new IdentityError { Code = IdentityRolesStatus.DuplicateRoleName.ToString() });
            _roleManagerMock.Setup(rm => rm.FindByNameAsync(roleName))
                           .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId))
                          .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<AspNetRoles>()))
                           .ReturnsAsync(identityResult);

            // Set rate limit exceeded in HttpContext.Items
            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            // Act
            var result = await _controller.AddRole(model);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task AddRole_AdditionFails_ReturnsJsonResultWithError()
        {
            // Arrange
            var roleId = "validRoleId";
            var roleName = "validRoleName";
            var applicationName = "SomeApplication";
            var model = new RolesViewModel
            {
                RoleId = roleId,
                RoleName = roleName,
                ApplicationName = applicationName
            };
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(new AspNetRoles { Id = roleId });

            // Set up a failed IdentityResult with an error description
            var errorDescription = "Role Adding failed";
            var failedResult = IdentityResult.Failed(new IdentityError { Description = errorDescription });
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(failedResult);
            // Set rate limit exceeded in HttpContext.Items
            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };


            // Act
            var result = await _controller.AddRole(model);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task AddRole_AdditionFails_ReturnsJsonResultWithErrorWithDifferentParameters()
        {
            // Arrange
            var roleId = "someRoleId";
            var applicationName = "SomeApplicationName";
            var model = new RolesViewModel
            {
                RoleId = roleId,
                ApplicationName = applicationName
            };

            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId))
                          .ReturnsAsync(new AspNetRoles { Id = roleId });
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(IdentityResult.Success).Verifiable();

            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;

            // Set rate limit exceeded in HttpContext.Items
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AddRole(model);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task AddRole_AdditionFails_ExceptionOccures()
        {
            var param = new RolesViewModel()
            {
                RoleName = "Name",
                ApplicationName = "3"
            };
            var role = new AspNetRoles { Name = "Name", ApplicationId = "3" };
            _roleManagerMock.Setup(_ => _.CreateAsync(It.IsAny<AspNetRoles>())).ThrowsAsync(new Exception("An error occured while creating new role.")).Verifiable();

            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;

            // Set rate limit exceeded in HttpContext.Items
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.AddRole(param);
            Assert.NotNull(result);
            var viewresult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewresult.ActionName);
        }

        [Fact]
        public async Task UpdateRole_retrurnJson()
        {
            var role = new AspNetRoles { Name = "Name", Id = "1", ApplicationId = "1" };
            _roleManagerMock.Setup(_ => _.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.UpdateAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(IdentityResult.Success).Verifiable();

            var result = await _controller.UpdateRole("Name", "1");
            Assert.NotNull(result);
            var jsonresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Record Saved !", jsonresult.Value);
        }

        [Fact]
        public async Task UpdateRole_DuplicateRoleName_ReturnsWarningJsonResult()
        {
            // Arrange
            var roleName = "Admin";
            var roleId = "017ee98c-93f2-4aeb-8e13-45e4e3950c34";
            var identityResult = IdentityResult.Failed(new IdentityError { Code = IdentityRolesStatus.DuplicateRoleName.ToString() });

            _roleManagerMock.Setup(rm => rm.FindByNameAsync(roleName))
                           .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId))
                          .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.UpdateAsync(It.IsAny<AspNetRoles>()))
                           .ReturnsAsync(identityResult);


            // Act
            var result = await _controller.UpdateRole(roleName, roleId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var respMsg = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal("RoleName is already exists!please try different role name.", respMsg);
        }

        [Fact]
        public async Task UpdateRole_ExceptionOccurs_LogsErrorAndReturnsEmptyJsonResult()
        {
            // Arrange
            var roleName = "NewRole";
            var roleId = "someRoleId";
            var exceptionMessage = "An error occurred while updating the role.";

            _roleManagerMock.Setup(rm => rm.FindByNameAsync(roleName))
                           .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId))
                          .ReturnsAsync(new AspNetRoles { Id = roleId, Name = roleName });
            _roleManagerMock.Setup(rm => rm.UpdateAsync(It.IsAny<AspNetRoles>()))
                           .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UpdateRole(roleName, roleId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var respMsg = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(string.Empty, respMsg);
        }

        [Fact]
        public async Task UpdateRole_UpdationFails_ReturnsJsonResultWithError()
        {
            // Arrange
            var roleId = "validRoleId";
            var roleName = "validRoleName";
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(new AspNetRoles { Id = roleId });

            // Set up a failed IdentityResult with an error description
            var errorDescription = "Update failed";
            var failedResult = IdentityResult.Failed(new IdentityError { Description = errorDescription });
            _roleManagerMock.Setup(rm => rm.UpdateAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(failedResult);


            // Act
            var result = await _controller.UpdateRole(roleName, roleId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Something went wrong ! Try again later.", result.Value);
        }

        [Fact]
        public async Task DeleteRole_resturnsSucces()
        {
            var role = new AspNetRoles { Id = "1", Name = "Name", ApplicationId = "1" };
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.DeleteAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(IdentityResult.Success).Verifiable();
            var result = await _controller.DeleteRole("1");
            Assert.NotNull(result);
            var jsonresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Record has been deleted successfully", jsonresult.Value);
        }

        [Fact]
        public async Task DeleteRole_WithInvalidId_ReturnsJsonResultWithError()
        {
            // Arrange
            var roleId = "invalidRoleId";
            // Act
            var result = await _controller.DeleteRole(roleId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Something went wrong ! Try again later.", result.Value);
        }

        [Fact]
        public async Task DeleteRole_DeletionFails_ReturnsJsonResultWithError()
        {
            // Arrange
            var roleId = "validRoleId";
            _roleManagerMock.Setup(rm => rm.FindByIdAsync(roleId)).ReturnsAsync(new AspNetRoles { Id = roleId });

            // Set up a failed IdentityResult with an error description
            var errorDescription = "Delete failed";
            var failedResult = IdentityResult.Failed(new IdentityError { Description = errorDescription });
            _roleManagerMock.Setup(rm => rm.DeleteAsync(It.IsAny<AspNetRoles>())).ReturnsAsync(failedResult);


            // Act
            var result = await _controller.DeleteRole(roleId) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Something went wrong ! Try again later.", result.Value);
        }

        [Fact]
        public async Task DeleteRole_ExceptionOccures()
        {
            var role = new AspNetRoles { Id = "1", Name = "Name", ApplicationId = "1" };
            _roleManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(role);
            _roleManagerMock.Setup(_ => _.DeleteAsync(It.IsAny<AspNetRoles>())).ThrowsAsync(new Exception("An error occured while deleting role.")).Verifiable();
            var result = await _controller.DeleteRole("1");
            Assert.NotNull(result);
            var jsonresult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Something went wrong ! Try again later.", jsonresult.Value);
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

        //Latest Added
        [Fact]
        public async Task AddRole_RateLimitExceeded_SetsErrorMessageAndRedirectsToIndex()
        {
            // Arrange
            var model = new RolesViewModel
            {
                RoleName = "SomeRoleName"
            };

            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;

            // Set rate limit exceeded in HttpContext.Items
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Items = { { RateLimitForRoleAttribute.RateLimitExceededKey, true } }
                }
            };

            // Act
            var result = await _controller.AddRole(model);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task AddRole_SuccessfulRoleCreation_RedirectsToIndexAndSetsSuccessMessage()
        {
            // Arrange
            var model = new RolesViewModel
            {
                RoleName = "SomeRoleName",
                ApplicationName = "SomeApplicationName"
            };

            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<AspNetRoles>()))
                           .ReturnsAsync(IdentityResult.Success);

            var rateLimitExceededKey = RateLimitForRoleAttribute.RateLimitExceededKey;
            object? rateLimitExceededValue;

            // Set rate limit exceeded in HttpContext.Items
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.Items.TryGetValue(rateLimitExceededKey, out rateLimitExceededValue))
                           .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.AddRole(model);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
    }
}
