using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NuGet.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TAN.DomainModels;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.UnitTest.Helpers;
using TANWeb.Controllers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class FacilityMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly FacilityController _facilityController;
        private Mock<HttpContext> _httpContextMock;
        private readonly Mock<UserManager<AspNetUser>> _userManager;

        public FacilityMock()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManager = GetUserManagerMock();
            _facilityController = new FacilityController(_unitOfWorkMock.Object, _userManager.Object);
            _httpContextMock = new Mock<HttpContext>();
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
        public async Task SaveFacility_InValidModelMock()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "20" }
            };
            var expectedFacilityId = facility.FacilityID;
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.AddFacility(facility)).ReturnsAsync(expectedFacilityId); // Set the return value
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Rollback());
            // Act
            var result = await _facilityController.SaveFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockFacilityId = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedFacilityId, mockFacilityId.ToString());
        }

        [Fact]
        public async Task SaveFacility_InValidModelMockThrowsException()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "20" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.AddFacility(facility)).Throws(new Exception("An error occured while adding new facility.")); // Set the return value
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Rollback());
            // Act
            var result = await _facilityController.SaveFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult.ToString());
        }

        [Fact]
        public async Task SaveFacility_ValidModelMock()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                FacilityID = "2020",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "20" },
                HasDeduction = true,
                RegularDeduction = 30,
                OverTimeDeduction = 60,
            };

            var breakDeduction = new List<BreakDeduction>
            {
                new BreakDeduction
                {
                Id = 1,
                FacilityId = 2000,
                RegularTime = 30,
                OverTime = 60,
                }
            };

            var fakeFacilities = new List<FacilityModel>
            {
                new FacilityModel { Id = 2020, FacilityID = "2020" },
                new FacilityModel { Id = 20001, FacilityID = "2001" },
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.AddFacility(facility)).ReturnsAsync(expectedResult); // Set the return value
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.GetAll()).Returns(fakeFacilities);
            _unitOfWorkMock.Setup(repo => repo.BreakDeductionRepo.GetAll()).Returns(breakDeduction);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _facilityController.SaveFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task SaveFacility_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var facilityViewModel = new FacilityViewModel
            {
                // Set valid property values for the facility object
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "20" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.AddFacility(facilityViewModel)).ReturnsAsync(expectedResult);
            // Set ModelState to be invalid for the test
            _facilityController.ModelState.AddModelError("FacilityID", "Invalid input data");
            // Act
            var result = await _facilityController.SaveFacility(facilityViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task EditFacility_ValidMock()
        {
            // Arrange
            var facilityId = 10;
            var expectedFacilityModel = new FacilityViewModel();
            _unitOfWorkMock.Setup(o => o.FacilityRepo.GetFacilityDetail(facilityId)).ReturnsAsync(expectedFacilityModel);
            // Act
            var result = await _facilityController.EditFacility(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockFacilityViewModel = Assert.IsType<FacilityViewModel>(jsonResult.Value);
            Assert.NotNull(mockFacilityViewModel);
        }

        [Fact]
        public async Task EditFacility_ValidMockThrowsException()
        {
            // Arrange
            var facilityId = 10;
            var expectedFacilityModel = new FacilityViewModel();
            _unitOfWorkMock.Setup(o => o.FacilityRepo.GetFacilityDetail(facilityId)).Throws(new Exception("An error occured while getting facility details."));
            // Act
            var result = await _facilityController.EditFacility(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockFacilityViewModel = Assert.IsType<FacilityViewModel>(jsonResult.Value);
            Assert.NotNull(mockFacilityViewModel);
        }

        [Fact]
        public async Task UpdateFacility_ValidModelMock()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "30" },
                HasDeduction = true,
                RegularDeduction = 30,
                OverTimeDeduction = 60
            };
            var breakDeduction = new List<BreakDeduction>
            {
                new BreakDeduction
                {
                Id = 1,
                FacilityId = 2000,
                RegularTime = 30,
                OverTime = 60,
                }
            };

            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.UpdateFacility(facility)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(repo => repo.BreakDeductionRepo.GetAll()).Returns(breakDeduction);
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Commit());

            // Act
            var result = await _facilityController.UpdateFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateFacility_ValidModelMockExistingFacility()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                Id = 1,
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "30" },
                HasDeduction = true,
                RegularDeduction = 30,
                OverTimeDeduction = 60
            };
            var breakDeduction = new List<BreakDeduction>
            {
                new BreakDeduction
                {
                Id = 1,
                FacilityId = 1,
                RegularTime = 30,
                OverTime = 60,
                }
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.UpdateFacility(facility)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(repo => repo.BreakDeductionRepo.GetAll()).Returns(breakDeduction);
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Commit());

            // Act
            var result = await _facilityController.UpdateFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateFacility_ValidModelMockFailedTransaction()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                Id = 1,
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "30" },
                HasDeduction = true,
                RegularDeduction = 30,
                OverTimeDeduction = 60
            };
            var breakDeduction = new List<BreakDeduction>
            {
                new BreakDeduction
                {
                Id = 1,
                FacilityId = 1,
                RegularTime = 30,
                OverTime = 60,
                }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.UpdateFacility(facility)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(repo => repo.BreakDeductionRepo.GetAll()).Returns(breakDeduction);
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Rollback());

            // Act
            var result = await _facilityController.UpdateFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateFacility_ValidModelMockFailedOnException()
        {
            // Arrange
            var facility = new FacilityViewModel
            {
                // Set valid property values for the facility object
                Id = 1,
                FacilityID = "2000",
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "30" },
                HasDeduction = true,
                RegularDeduction = 30,
                OverTimeDeduction = 60
            };
            var breakDeduction = new List<BreakDeduction>
            {
                new BreakDeduction
                {
                Id = 1,
                FacilityId = 1,
                RegularTime = 30,
                OverTime = 60,
                }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.UpdateFacility(facility)).Throws(new Exception("An error occured while updating existing facility."));
            _unitOfWorkMock.Setup(repo => repo.BreakDeductionRepo.GetAll()).Returns(breakDeduction);
            _unitOfWorkMock.Setup(repo => repo.BeginTransaction().Rollback());

            // Act
            var result = await _facilityController.UpdateFacility(facility);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateFacility_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var facilityViewModel = new FacilityViewModel
            {
                FacilityName = "Test Facility",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                OrganizationId = "2007",
                ApplicationId = new List<string> { "10", "30" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.UpdateFacility(facilityViewModel)).ReturnsAsync(expectedResult);
            // Set ModelState to be invalid for the test
            _facilityController.ModelState.AddModelError("FacilityID", "Invalid input data");
            // Act
            var result = await _facilityController.UpdateFacility(facilityViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task DeleteFacilityDetailMock()
        {
            // Arrange
            int facilityId = 1; // Provide a valid organization ID for the test
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.DeleteFacilityDetail(facilityId)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(uow => uow.BeginTransaction().Commit());
            // Act
            var result = await _facilityController.DeleteFacility(facilityId);


            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteFacilityDetailMockTransactionFailed()
        {
            // Arrange
            int facilityId = 1; // Provide a valid organization ID for the test
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.DeleteFacilityDetail(facilityId)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(uow => uow.BeginTransaction().Rollback());
            // Act
            var result = await _facilityController.DeleteFacility(facilityId);


            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteFacilityDetailMockTransactionFailedOnException()
        {
            // Arrange
            int facilityId = 1; // Provide a valid organization ID for the test
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.FacilityRepo.DeleteFacilityDetail(facilityId)).Throws(new Exception("An error occured while deleting facility details."));
            _unitOfWorkMock.Setup(uow => uow.BeginTransaction().Rollback());
            // Act
            var result = await _facilityController.DeleteFacility(facilityId);


            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public void GetFacilities_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string orgName = "";
            string appName = "";
            string storedValue = "";
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "FacilityName";
            string sortDirection = "asc";
            int draw = 1;

            int totalRecord = 10;
            int filterRecord = 10;
            // Mock the FacilityRepository's GetAllFacilities method
            int facilityCount = 10;
            IEnumerable<FacilityViewModel> data = new List<FacilityViewModel>
        {
           new FacilityViewModel{ Id=10, FacilityID="45564", FacilityName="DemoFacility", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId="1" },
           new FacilityViewModel{ Id=10, FacilityID="159753", FacilityName="DemoFacility1", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId="1" },
        };
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAllFacilities(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), orgName, appName, It.IsAny<string>(), out facilityCount))
                .Returns(data);
            // Act
            var result = _facilityController.GetFacilities(orgName, appName, storedValue, searchValue, start, length, sortColumn, sortDirection, draw, null);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<FacilityViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public async void Index_Returns_View_With_FacilityViewModel()
        {
            // Arrange
            const string selectedApp = "PBJSnap";
            var organizations = new List<OrganizationModel>()
            {
                new OrganizationModel { OrganizationID= 1,OrganizationName="OrganizationOne1",OrganizationEmail="OrganizationOne@demo.com", PrimaryPhone="1234567890", SecondaryPhone="1234567890", Country="USA", Address1="Supa", Address2="WK dummy", City="Parner", State="AR", ZipCode="12345", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
                new OrganizationModel { OrganizationID= 2,OrganizationName="OrganizationTwo",OrganizationEmail="OrganizationTwo@demo.com", PrimaryPhone="0987654321", SecondaryPhone="0987654321", Country="USA", Address1="Supa", Address2="", City="Parner", State="AZ", ZipCode="41430", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
            };

            var applicationOrganizations = new List<Itemlist>()
            {
                new Itemlist { Value= 1,Text="OrganizationOne1"},
                new Itemlist { Value= 2,Text="OrganizationTwo" },
            };
            var states = new List<StateModel>()
            {
                new StateModel{ StateID=1, StateCode="AL", StateName="Alabama", DisplayOrder=1 },
                new StateModel{ StateID=2, StateCode="AK", StateName="Alaska", DisplayOrder=1 },
                new StateModel{ StateID=3, StateCode="AZ", StateName="Arizona", DisplayOrder=3 }
            };
            var applications = new List<Application>()
            {
                new Application{ ApplicationId=1, Name="PBJSnap", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
                new Application{ ApplicationId=2, Name="Inventory", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
                new Application{ ApplicationId=3, Name="Reporting", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
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
            _facilityController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetAll()).Returns(organizations);
            _unitOfWorkMock.Setup(u => u.OrganizationRepo.GetApplicationWiseOrganizations(selectedApp))
                    .ReturnsAsync(applicationOrganizations);
            _unitOfWorkMock.Setup(uow => uow.StateRepo.GetAll()).Returns(states);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo()).Returns(true);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _facilityController.TempData = tempData;
            var mockUser = new AspNetUser { UserType = 1 };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);
            // Act
            var result = await _facilityController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var facilityViewModel = Assert.IsType<FacilityViewModel>(viewResult.Model);
            Assert.NotNull(facilityViewModel);
            // Check if OrganizationList, StateList, and ApplicationList are correctly populated
            Assert.Null(facilityViewModel.OrganizationList);
            Assert.Equal(states.Count, facilityViewModel.StateList.Count);
            Assert.Equal(applications.Count, facilityViewModel.ApplicationList.Count);
            Assert.Equal(1, mockUser.UserType);
        }

        [Fact]
        public void GetValues_Redirects_To_Index_With_TempData()
        {
            // Arrange
            string buttonId = "Button1";
            string isFromOrganization = "true";
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _facilityController.TempData = tempData;
            // Act
            var result = _facilityController.GetValues(buttonId, isFromOrganization);
            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            // Check if TempData is correctly set
            Assert.Equal(isFromOrganization, _facilityController.TempData["isFromOrganization"]);
            Assert.Equal(buttonId, _facilityController.TempData["buttonId"]);
        }

        [Fact]
        public async Task CheckForValidName_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string facilityName = "TestFacility";
            string facilityId = "1";
            // Mock the FacilityRepository's GetFacilityName method
            bool isNameExists = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetFacilityName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(isNameExists);
            // Act
            var result = await _facilityController.CheckForValidName(facilityName, facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isNameExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.Equal(isNameExists, isNameExistsResult);
        }

        [Fact]
        public async Task CheckForValidName_Returns_False_When_Exception_Occurs()
        {
            // Arrange
            string facilityName = "TestFacility";
            string facilityId = "1";
            // Mock the FacilityRepository's GetFacilityName method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetFacilityName(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _facilityController.CheckForValidName(facilityName, facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isNameExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(isNameExistsResult); // Ensure the returned value is false due to the exception.
        }

        [Fact]
        public async Task CheckForUniqueId_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string facilityId = "TestFacilityId";
            // Mock the FacilityRepository's GetFacilityId method
            bool isIdExists = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetFacilityId(It.IsAny<string>())).ReturnsAsync(isIdExists);
            // Act
            var result = await _facilityController.CheckForUniqueId(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isIdExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.Equal(isIdExists, isIdExistsResult);
        }

        [Fact]
        public async Task CheckForUniqueId_Returns_False_When_Exception_Occurs()
        {
            // Arrange
            string facilityId = "TestFacilityId";
            // Mock the FacilityRepository's GetFacilityId method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetFacilityId(It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _facilityController.CheckForUniqueId(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isIdExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(isIdExistsResult); // Ensure the returned value is false due to the exception.
        }

        [Fact]
        public void CheckUserRole_ReturnsTrue_WhenUserIsInSuperAdminRole()
        {
            // Arrange
            var expectedResult = true;
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.CheckRole()).Returns(expectedResult);
            // Act
            var result = _facilityController.CheckUserRole();
            // Assert
            Assert.IsType<JsonResult>(result);
            var mockResult = (JsonResult)result;
            Assert.True((bool)mockResult.Value);
        }

        [Fact]
        public void CheckUserRole_ReturnsFalse_WhenUserIsNotInSuperAdminRole()
        {
            // Arrange
            var expectedResult = false;
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.CheckRole()).Returns(expectedResult);
            // Act
            var result = _facilityController.CheckUserRole();
            // Assert
            Assert.IsType<JsonResult>(result);
            var mockResult = (JsonResult)result;
            Assert.False((bool)mockResult.Value);
        }

        [Fact]
        public void CheckUserRole_ReturnsError_WhenExceptionOccurs()
        {
            // Arrange
            var expectedResult = false;
            _unitOfWorkMock.Setup(repo => repo.FacilityRepo.CheckRole()).Throws(new Exception("An error occurred while checking the user role."));
            // Act
            var result = _facilityController.CheckUserRole();
            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            Assert.False((bool)jsonResult.Value);
        }

        [Fact]
        public void GetOrganizationServiceDetails_ReturnsJsonResult_WhenServiceDetailsExist()
        {
            // Arrange
            int organizationId = 132;
            var expectedOrgServiceDetails = new ServiceOrganizationViewModel
            {
                OrganizationId = 132,
                OrganizationServiceType = 0
            };

            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationServiceDetails(organizationId))
                .Returns(expectedOrgServiceDetails);

            // Act
            var result = _facilityController.GetOrganizationServiceDetails(organizationId);

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var mockOrgServiceDetails = Assert.IsType<ServiceOrganizationViewModel>(jsonResult.Value);
            Assert.Equal(expectedOrgServiceDetails.OrganizationId, mockOrgServiceDetails.OrganizationId);
            Assert.Equal(expectedOrgServiceDetails.OrganizationServiceType, mockOrgServiceDetails.OrganizationServiceType);
        }

        [Fact]
        public void GetOrganizationServiceDetails_ReturnsJsonResult_WhenServiceDetailsDoNotExist()
        {
            // Arrange
            int organizationId = 132; // Provide a valid organizationId
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationServiceDetails(organizationId))
                .Returns(new ServiceOrganizationViewModel());

            // Act
            var result = _facilityController.GetOrganizationServiceDetails(organizationId);

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var mockOrgServiceDetails = Assert.IsType<ServiceOrganizationViewModel>(jsonResult.Value);
            Assert.Equal(mockOrgServiceDetails.OrganizationId, 0);
            Assert.Equal(mockOrgServiceDetails.OrganizationServiceType, 0);
        }

        [Fact]
        public void GetOrganizationServiceDetails_ReturnsJsonResult_OnException()
        {
            // Arrange
            int organizationId = 132; // Provide a valid organizationId
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationServiceDetails(organizationId))
                .Throws(new Exception("An error occured while getting organization service details for organization Id"));

            // Act
            var result = _facilityController.GetOrganizationServiceDetails(organizationId);

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public void GetOrganizationUserInfo_ReturnsJsonResult_WhenUserInfoExists()
        {
            // Arrange
            var expectedOrganizationUserInfo = "132";

            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails())
                .Returns(expectedOrganizationUserInfo);

            // Act
            var result = _facilityController.GetOrganizationUserInfo();

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
            var result = _facilityController.GetOrganizationUserInfo();

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
            var result = _facilityController.GetOrganizationUserInfo();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public async void Index_Returns_View_With_FacilityViewModelForOrganizationUser()
        {
            // Arrange

            var organizationId = "132";
            const string selectedApp = "PBJSnap";
            var organizations = new List<OrganizationModel>()
            {
                new OrganizationModel { OrganizationID= 1,OrganizationName="OrganizationOne1",OrganizationEmail="OrganizationOne@demo.com", PrimaryPhone="1234567890", SecondaryPhone="1234567890", Country="USA", Address1="Supa", Address2="WK dummy", City="Parner", State="AR", ZipCode="12345", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
                new OrganizationModel { OrganizationID= 2,OrganizationName="OrganizationTwo",OrganizationEmail="OrganizationTwo@demo.com", PrimaryPhone="0987654321", SecondaryPhone="0987654321", Country="USA", Address1="Supa", Address2="", City="Parner", State="AZ", ZipCode="41430", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
            };

            var organizationUserOrganizations = new List<OrganizationModel>()
            {
                new OrganizationModel { OrganizationID= 132,OrganizationName="TelecomVoice",OrganizationEmail="TelecomVoice@demo.com", PrimaryPhone="1234567890", SecondaryPhone="1234567890", Country="USA", Address1="Supa", Address2="", City="Parner", State="AR", ZipCode="41424", CreatedDate=DateTime.UtcNow, CreatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435",UpdatedDate=DateTime.UtcNow,UpdatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435", IsActive=true },
            };

            var applicationOrganizations = new List<Itemlist>
        {
            new Itemlist { Value = 1, Text = "OrganizationOne1"},
            new Itemlist { Value = 2, Text = "OrganizationTwo" }
        };
            var states = new List<StateModel>()
            {
                new StateModel{ StateID=1, StateCode="AL", StateName="Alabama", DisplayOrder=1 },
                new StateModel{ StateID=2, StateCode="AK", StateName="Alaska", DisplayOrder=1 },
                new StateModel{ StateID=3, StateCode="AZ", StateName="Arizona", DisplayOrder=3 }
            };
            var applications = new List<Application>()
            {
                new Application{ ApplicationId=1, Name="PBJSnap", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
                new Application{ ApplicationId=2, Name="Inventory", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
                new Application{ ApplicationId=3, Name="Reporting", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow, CreatedBy="", UpdatedBy="", IsActive=true },
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
            _facilityController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetAll()).Returns(organizationUserOrganizations);
            _unitOfWorkMock.Setup(uow => uow.StateRepo.GetAll()).Returns(states);
            _unitOfWorkMock.Setup(uow => uow.ApplicationRepo.GetAll()).Returns(applications);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo()).Returns(false);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails()).Returns(organizationId);
            _unitOfWorkMock.Setup(u => u.OrganizationRepo.GetApplicationWiseOrganizations(selectedApp))
                    .ReturnsAsync(applicationOrganizations);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _facilityController.TempData = tempData;
            var mockUser = new AspNetUser { UserType = 1 };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);
            // Act
            var result = await _facilityController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var facilityViewModel = Assert.IsType<FacilityViewModel>(viewResult.Model);
            Assert.NotNull(facilityViewModel);
            // Check if OrganizationList, StateList, and ApplicationList are correctly populated
            Assert.Equal(organizationUserOrganizations.Count, facilityViewModel.OrganizationList.Count);
            Assert.Equal(states.Count, facilityViewModel.StateList.Count);
            Assert.Equal(applications.Count, facilityViewModel.ApplicationList.Count);
            Assert.Equal(1, mockUser.UserType);
        }
    }
}
