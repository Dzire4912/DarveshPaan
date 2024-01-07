using AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Client;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Controllers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class OrganizationMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrganizationController _organizationController;
        public OrganizationMock()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _organizationController = new OrganizationController(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task SaveOrganization_ValidModelMock()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization to match a valid model
                OrganizationID = 2007,
                OrganizationName = "Test Organization",
                OrganizationEmail = "test@example.com",
                PrimaryPhone = "1234567890",
                SecondaryPhone = "9876543210",
                Country = "Test Country",
                Address1 = "Test Address 1",
                Address2 = "Test Address 2",
                State = "Test State",
                City = "Test City",
                ZipCode = "12345"
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.AddOrganization(organization)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _organizationController.SaveOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task SaveOrganization_ValidModelMockTransactionFailed()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization to match a valid model
                OrganizationID = 2007,
                OrganizationName = "Test Organization",
                OrganizationEmail = "test@example.com",
                PrimaryPhone = "1234567890",
                SecondaryPhone = "9876543210",
                Country = "Test Country",
                Address1 = "Test Address 1",
                Address2 = "Test Address 2",
                State = "Test State",
                City = "Test City",
                ZipCode = "12345"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.AddOrganization(organization)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.SaveOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task SaveOrganization_ValidModelMockTransactionFailedOnExceptionOccures()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization to match a valid model
                OrganizationID = 2007,
                OrganizationName = "Test Organization",
                OrganizationEmail = "test@example.com",
                PrimaryPhone = "1234567890",
                SecondaryPhone = "9876543210",
                Country = "Test Country",
                Address1 = "Test Address 1",
                Address2 = "Test Address 2",
                State = "Test State",
                City = "Test City",
                ZipCode = "12345"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.AddOrganization(organization)).Throws(new Exception("An error occured while adding new organization details."));
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.SaveOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task EditOrganization_ValidMock()
        {
            // Arrange
            var organizationId = 2006;
            var expectedOrganizationViewModel = new OrganizationViewModel();
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.GetOrganizationDetail(organizationId)).ReturnsAsync(expectedOrganizationViewModel);
            // Act
            var result = await _organizationController.EditOrganization(organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockOrganizationViewModel = Assert.IsType<OrganizationViewModel>(jsonResult.Value);
            Assert.NotNull(mockOrganizationViewModel);
        }

        [Fact]
        public async Task EditOrganization_InvalidMock()
        {
            // Arrange
            int organizationId = 1;
            // Set up mock repository to return a specific organization view model
            _unitOfWorkMock.Setup(repo => repo.OrganizationRepo.GetOrganizationDetail(organizationId)).Throws(new Exception("An error occured while getting organization details."));
            // Act
            var result = await _organizationController.EditOrganization(organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockOrganizationViewModel = Assert.IsType<OrganizationViewModel>(jsonResult.Value);
            Assert.Null(mockOrganizationViewModel.OrganizationName);
        }

        [Fact]
        public async Task UpdateOrganization_ValidModelMock()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization view model to match a valid model
                OrganizationID = 1,
                OrganizationName = "Updated Organization",
                OrganizationEmail = "updated@example.com",
                PrimaryPhone = "9876543210",
                SecondaryPhone = "1234567890",
                Country = "Updated Country",
                Address1 = "Updated Address 1",
                Address2 = "Updated Address 2",
                State = "Updated State",
                City = "Updated City",
                ZipCode = "54321"
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.UpdateOrganization(organization)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _organizationController.UpdateOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateOrganization_ValidModelMockTransactionFailed()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization view model to match a valid model
                OrganizationID = 1,
                OrganizationName = "Updated Organization",
                OrganizationEmail = "updated@example.com",
                PrimaryPhone = "9876543210",
                SecondaryPhone = "1234567890",
                Country = "Updated Country",
                Address1 = "Updated Address 1",
                Address2 = "Updated Address 2",
                State = "Updated State",
                City = "Updated City",
                ZipCode = "54321"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.UpdateOrganization(organization)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.UpdateOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateOrganization_ValidModelMockTransactionFailedOnException()
        {
            // Arrange
            var organization = new OrganizationViewModel
            {
                // Set the properties of the organization view model to match a valid model
                OrganizationID = 1,
                OrganizationName = "Updated Organization",
                OrganizationEmail = "updated@example.com",
                PrimaryPhone = "9876543210",
                SecondaryPhone = "1234567890",
                Country = "Updated Country",
                Address1 = "Updated Address 1",
                Address2 = "Updated Address 2",
                State = "Updated State",
                City = "Updated City",
                ZipCode = "54321"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.UpdateOrganization(organization)).Throws(new Exception("An error occured while updating organization details."));
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.UpdateOrganization(organization);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteOrganizationDetailMock_Returns_Success()
        {
            // Arrange
            int organizationId = 1; // Provide a valid organization ID for the test
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.DeleteOrganizationDetail(organizationId)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _organizationController.DeleteOrganization(organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteOrganizationDetailMock_Returns_TransactionFailed()
        {
            // Arrange
            int organizationId = 1; // Provide a valid organization ID for the test
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.DeleteOrganizationDetail(organizationId)).ReturnsAsync(expectedResult);
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.DeleteOrganization(organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteOrganizationDetailMock_Returns_TransactionFailedOnException()
        {
            // Arrange
            int organizationId = 1; // Provide a valid organization ID for the test
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.DeleteOrganizationDetail(organizationId)).Throws(new Exception("An error occured while deleting organization details."));
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _organizationController.DeleteOrganization(organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public void GetOrganizationsList_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "OrganizationName";
            string sortDirection = "asc";
            int draw = 1;
            int totalRecord = 10;
            int filterRecord = 10;
            // Mock the OrganizationRepository's GetAllOrganizations method
            int organizationCount = 10;
            IEnumerable<OrganizationViewModel> data = new List<OrganizationViewModel>
            {
                new OrganizationViewModel { OrganizationID= 1,OrganizationName="OrganizationOne1",OrganizationEmail="OrganizationOne@demo.com", PrimaryPhone="1234567890", SecondaryPhone="1234567890", Country="USA", Address1="Supa", Address2="WK dummy", City="Parner", State="AR", ZipCode="12345", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
                new OrganizationViewModel { OrganizationID= 2,OrganizationName="OrganizationTwo",OrganizationEmail="OrganizationTwo@demo.com", PrimaryPhone="0987654321", SecondaryPhone="0987654321", Country="USA", Address1="Supa", Address2="", City="Parner", State="AZ", ZipCode="41430", CreatedDate=DateTime.UtcNow, CreatedBy="f8dd5b1a - 90b4 - 463b - 850c - 5f8ec60f6b14",UpdatedDate=DateTime.UtcNow,UpdatedBy="c500c78c - 32a1 - 451b - 8c5b - a62c60520110", IsActive=true },
            };
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetAllOrganizations(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out organizationCount))
                .Returns(data);
            // Act
            var result = _organizationController.GetOrganizationsList(searchValue, start, length, sortColumn, sortDirection, draw, null);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<OrganizationViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public void Index_Returns_ViewResult_With_OrganizationViewModel()
        {
            // Arrange
            var states = new List<StateModel>()
            {
                new StateModel{ StateID=1, StateCode="AL", StateName="Alabama", DisplayOrder=1 },
                new StateModel{ StateID=2, StateCode="AK", StateName="Alaska", DisplayOrder=1 },
                new StateModel{ StateID=3, StateCode="AZ", StateName="Arizona", DisplayOrder=3 }
            };
            // Mock the StateRepository's GetAll method
            _unitOfWorkMock.Setup(uow => uow.StateRepo.GetAll()).Returns(states);
            // Act
            var result = _organizationController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganizationViewModel>(viewResult.Model);
            // Verify that the StateList in the model contains the same number of items as the mocked stateList
            Assert.Equal(states.Count, model.StateList.Count);
        }

        [Fact]
        public async Task SaveOrganization_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var organizationViewModel = new OrganizationViewModel
            {
                // Set the properties of the organization view model to match a valid model
                OrganizationName = "Updated Organization",
                OrganizationEmail = "updated@example.com",
                PrimaryPhone = "9876543210",
                SecondaryPhone = "1234567890",
                Country = "Updated Country",
                Address1 = "Updated Address 1",
                Address2 = "Updated Address 2",
                State = "Updated State",
                City = "Updated City",
                ZipCode = "54321"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.AddOrganization(organizationViewModel)).ReturnsAsync(expectedResult);
            // Set ModelState to be invalid for the test
            _organizationController.ModelState.AddModelError("OrganizationID", "Invalid input data");
            // Act
            var result = await _organizationController.SaveOrganization(organizationViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task UpdateOrganization_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var organizationViewModel = new OrganizationViewModel
            {
                // Set the properties of the organization view model to match a valid model
                OrganizationName = "Updated Organization",
                OrganizationEmail = "updated@example.com",
                PrimaryPhone = "9876543210",
                SecondaryPhone = "1234567890",
                Country = "Updated Country",
                Address1 = "Updated Address 1",
                Address2 = "Updated Address 2",
                State = "Updated State",
                City = "Updated City",
                ZipCode = "54321"
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.OrganizationRepo.UpdateOrganization(organizationViewModel)).ReturnsAsync(expectedResult);
            // Set ModelState to be invalid for the test
            _organizationController.ModelState.AddModelError("OrganizationID", "Invalid input data");
            // Act
            var result = await _organizationController.UpdateOrganization(organizationViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task CheckForValidEmail_Returns_JsonResult_True_When_Email_Is_Valid()
        {
            // Arrange
            string emailId = "test@example.com";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetEmailId method
            bool expectedResult = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetEmailId(emailId, organizationId)).ReturnsAsync(expectedResult);
            // Act
            var result = await _organizationController.CheckForValidEmail(emailId, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.True(resultValue);
        }

        [Fact]
        public async Task CheckForValidEmail_Returns_JsonResult_False_When_Email_Is_Invalid()
        {
            // Arrange
            string emailId = "test@example.com";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetEmailId method
            bool expectedResult = false; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetEmailId(emailId, organizationId)).ReturnsAsync(expectedResult);
            // Act
            var result = await _organizationController.CheckForValidEmail(emailId, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(resultValue);
        }

        [Fact]
        public async Task CheckForValidEmail_Returns_JsonResult_False_When_Exception_Occurs()
        {
            // Arrange
            string emailId = "test@example.com";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetEmailId method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetEmailId(emailId, organizationId)).ThrowsAsync(new Exception("Invalid Email Id"));
            // Act
            var result = await _organizationController.CheckForValidEmail(emailId, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(resultValue);
        }

        [Fact]
        public async Task CheckForValidName_Returns_JsonResult_True_When_Name_Is_Valid()
        {
            // Arrange
            string organizationName = "TestOrg";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetOrganizationName method
            bool expectedResult = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetOrganizationName(organizationName, organizationId)).ReturnsAsync(expectedResult);
            // Act
            var result = await _organizationController.CheckForValidName(organizationName, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.True(resultValue);
        }

        [Fact]
        public async Task CheckForValidName_Returns_JsonResult_False_When_Name_Is_Invalid()
        {
            // Arrange
            string organizationName = "12345";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetOrganizationName method
            bool expectedResult = false; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetOrganizationName(organizationName, organizationId)).ReturnsAsync(expectedResult);
            // Act
            var result = await _organizationController.CheckForValidName(organizationName, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(resultValue);
        }

        [Fact]
        public async Task CheckForValidName_Returns_JsonResult_False_When_Exception_Occurs()
        {
            // Arrange
            string organizationName = "@123";
            int organizationId = 1;
            // Mock the OrganizationRepository's GetOrganizationName method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetOrganizationName(organizationName, organizationId)).ThrowsAsync(new Exception("Invalid Organization Name"));
            // Act
            var result = await _organizationController.CheckForValidName(organizationName, organizationId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(resultValue);
        }

        [Fact]
        public void GetMasterAccountDetails_ReturnsJsonResult_WhenDataExists()
        {
            // Arrange
            BandWidthMasterViewModel bandWidthMasterViewModel = new BandWidthMasterViewModel
            {
                AccountId = "5009107",
                AccountName = "ThinkAnew"
            };
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetMasterAccountDetails())
                .Returns(bandWidthMasterViewModel);

            // Act
            var result = _organizationController.GetMasterAccountDetails();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockBandWidthMasterViewModel = Assert.IsType<BandWidthMasterViewModel>(jsonResult.Value);
            Assert.NotNull(jsonResult.Value);
            Assert.Equal(mockBandWidthMasterViewModel.AccountId, bandWidthMasterViewModel.AccountId);
            Assert.Equal(mockBandWidthMasterViewModel.AccountName, bandWidthMasterViewModel.AccountName);
        }

        [Fact]
        public void GetMasterAccountDetails_ReturnsJsonResult_WhenNoData()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetMasterAccountDetails())
                .Returns((new BandWidthMasterViewModel()));

            // Act
            var result = _organizationController.GetMasterAccountDetails();

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockBandWidthMasterViewModel = Assert.IsType<BandWidthMasterViewModel>(jsonResult.Value);
            Assert.NotNull(jsonResult.Value);
            Assert.Null(mockBandWidthMasterViewModel.AccountId);
            Assert.Null(mockBandWidthMasterViewModel.AccountName);
        }

        [Fact]
        public void GetMasterAccountDetails_ReturnsJsonResult_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.GetMasterAccountDetails())
                .Throws(new Exception("An exception occured while getting master account details"));
            // Act
            var result = _organizationController.GetMasterAccountDetails();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public async Task CheckForSubAccountId_ReturnsJsonResult_WhenSubAccountIdExists()
        {
            // Arrange
            var organizationId = 132;
            var subAccountId = "98953";
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.CheckSubAccountId(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _organizationController.CheckForSubAccountId(organizationId,subAccountId);

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public async Task CheckForSubAccountId_ThrowsException()
        {
            // Arrange
            var organizationId = 132;
            var subAccountId = "98953";
            _unitOfWorkMock.Setup(uow => uow.OrganizationRepo.CheckSubAccountId(It.IsAny<int>(), It.IsAny<string>()))
                .Throws(new Exception("An error occured while checking subaccount for organization"));

            // Act
            var result = await _organizationController.CheckForSubAccountId(organizationId, subAccountId);

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False((bool)jsonResult.Value);
        }
    }
}
