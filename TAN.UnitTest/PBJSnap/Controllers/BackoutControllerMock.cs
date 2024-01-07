using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Areas.PBJSnap.Controllers;
using TANWeb.Interface;
using static TAN.DomainModels.Models.UploadModel;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class BackoutControllerMock
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IUpload> _iUpload;
        private Mock<UserManager<AspNetUser>> _userManagerMock;
        public BackoutControllerMock()
        {
            _iUpload = new Mock<IUpload>();
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
        public async Task LoadBackoutPageReturnViewResult()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            mockUow.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(new List<FacilityModel>());
            var result = await controller.Backout() as ViewResult;
            Assert.NotNull(result);
            var viewModel = result?.Model as BackoutModal;
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task LoadBackoutPageThrowsException()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            mockUow.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).Throws(new Exception("An error occured while getting facility details"));
            var result = await controller.Backout() as ViewResult;
            Assert.NotNull(result);
            var viewModel = result?.Model as BackoutModal;
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.FacilityList);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BackOutAllData_Returns_BackOutAllRecordResponse(bool StatusResult)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            BackOutAllRecordRequest recordRequest = new BackOutAllRecordRequest()
            {
                FacilityId = 1,
                FileDetailsId = 1,
            };
            BackOutAllRecordResponse recordResponse = new BackOutAllRecordResponse()
            {
                status = StatusResult,
                Message = ""
            };
            mockUow.Setup(u => u.UploadFileDetailsRepo.BackOutAllRecord(recordRequest)).ReturnsAsync(recordResponse);
            var result = await controller.BackOutAllData(recordRequest);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<BackOutAllRecordResponse>(jsonResult.Value);
            if (StatusResult)
                Assert.True(response.status);
            else
                Assert.False(response.status);
        }

        [Theory]
        [InlineData(false)]
        public async Task BackOutAllData_Throws_Exception(bool statusRequest)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            BackOutAllRecordRequest recordRequest = new BackOutAllRecordRequest()
            {
                FacilityId = 1,
                FileDetailsId = 1,
            };
            BackOutAllRecordResponse recordResponse = new BackOutAllRecordResponse()
            {
                status = statusRequest,
                Message = ""
            };
            mockUow.Setup(u => u.UploadFileDetailsRepo.BackOutAllRecord(recordRequest)).Throws(new Exception("An error occured while backout data"));
            var result = await controller.BackOutAllData(recordRequest);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<BackOutAllRecordResponse>(jsonResult.Value);
            Assert.False(response.status);
            Assert.Equal("Failed to Delete", response.Message);
        }


        [Theory]
        [MemberData(nameof(MockTestDataForGetErrorRowData))]
        public async Task GetErrorRowData_Returns_ErrorData(ErrorData errorData, int Count)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            mockUow.Setup(u => u.UploadFileDetailsRepo.GetErrorRawData(1)).ReturnsAsync(errorData);
            var result = await controller.GetErrorRowData("1");
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ErrorData>(jsonResult.Value);
            Assert.Equal(Count, response.keyValueList.Count);

        }

        [Fact]
        public async Task GetErrorRowData_Throws_Exception()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            mockUow.Setup(u => u.UploadFileDetailsRepo.GetErrorRawData(1)).Throws(new Exception("An error occured while gerring error data"));
            var result = await controller.GetErrorRowData("1");
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ErrorData>(jsonResult.Value);
            Assert.Null(response.keyValueList);

        }

        [Theory]
        [MemberData(nameof(MockTestDataForUpdateErrorRowData))]
        public async Task GetErrorRowData_Returns_UpdateErrorRowData(ErrorData errorData, ValidatDataResponse validatDataResponse)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            bool check = validatDataResponse.Success == true ? true : false;

            _iUpload.Setup(u => u.ReValidatedWorkingHours(errorData)).ReturnsAsync(validatDataResponse);
            mockUow.Setup(u => u.FileErrorRecordsRepo.UpdateErrorValidationRecords(Convert.ToInt32(errorData.Id))).ReturnsAsync(check);

            var result = await controller.UpdateErrorRowData(errorData);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ValidatDataResponse>(jsonResult.Value);
            if (response.Success == true)
                Assert.True(response.Success);
            else
                Assert.False(response.Success);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForUpdateErrorRowDataForException))]
        public async Task GetErrorRowData_Returns_Exception(ErrorData errorData, ValidatDataResponse validatDataResponse)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            bool check = validatDataResponse.Success == true ? true : false;

            _iUpload.Setup(u => u.ReValidatedWorkingHours(errorData)).Throws(new Exception("An error occured while revalidating records"));
            mockUow.Setup(u => u.FileErrorRecordsRepo.UpdateErrorValidationRecords(Convert.ToInt32(errorData.Id))).ReturnsAsync(check);

            var result = await controller.UpdateErrorRowData(errorData);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, jsonResult.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForDeleteErrorRowData))]
        public async Task DeleteErrorRowData_Returns_ResponseWithMessage(int Id, bool status)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            mockUow.Setup(u => u.UploadFileDetailsRepo.DeleteErrorRawData(Id)).ReturnsAsync(status);

            var result = await controller.DeleteErrorRowData(Id.ToString());
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response>(jsonResult.Value);
            if (status)
                Assert.Equal(200, response.StatusCode);
            else
                Assert.Equal(400, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForDeleteErrorRowDataForException))]
        public async Task DeleteErrorRowData_Throws_Exception(int Id, bool status)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            mockUow.Setup(u => u.UploadFileDetailsRepo.DeleteErrorRawData(Id)).Throws(new Exception("An error occured while deleting error raw data"));

            var result = await controller.DeleteErrorRowData(Id.ToString());
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response>(jsonResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Equal("Failed to delete the record", response.Message);
        }


        [Theory]
        [MemberData(nameof(MockTestDataForDeleteInvalidRecord))]
        public async Task DeleteInvalidRecord_Returns_ResponseWithMessage(string FileDetailId, bool status)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            mockUow.Setup(u => u.UploadFileDetailsRepo.DeleteInvalidRecord(Convert.ToInt32(FileDetailId))).ReturnsAsync(status);

            var result = await controller.DeleteInvalidRecord(FileDetailId);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response>(jsonResult.Value);
            if (status)
                Assert.Equal(200, response.StatusCode);
            else
                Assert.Equal(400, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(MockTestDataForDeleteInvalidRecordForException))]
        public async Task DeleteInvalidRecord_Throws_Exception(string FileDetailId, bool status)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            mockUow.Setup(u => u.UploadFileDetailsRepo.DeleteInvalidRecord(Convert.ToInt32(FileDetailId))).Throws(new Exception("An error occured while deleting invalid records"));

            var result = await controller.DeleteInvalidRecord(FileDetailId);
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response>(jsonResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Equal("Failed to delete invalid record", response.Message);
        }

        [Fact]
        public async Task GetAllFileNameByFacility_ReturnsListOfFileDetails()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetAllFileDetails(It.IsAny<FileDetailListRequest>()))
                .ReturnsAsync(new List<FileDetailList>
                {
            new FileDetailList { FileName = "file1.csv" },
            new FileDetailList { FileName = "file2.csv" },
                });

            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            var request = new FileDetailListRequest { FacilityId = 1 };

            // Act
            var result = await controller.GetAllFileNameByFacility(request);

            // Assert
            Assert.IsType<JsonResult>(result);
            var model = (result as JsonResult).Value as List<FileDetailList>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
            Assert.Equal("file1.csv", model[0].FileName);
            Assert.Equal("file2.csv", model[1].FileName);
        }

        [Fact]
        public async Task GetAllFileNameByFacility_Throws_Exception()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetAllFileDetails(It.IsAny<FileDetailListRequest>()))
                .Throws(new Exception("An error occured while getting file details"));
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            var request = new FileDetailListRequest { FacilityId = 1 };
            // Act
            var result = await controller.GetAllFileNameByFacility(request);
            // Assert
            Assert.IsType<JsonResult>(result);
            var model = (result as JsonResult).Value as List<FileDetailList>;
            Assert.NotNull(model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetFileErrorData_ReturnsListOfFileErrorList()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var expectedFileErrorList = new List<FileErrorList>
                {
            new FileErrorList {Id=1, FileName = "file1.csv", FileRowData="", FileDetailId=3, FacilityId=10,ReportQuarter=3, Month=0,Year=2023  },
            new FileErrorList {Id=2, FileName = "file1.csv", FileRowData="", FileDetailId=3, FacilityId=10,ReportQuarter=3, Month=0,Year=2023 },
                };
            int totalRecord = 2;
            int filterRecord = 2;
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetFileErrorList(It.IsAny<BackOutAllRecordRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Returns(expectedFileErrorList);
            var request = new BackOutAllRecordRequest { FacilityId = 1 };
            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            // Set up the HttpContext for the controller
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;
            // Set up the form values in the HttpContext
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
            });

            // Act
            var result = await controller.GetFileErrorData(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<FileErrorList>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(expectedFileErrorList.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetFileErrorData_Throws_exception()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var expectedFileErrorList = new List<FileErrorList>();
            int totalRecord = 0;
            int filterRecord = 0;
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetFileErrorList(It.IsAny<BackOutAllRecordRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Throws(new Exception("An error occured while getting getting file error list"));
            var request = new BackOutAllRecordRequest { FacilityId = 1 };

            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
            });

            // Act
            var result = await controller.GetFileErrorData(request);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<FileErrorList>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(expectedFileErrorList.Count(), dataObj.Count());
        }

        [Fact]
        public void GetUploadFileDetails_ReturnsListOfFileDetailsResponse()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var expectedFileList = new List<FileDetailsResponse>
                {
            new FileDetailsResponse { FileName = "file1.csv", FacilityID="1", ReportQuarter=3 },
            new FileDetailsResponse { FileName = "file1.csv", FacilityID="1", ReportQuarter=3 },
                };
            int totalRecord = 2;
            int filterRecord = 2;
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetFileRecord(It.IsAny<BackoutTableRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Returns(expectedFileList);

            var request = new BackoutTableRequest { FacilityID = 1 };

            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
            });

            // Act
            var result = controller.GetUploadFileDetails(request);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<FileDetailsResponse>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(expectedFileList.Count(), dataObj.Count());
        }

        [Fact]
        public void GetUploadFileDetails_Throws_Exception()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var expectedFileList = new List<FileDetailsResponse>();
            int totalRecord = 0;
            int filterRecord = 0;
            mockUow.Setup(m => m.UploadFileDetailsRepo.GetFileRecord(It.IsAny<BackoutTableRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Throws(new Exception("An error occured while getting file records"));
            var request = new BackoutTableRequest { FacilityID = 1 };

            var controller = new BackoutUploadFileController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;

            // Set up the form values in the HttpContext
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
            });
            // Act
            var result = controller.GetUploadFileDetails(request);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<FileDetailsResponse>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(expectedFileList.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetFileValidData_Returns_JsonResult_With_Correct_Data()
        {
            var validDataViewModelList = new List<ValidDataViewModel>
                {
                    new ValidDataViewModel
                     {
                        EmployeeId="101",
                        FirstName="Rahul",
                        LastName="Pawar",
                        PayTypeCode=1,
                        Workday=DateTime.UtcNow.ToString(),
                        THours=1,
                        FacilityId=1,
                        JobTitleCode=10
                    },
                    new ValidDataViewModel
                     {
                        EmployeeId="102",
                        FirstName="Kapil",
                        LastName="Dev",
                        PayTypeCode=2,
                        Workday=DateTime.UtcNow.ToString(),
                        THours=12,
                        FacilityId=1,
                        JobTitleCode=12
                    }
            };
            int totalRecord = 2;
            int filterRecord = 2;
            _unitOfWorkMock.Setup(m => m.UploadFileDetailsRepo.GetFileValidRecordList(It.IsAny<BackOutAllRecordRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Returns(validDataViewModelList);
            var request = new BackOutAllRecordRequest { FacilityId = 1, FileDetailsId = 123 };
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("employeeId") },
                { "order[0][dir]", new StringValues("asc") },
            });
            // Act
            var result = await controller.GetFileValidData(request);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<ValidDataViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(validDataViewModelList.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetFileValidData_Returns_JsonResult_With_InCorrect_Data_ExceptionOccures()
        {
            var validDataViewModelList = new List<ValidDataViewModel>();
            int totalRecord = 0;
            int filterRecord = 0;

            _unitOfWorkMock.Setup(m => m.UploadFileDetailsRepo.GetFileValidRecordList(It.IsAny<BackOutAllRecordRequest>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecord, It.IsAny<string>()))
                .Throws(new Exception("An error occured while getting valid list"));
            var request = new BackOutAllRecordRequest { FacilityId = 1, FileDetailsId = 123 };

            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Set up the HttpContext for the controller
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            controller.ControllerContext = controllerContext;
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("employeeId") },
                { "order[0][dir]", new StringValues("asc") },
            });
            // Act
            var result = await controller.GetFileValidData(request);
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<ValidDataViewModel>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(validDataViewModelList.Count(), dataObj.Count());
        }

        [Fact]
        public void GetFacilityName_ReturnsJsonResult()
        {
            // Arrange
            var facilityId = "1"; // Set the facilityId parameter
            var expectedFacilityName = "TestFacility";
            _unitOfWorkMock.Setup(repo => repo.UploadFileDetailsRepo.GetFacilityName(It.IsAny<int>()))
                .Returns(expectedFacilityName);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.GetFacilityName(facilityId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var mockedFacilityName = result.Value as string;
            Assert.Equal(expectedFacilityName, mockedFacilityName);
        }

        [Fact]
        public void GetFacilityName_HandlesException()
        {
            // Arrange
            var facilityId = "1"; // Set the facilityId parameter

            _unitOfWorkMock.Setup(repo => repo.UploadFileDetailsRepo.GetFacilityName(It.IsAny<int>()))
                .Throws(new Exception("An error occured while getting facility name"));
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.GetFacilityName(facilityId) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var mockedFacilityName = result.Value; // Should be null due to the exception
            Assert.Null(mockedFacilityName);
        }

        [Fact]
        public async Task GetFileNameByFacility_ReturnsJsonResult()
        {
            // Arrange
            var request = new FileDetailListRequest
            {
                FacilityId = 1,
                ReportQuarter = 1,
                Year = 2023,
                FileDetailsId = 123,
            };
            var expectedFileDetail = new FileDetailList
            {
                FileNameId = 123,
                FileName = "demo.csv"
            };
            _unitOfWorkMock.Setup(repo => repo.UploadFileDetailsRepo.GetFileDetails(request))
                .ReturnsAsync(expectedFileDetail);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = await controller.GetFileNameByFacility(request) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var fileDetail = result.Value as FileDetailList;
            Assert.Equal(expectedFileDetail, fileDetail);
        }

        [Fact]
        public async Task GetFileNameByFacility_HandlesException()
        {
            // Arrange
            var request = new FileDetailListRequest
            {
                FacilityId = 1,
                ReportQuarter = 1,
                Year = 2023,
                FileDetailsId = 123,
            };
            _unitOfWorkMock.Setup(repo => repo.UploadFileDetailsRepo.GetFileDetails(request))
                .Throws(new Exception("An error occured while getting file details by file Id"));
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = await controller.GetFileNameByFacility(request) as JsonResult;
            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);
            var fileDetail = result.Value as FileDetailList;
            Assert.Null(fileDetail.FileName);
            Assert.Equal(fileDetail.FileNameId, 0);
        }

        [Fact]
        public void ExportInvalidFileErrorDynamicDataToCsv_ReturnsFileResult()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";

            dataTableMock.Columns.Add("Column1", typeof(string));
            dataTableMock.Columns.Add("Column2", typeof(int));
            dataTableMock.Rows.Add("Value1", 123);
            dataTableMock.Rows.Add("Value2", 456);
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetInvalidErrorFileDynamicDataForCsv(It.IsAny<int>()))
                .Returns(dataTableMock);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportInvalidFileErrorDynamicDataToCsv("", "", "", fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}", fileResult.FileDownloadName);
        }

        [Fact]
        public void ExportInvalidFileErrorDynamicDataToCsv_ReturnsFileResultOnThrowsException()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetInvalidErrorFileDynamicDataForCsv(It.IsAny<int>()))
                .Throws(new Exception("An error occured while getting datatable for csv"));
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportInvalidFileErrorDynamicDataToCsv("", "", "", fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public void ExportValidFileErrorDynamicDataToCsv_ReturnsFileResult()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";
            var facilityId = "1";

            dataTableMock.Columns.Add("Column1", typeof(string));
            dataTableMock.Columns.Add("Column2", typeof(int));
            dataTableMock.Rows.Add("Value1", 123);
            dataTableMock.Rows.Add("Value2", 456);
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetValidFileDataForCsv(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(dataTableMock);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportValidFileDataToCsv(facilityId, fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}", fileResult.FileDownloadName);
        }

        [Fact]
        public void ExportValidFileErrorDynamicDataToCsv_ReturnsFileResultOnThrowsException()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";
            var facilityId = "1";

            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetValidFileDataForCsv(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("An error occured while getting datatable for csv"));
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportValidFileDataToCsv(facilityId, fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public void ExportInvalidFileErrorDataToCsv_ReturnsFileResult()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";
            var facilityId = "1";
            var reportQuarter = "1";
            var year = "2023";
            dataTableMock.Columns.Add("Column1", typeof(string));
            dataTableMock.Columns.Add("Column2", typeof(int));
            dataTableMock.Rows.Add("Value1", 123);
            dataTableMock.Rows.Add("Value2", 456);
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetInvalidErrorFileDataForCsv(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(dataTableMock);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportInvalidFileErrorDataToCsv(facilityId, year, reportQuarter, fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}", fileResult.FileDownloadName);
        }

        [Fact]
        public void ExportInvalidFileErrorDataToCsv_ReturnsFileResultOnThrowsException()
        {
            // Arrange
            var dataTableMock = new DataTable(); // You can use a library like Moq.DataSet to mock DataTable
            var fileName = "TestFileName";
            var fileDetailsId = "1";
            var facilityId = "1";
            var reportQuarter = "1";
            var year = "2023";
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetInvalidErrorFileDataForCsv(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("An error occured while getting datatable for csv"));
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = controller.ExportInvalidFileErrorDataToCsv(facilityId, year, reportQuarter, fileName, fileDetailsId);
            // Assert
            Assert.IsType<FileContentResult>(result);
            var fileResult = result as FileContentResult;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal($"{fileName}.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ListValidationErrorByFileId_Returns_JsonResult()
        {
            // Arrange
            var request = new ValidationErrorListRequest
            {
                FileDetailId = 1,
                PageNo = 1,
                PageSize = 10,
                TotalCount = 2,
                RowCount = 2
            };

            var listDynamicDataTableModel = new List<DynamicDataTableModel>
            {
                new DynamicDataTableModel
                {
                  DynamicDataTable = new Dictionary<string, object>
                    {
                        { "Key1", "Value1" },
                        { "Key2", 123 }
                    }
                },
                new DynamicDataTableModel
                {
                    DynamicDataTable = new Dictionary<string, object>
                    {
                         { "Key1", "Value2" },
                         { "Key2", 456 }
                    }
                }
            };
            var fileDetails = new UploadFileIdResponse
            {
                FileName = "DemoFile",
                FacilityName = "DemoFacility"
            };
            var listTotalCount = 2;
            _unitOfWorkMock.Setup(uow => uow.UploadFileDetailsRepo.ListValidationByFileId(request, out listTotalCount))
                .Returns(listDynamicDataTableModel);
            _unitOfWorkMock.Setup(uow => uow.UploadFileDetailsRepo.GetUploadFileDetailsByFileId(request.FileDetailId))
                .ReturnsAsync(fileDetails);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = await controller.ListValidationErrorByFileId(request);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ValidationErrorListResponse>(jsonResult.Value);
            Assert.Equal(listDynamicDataTableModel, response.dynamicDataTableModel);
            Assert.Equal(listTotalCount, response.TotalCount);
            Assert.Equal(fileDetails.FileName, response.uploadFileDetails.FileName);
            Assert.Equal(fileDetails.FacilityName, response.uploadFileDetails.FacilityName);
        }

        [Fact]
        public async Task ListValidationErrorByFileId_Returns_JsonResultOnThrowsException()
        {
            // Arrange
            var request = new ValidationErrorListRequest
            {
                FileDetailId = 1,
                PageNo = 1,
                PageSize = 10,
                TotalCount = 2,
                RowCount = 2
            };

            var fileDetails = new UploadFileIdResponse
            {
                FileName = "DemoFile",
                FacilityName = "DemoFacility"
            };

            var listTotalCount = 0;
            _unitOfWorkMock.Setup(uow => uow.UploadFileDetailsRepo.ListValidationByFileId(request, out listTotalCount))
                .Throws(new Exception("An error occured while getting dynamic datatable"));
            _unitOfWorkMock.Setup(uow => uow.UploadFileDetailsRepo.GetUploadFileDetailsByFileId(request.FileDetailId))
                .ReturnsAsync(fileDetails);
            var controller = new BackoutUploadFileController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);
            // Act
            var result = await controller.ListValidationErrorByFileId(request);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<ValidationErrorListResponse>(jsonResult.Value);
            Assert.Null(response.dynamicDataTableModel);
            Assert.Null(response.uploadFileDetails);
        }

        public static IEnumerable<object[]> MockTestDataForGetErrorRowData()
        {
            yield return new object[] { new ErrorData { FacilityId=1, FileDetailId=1, Month = 0, ReportQuarter = 3, UploadType = 3, Year = 2023,
                keyValueList = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Employee Id", "Value1"),
            new KeyValuePair<string, string>("Pay Type", "Value2")
        } },2};
            yield return new object[] { new ErrorData { FacilityId=1, FileDetailId=1, Month = 0, ReportQuarter = 3, UploadType = 3, Year = 2023,
                keyValueList = new List<KeyValuePair<string, string>>() },0 };
        }


        public static IEnumerable<object[]> MockTestDataForUpdateErrorRowData()
        {
            yield return new object[] { new ErrorData { FacilityId=1, FileDetailId=1, Month = 0, ReportQuarter = 3, UploadType = 3, Year = 2023,
                keyValueList = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Employee Id", "Value1"),
            new KeyValuePair<string, string>("Pay Type", "Value2")
        } }, new ValidatDataResponse { Success=true, ErrorMessage="" } };

            yield return new object[] { new ErrorData { FacilityId=1, FileDetailId=1, Month = 0, ReportQuarter = 3, UploadType = 3, Year = 2023,
                keyValueList = new List<KeyValuePair<string, string>>() }, new ValidatDataResponse { Success=false, ErrorMessage="" } };
        }



        public static IEnumerable<object[]> MockTestDataForUpdateErrorRowDataForException()
        {
            yield return new object[] { new ErrorData { FacilityId=1, FileDetailId=1, Month = 0, ReportQuarter = 3, UploadType = 3, Year = 2023,
                keyValueList = new List<KeyValuePair<string, string>>() }, new ValidatDataResponse { Success=false, ErrorMessage="" } };
        }

        public static IEnumerable<object[]> MockTestDataForDeleteErrorRowData()
        {
            yield return new object[] { 1, true };

            yield return new object[] { 0, false };
        }

        public static IEnumerable<object[]> MockTestDataForDeleteErrorRowDataForException()
        {
            yield return new object[] { 0, false };
        }

        public static IEnumerable<object[]> MockTestDataForDeleteInvalidRecord()
        {
            yield return new object[] { "1", true };

            yield return new object[] { "0", false };
        }

        public static IEnumerable<object[]> MockTestDataForDeleteInvalidRecordForException()
        {
            yield return new object[] { "0", false };
        }
    }
}
