using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Controllers;
using static TAN.DomainModels.Helpers.Permissions;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class AgencyMock
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AgencyController _agencyController;
        public AgencyMock()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _agencyController = new AgencyController(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task SaveAgency_ValidModelMock()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "20" }
            };
            var expectedAgencyId = agency.AgencyId;
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.AddAgency(agency)).ReturnsAsync(expectedAgencyId); // Set the return value
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _agencyController.AddAgency(agency);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockAgencyId = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedAgencyId, mockAgencyId.ToString());
        }

        [Fact]
        public async Task SaveAgency_ValidModelMockThrowsException()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "20" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.AddAgency(agency)).Throws(new Exception("An error occured while adding new agency"));
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _agencyController.AddAgency(agency);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockAgencyResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockAgencyResult);
        }

        [Fact]
        public async Task SaveAgency_InValidModelMock()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "20" }
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.AddAgency(agency)).ReturnsAsync(expectedResult); // Set the return value
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _agencyController.AddAgency(agency);
            _agencyController.ModelState.AddModelError("PropertyName", "Error message");
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }
        [Fact]
        public async Task EditAgency_ValidMock()
        {
            // Arrange
            var agencyId = "10";
            var expectedAgencyViewModel = new AgencyViewModel();
            _unitOfWorkMock.Setup(o => o.AgencyRepo.GetAgencyDetails(agencyId)).ReturnsAsync(expectedAgencyViewModel);
            // Act
            var result = await _agencyController.GetAgencyDetail(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockAgencyViewModel = Assert.IsType<AgencyViewModel>(jsonResult.Value);
            Assert.NotNull(mockAgencyViewModel);
        }

        [Fact]
        public async Task EditAgency_ValidMockThrowsException()
        {
            // Arrange
            var agencyId = "10";
            _unitOfWorkMock.Setup(o => o.AgencyRepo.GetAgencyDetails(agencyId)).Throws(new Exception("An error occured while getting agency details"));
            // Act
            var result = await _agencyController.GetAgencyDetail(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockAgencyViewModel = Assert.IsType<AgencyViewModel>(jsonResult.Value);
            Assert.NotNull(mockAgencyViewModel);
        }

        [Fact]
        public async Task UpdateAgency_ValidModelMock()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "30" }
            };
            var expectedResult = "success";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.UpdateAgency(agency)).ReturnsAsync(expectedResult); // Set the return value
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Commit());
            // Act
            var result = await _agencyController.UpdateAgency(agency);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateAgency_ValidModelMockFailed()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "30" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.UpdateAgency(agency)).ReturnsAsync(expectedResult); // Set the return value
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _agencyController.UpdateAgency(agency);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task UpdateAgency_ValidModelMockExceptionThrows()
        {
            // Arrange
            var agency = new AgencyViewModel
            {
                // Set valid property values for the facility object
                AgencyId = "1500",
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "30" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.UpdateAgency(agency)).Throws(new Exception("An error occured while updating agency details"));
            _unitOfWorkMock.Setup(x => x.BeginTransaction().Rollback());
            // Act
            var result = await _agencyController.UpdateAgency(agency);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteAgencyDetailMock()
        {
            // Arrange
            int agencyId = 1; // Provide a valid organization ID for the test
            var expectedResult = "success";
            _unitOfWorkMock.Setup(o => o.AgencyRepo.DeleteAgencyDetail(agencyId)).ReturnsAsync(expectedResult);
            // Act
            var result = await _agencyController.DeleteAgency(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public async Task DeleteAgencyDetailMockThorowsException()
        {
            // Arrange
            int agencyId = 1; // Provide a valid organization ID for the test
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(o => o.AgencyRepo.DeleteAgencyDetail(agencyId)).Throws(new Exception("An error occured while deleting agency details"));
            // Act
            var result = await _agencyController.DeleteAgency(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var mockResult = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, mockResult);
        }

        [Fact]
        public void GetAllAgencies_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string facilityName = "";
            string searchValue = "";
            string start = "0";
            string length = "10";
            string sortColumn = "AgencyName";
            string sortDirection = "asc";
            int draw = 1;
            int totalRecord = 10;
            int filterRecord = 10;
            // Mock the AgencyRepository's GetAllAgenciesList method
            int agencyCount = 10;
            IEnumerable<AgencyViewModel> data = new List<AgencyViewModel>
        {
           new AgencyViewModel{ Id=1, AgencyId="12345", AgencyName ="DemoAgency", Address1 ="Supa", Address2="", City="Parner", State="AK", ZipCode="41424", IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdateDate=DateTime.UtcNow, FacilityId=new List<string>{"10" } },
           new AgencyViewModel{ Id=2, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode="41424", IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdateDate=DateTime.UtcNow, FacilityId=new List<string>{"11" } }
        };
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAllAgenciesList(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), facilityName, "","", out agencyCount))
                .Returns(data);
            // Act
            var result = _agencyController.GetAllAgencies(facilityName, "", searchValue, start, length, sortColumn, sortDirection, draw,"");
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<AgencyViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(data.Count(), dataObj.Count());
        }

        [Fact]
        public async Task Index_Returns_View_With_AgencyViewModel()
        {
            // Arrange
            var states = new List<StateModel>()
            {
                new StateModel{ StateID=1, StateCode="AL", StateName="Alabama", DisplayOrder=1 },
                new StateModel{ StateID=2, StateCode="AK", StateName="Alaska", DisplayOrder=1 },
                new StateModel{ StateID=3, StateCode="AZ", StateName="Arizona", DisplayOrder=3 }
            };
            var facilities = new List<FacilityModel>
            {
                new FacilityModel{ Id=10, FacilityID="45564", FacilityName="DemoFacility", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId=1 },
                new FacilityModel{ Id=10, FacilityID="159753", FacilityName="DemoFacility1", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId=1},
            };
            var agencies = new List<AgencyModel>
        {
           new AgencyModel{ Id=1, AgencyId="12345", AgencyName ="DemoAgency", Address1 ="Supa", Address2="", City="Parner", State="AK", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=10 },
           new AgencyModel{ Id=2, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=11 }
        };
            _unitOfWorkMock.Setup(uow => uow.StateRepo.GetAll()).Returns(states);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAllFacilitiesByOrgId()).Returns(Task.FromResult(facilities));
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAll()).Returns(agencies);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo()).Returns(true);
            // Act
            var result = await _agencyController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var agencyViewModel = Assert.IsType<AgencyViewModel>(viewResult.Model);
            Assert.NotNull(agencyViewModel);
            // Check if StateList and FacilityList are correctly populated
            Assert.Equal(states.Count, agencyViewModel.StateList.Count);
            Assert.Equal(facilities.Count, agencyViewModel.FacilityList.Count);
            Assert.Equal(agencies.Count, agencyViewModel.AgencyIdList.Count);
        }

        [Fact]
        public async Task GetAgenciesByFacility_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            int facilityId = 1;
            // Mock the AgencyRepository's GetAllAgeniciesByFacilityId method
            List<AgencyModel> agencies = new List<AgencyModel>
        {
           new AgencyModel{ Id=1, AgencyId="12345", AgencyName ="DemoAgency", Address1 ="Supa", Address2="", City="Parner", State="AK", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=10},
           new AgencyModel{ Id=2, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=11 }
        };
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAllAgeniciesByFacilityId(It.IsAny<int>())).ReturnsAsync(agencies);
            // Act
            var result = await _agencyController.GetAgenciesByFacility(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var agenciesObj = Assert.IsType<List<AgencyModel>>(jsonResult.Value);
            Assert.Equal(agencies.Count, agenciesObj.Count);
        }

        [Fact]
        public async Task GetAgenciesByFacility_Returns_EmptyList_When_Exception_Occurs()
        {
            // Arrange
            int facilityId = 1;
            // Mock the AgencyRepository's GetAllAgeniciesByFacilityId method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAllAgeniciesByFacilityId(It.IsAny<int>())).ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _agencyController.GetAgenciesByFacility(facilityId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var agenciesObj = Assert.IsType<List<AgencyModel>>(jsonResult.Value);
            Assert.Empty(agenciesObj); // Ensure the returned list is empty due to the exception.
        }

        [Fact]
        public async Task CheckForValidName_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string agencyName = "TestAgency";
            string agencyId = "1";
            // Mock the AgencyRepository's GetAgencyName method
            bool isNameExists = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAgencyName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(isNameExists);
            // Act
            var result = await _agencyController.CheckForValidName(agencyName, agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isNameExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.Equal(isNameExists, isNameExistsResult);
        }

        [Fact]
        public async Task CheckForValidName_Returns_False_When_Exception_Occurs()
        {
            // Arrange
            string agencyName = "TestAgency";
            string agencyId = "1";
            // Mock the AgencyRepository's GetAgencyName method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAgencyName(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _agencyController.CheckForValidName(agencyName, agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isNameExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(isNameExistsResult); // Ensure the returned value is false due to the exception.
        }

        [Fact]
        public async Task CheckForUniqueId_Returns_JsonResult_With_Correct_Data()
        {
            // Arrange
            string agencyId = "TestId";
            // Mock the AgencyRepository's GetAgencyId method
            bool isIdExists = true; // Set the desired return value for the test
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAgencyId(It.IsAny<string>())).ReturnsAsync(isIdExists);
            // Act
            var result = await _agencyController.CheckForUniqueId(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isIdExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.Equal(isIdExists, isIdExistsResult);
        }

        [Fact]
        public async Task CheckForUniqueId_Returns_False_When_Exception_Occurs()
        {
            // Arrange
            string agencyId = "TestId";
            // Mock the AgencyRepository's GetAgencyId method to throw an exception
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAgencyId(It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));
            // Act
            var result = await _agencyController.CheckForUniqueId(agencyId);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var isIdExistsResult = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(isIdExistsResult); // Ensure the returned value is false due to the exception.
        }

        [Fact]
        public async Task AddAgency_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var agencyViewModel = new AgencyViewModel
            {
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "30" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.AddAgency(agencyViewModel)).ReturnsAsync(expectedResult); // Set the return value
            // Set ModelState to be invalid for the test
            _agencyController.ModelState.AddModelError("AgencyId", "Invalid input data");
            // Act
            var result = await _agencyController.AddAgency(agencyViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task UpdateAgency_Returns_Failed_JsonResult_When_ModelState_Is_Invalid()
        {
            // Arrange
            var agencyViewModel = new AgencyViewModel
            {
                AgencyName = "Test Agency",
                Address1 = "Address1Update",
                Address2 = "Address2Update",
                City = "CityUpdate",
                State = "AK",
                ZipCode = "414301",
                IsActive = true,
                FacilityId = new List<string> { "10", "30" }
            };
            var expectedResult = "failed";
            _unitOfWorkMock.Setup(repo => repo.AgencyRepo.UpdateAgency(agencyViewModel)).ReturnsAsync(expectedResult); // Set the return value
            // Set ModelState to be invalid for the test
            _agencyController.ModelState.AddModelError("AgencyId", "Invalid input data");
            // Act
            var result = await _agencyController.UpdateAgency(agencyViewModel);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<string>(jsonResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public void GetAgencyIdListByFacilityName_ReturnsJsonResult()
        {
            // Arrange
            var facilityName = "RahulFacility"; // Replace with the desired facility name

            var agencies = new List<AgencyModel>
        {
            new AgencyModel { FacilityId = 13, AgencyId = "finalAgency", IsActive = true },
            new AgencyModel { FacilityId = 13, AgencyId = "RAgency", IsActive = true },
        };


            // Set up mock behavior for FacilityRepo and AgencyRepo
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll())
                .Returns(new List<FacilityModel> { new FacilityModel { Id = 13, FacilityName = facilityName } });

            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAll())
                .Returns(agencies);


            // Act
            var result = _agencyController.GetAgencyIdListByFacilityName(facilityName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Itemlist>>(jsonResult.Value);

            // Perform additional assertions on the model as needed
            Assert.Equal(agencies.Count, model.Count());
        }

        [Fact]
        public void GetAgencyIdListByFacilityName_WhenFacilityDetailsIsNull_ReturnsJsonResultWithAllActiveAgencies()
        {
            // Arrange
            var facilityName = "NonExistentFacility"; // Replace with the desired facility name

            var agencies = new List<AgencyModel>
        {
            new AgencyModel { FacilityId = 13, AgencyId = "finalAgency", IsActive = true },
            new AgencyModel { FacilityId = 13, AgencyId = "RAgency", IsActive = true },
        };

            // Set up mock behavior for FacilityRepo and AgencyRepo
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll())
            .Returns(new List<FacilityModel>());

            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAll())
                .Returns(agencies);
            // Act
            var result = _agencyController.GetAgencyIdListByFacilityName(facilityName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Itemlist>>(jsonResult.Value);

            // Perform additional assertions on the model as needed
            Assert.Equal(agencies.Count, model.Count());
        }

        [Fact]
        public void GetAgencyIdListByFacilityName_WhenExceptionOccurs_ReturnsJsonResultWithEmptyAgencyModel()
        {
            // Arrange
            var facilityName = "RahulFacility"; // Replace with a valid facility name
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll())
                .Throws(new Exception("An Error occured while getting facilities information")); // Simulate an exception
            // Act
            var result = _agencyController.GetAgencyIdListByFacilityName(facilityName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsType<AgencyModel>(jsonResult.Value);
            Assert.Null(model.AgencyId); 
        }

        [Fact]
        public void GetOrganizationUserInfo_ReturnsJsonResult_WhenUserInfoExists()
        {
            // Arrange
            var expectedOrganizationUserInfo = "132";

            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails())
                .Returns(expectedOrganizationUserInfo);

            // Act
            var result = _agencyController.GetOrganizationUserInfo();

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
            var result = _agencyController.GetOrganizationUserInfo();

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
            var result = _agencyController.GetOrganizationUserInfo();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_WhenUserInfoExists()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Returns(true);

            // Act
            var result = _agencyController.GetLoggedInUserInfo();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.True(mockedUserInfo);
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_WhenUserInfoDoNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Returns(false);

            // Act
            var result = _agencyController.GetLoggedInUserInfo();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(mockedUserInfo);
        }

        [Fact]
        public void GetLoggedInUserInfo_ReturnsJsonResult_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo())
                .Throws(new Exception("An error occured while getting logged in user information"));

            // Act
            var result = _agencyController.GetLoggedInUserInfo();

            // Assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var mockedUserInfo = Assert.IsType<bool>(jsonResult.Value);
            Assert.False(mockedUserInfo);
        }

        [Fact]
        public async Task Index_Returns_View_With_AgencyViewModelForOrganizationUser()
        {
            // Arrange
            var organizationId = "132";
            var states = new List<StateModel>()
            {
                new StateModel{ StateID=1, StateCode="AL", StateName="Alabama", DisplayOrder=1 },
                new StateModel{ StateID=2, StateCode="AK", StateName="Alaska", DisplayOrder=1 },
                new StateModel{ StateID=3, StateCode="AZ", StateName="Arizona", DisplayOrder=3 }
            };
            var facilities = new List<FacilityModel>
            {
                new FacilityModel{ Id=47, FacilityID="TelecomVoiceFaci", FacilityName="TelecomVoiceFaci", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId=132},
            };
            var organizationUserOrganizations = new List<OrganizationModel>()
            {
                new OrganizationModel { OrganizationID= 132,OrganizationName="TelecomVoice",OrganizationEmail="TelecomVoice@demo.com", PrimaryPhone="1234567890", SecondaryPhone="1234567890", Country="USA", Address1="Supa", Address2="", City="Parner", State="AR", ZipCode="41424", CreatedDate=DateTime.UtcNow, CreatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435",UpdatedDate=DateTime.UtcNow,UpdatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435", IsActive=true },
            };
            var agencies = new List<AgencyModel>
        {
           new AgencyModel{ Id=1, AgencyId="12345", AgencyName ="DemoAgency", Address1 ="Supa", Address2="", City="Parner", State="AK", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=10 },
           new AgencyModel{ Id=2, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=11 },
           new AgencyModel{ Id=3, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=47 }
        };
            var facilitiesagencies = new List<AgencyModel>
        {
           new AgencyModel{ Id=3, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=47 }
        };
            _unitOfWorkMock.Setup(uow => uow.StateRepo.GetAll()).Returns(states);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo()).Returns(false);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails()).Returns(organizationId);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAll()).Returns(facilitiesagencies);

            // Act
            var result = await _agencyController.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var agencyViewModel = Assert.IsType<AgencyViewModel>(viewResult.Model);
            Assert.NotNull(agencyViewModel);
            // Check if StateList and FacilityList are correctly populated
            Assert.Equal(states.Count, agencyViewModel.StateList.Count);
            Assert.Equal(facilities.Count, agencyViewModel.FacilityList.Count);
            Assert.Equal(facilitiesagencies.Count, agencyViewModel.AgencyIdList.Count);
        }

        [Fact]
        public void GetAgencyIdListByFacilityName_WhenFacilityDetailsIsNull_ReturnsJsonResultWithOrganizationUserActiveAgencies()
        {
            // Arrange
            var facilityName = "NonExistentFacility"; // Replace with the desired facility name
            var organizationId = "132";

            var agencies = new List<AgencyModel>
        {
            new AgencyModel { FacilityId = 13, AgencyId = "finalAgency", IsActive = true },
            new AgencyModel { FacilityId = 13, AgencyId = "RAgency", IsActive = true },
        };
            var facilities = new List<FacilityModel>
            {
                new FacilityModel{ Id=47, FacilityID="TelecomVoiceFaci", FacilityName="TelecomVoiceFaci", Address1="Supa", Address2="", City="Parner", State="AK",ZipCode="41424", CreatedDate=DateTime.UtcNow,CreatedBy="061d2430-9d89-4382-a1dd-2fe01e46d435", UpdatedDate=DateTime.UtcNow,UpdatedBy="", IsActive=true, OrganizationId=132},
            };
            var facilitiesagencies = new List<AgencyModel>
        {
           new AgencyModel{ Id=3, AgencyId="159753", AgencyName ="RahulAgency", Address1 ="Supa", Address2="", City="Parner", State="AR", ZipCode=41424, IsActive=true, CreatedDate=DateTime.UtcNow, CreatedBy="061d2430 - 9d89 - 4382 - a1dd - 2fe01e46d435" , UpdatedBy="", UpdatedDate=DateTime.UtcNow, FacilityId=47 }
        };

            // Set up mock behavior for FacilityRepo and AgencyRepo
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll())
            .Returns(new List<FacilityModel>());
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetLoggedInUserInfo()).Returns(false);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetOrganizationUserDetails()).Returns(organizationId);
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo.GetAll()).Returns(facilities);
            _unitOfWorkMock.Setup(uow => uow.AgencyRepo.GetAll()).Returns(facilitiesagencies);
            
            // Act
            var result = _agencyController.GetAgencyIdListByFacilityName(facilityName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Itemlist>>(jsonResult.Value);

            // Perform additional assertions on the model as needed
            Assert.Equal(facilitiesagencies.Count, model.Count());
        }

    }
}
