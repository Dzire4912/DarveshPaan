using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data;
using TAN.Repository.Abstractions;
using TANWeb.Areas.PBJSnap.Controllers;
using TANWeb.Interface;
using static TAN.DomainModels.Models.UploadModel;
using System.Text;
using TANWeb.Services;
using TAN.DomainModels.Entities;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using TAN.DomainModels.Helpers;
using System.Security.Claims;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using TANWeb.Areas.PBJSnap.Controllers;
using Microsoft.AspNetCore.DataProtection;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class UploadControllerMock
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IUpload> _iUpload;
        private Mock<UploadService> _uploadService;
        private readonly UploadController _controllerMock;
        private Mock<UserManager<AspNetUser>> _userManager;
        private readonly Mock<IHttpContextAccessor> _contextAccessor;
        private readonly Mock<IDataProtectionProvider> _dataProtectionProviderMock;
        public UploadControllerMock()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _iUpload = new Mock<IUpload>();
            _uploadService = new Mock<UploadService>();
            _contextAccessor = new Mock<IHttpContextAccessor>();
            _userManager = GetUserManagerMock();
            var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "TestUserId"),
            new Claim(ClaimTypes.Email,"Test@gmail.com")
        });
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            _controllerMock = new UploadController(_iUpload.Object, _userManager.Object, _unitOfWorkMock.Object, _dataProtectionProviderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
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

        [Fact]
        public async Task UploadFile_Should_Return_StatusCode_FileExist()
        {
            //Arrange
            RequestModel requestModel = new RequestModel();
            var fileMock = new Mock<IFormFile>();
            IUpload.uploadFileDetails.DtCmsList = CMSMappingData();
            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(DataTable());
            _iUpload.Setup(u => u.PreviewData(It.IsAny<DataTable>(), It.IsAny<string>())).ReturnsAsync(DataTable());
            _unitOfWorkMock.Setup(u => u.CmsMapFieldDataRepo.GetAllCMSMappingFieldData()).ReturnsAsync(IUpload.uploadFileDetails.DtCmsList);
            _unitOfWorkMock.Setup(u => u.UploadFileDetailsRepo.GetUploadFileDetailsByFileName(requestModel.Filename, requestModel.FacilityId.ToString())).ReturnsAsync(new UploadFileDetails());

            //Act  
            var expectedStatusCode = (int)UploadFileFailureSuccess.FileExist;
            var result = await _controllerMock.UploadFile(requestModel, fileMock.Object);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseUploadModel responseUploadModel = (ResponseUploadModel)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, responseUploadModel.StatusCode);
        }

        [Fact]
        public async Task UploadFile_Should_Return_StatusCode_FileExtension()
        {
            //Arrange

            RequestModel requestModel = new RequestModel();
            var fileMock = new Mock<IFormFile>();
            DataTable dataTable = DataTable();
            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(new DataTable());
            _iUpload.Setup(u => u.PreviewData(It.IsAny<DataTable>(), It.IsAny<string>())).ReturnsAsync(dataTable);
            _unitOfWorkMock.Setup(u => u.CmsMapFieldDataRepo.GetAllCMSMappingFieldData()).ReturnsAsync(CMSMappingData());

            //Act  
            var expectedStatusCode = (int)UploadFileFailureSuccess.FileExtension;
            var result = await _controllerMock.UploadFile(requestModel, fileMock.Object);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseUploadModel responseUploadModel = (ResponseUploadModel)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, responseUploadModel.StatusCode);

        }

        [Theory]
        [InlineData("Limestone_outptil1 idghtDivid.csv")]
        public async Task UploadServiceMock(string filename)
        {
            RequestModel requestModel = new RequestModel()
            {
                FacilityId = 11,
                FacilityName = "FacilityOne",
                year = 2023,
                Quarter = 2,
                Month = 6,
                UploadType = "3",
                AgencyId = "1111",
                Filename = filename,
                FileNameId = 0
            };

            bool IsValid = await CheckFileName_Should_Return_True(filename, requestModel.FacilityId.ToString(), true, filename);
            if (IsValid)
            {
                DataTable DataTablePreview = await CheckFileExtension_Should_Return_DataTable(filename);
                if (DataTablePreview != null)
                {
                    Assert.IsType<DataTable>(DataTablePreview);
                }
            }
            else
            {
                Assert.False(IsValid);
            }
        }

        [Fact]
        public async Task UploadFile_Should_Return_SuccessStatusCode()
        {
            //Arrange

            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(DataTable());
            _iUpload.Setup(u => u.GetAllCMSFieldData()).ReturnsAsync(CMSMappingData());
            _iUpload.Setup(u => u.PreviewData(It.IsAny<DataTable>(), It.IsAny<string>())).ReturnsAsync(DataTable());
            //IUpload.uploadFileDetails.DtCmsList = CMSMappingData();
            RequestModel requestModel = new RequestModel();
            var fileMock = new Mock<IFormFile>();
            var expectedResult = new ResponseUploadModel() { StatusCode = 200 };

            //Act 
            var result = await _controllerMock.UploadFile(requestModel, fileMock.Object) as PartialViewResult;
            var reqModel = result?.Model as ResponseUploadModel;

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(reqModel);
            Assert.Equal(expectedResult.StatusCode, reqModel.StatusCode);
        }

        [Fact]
        public async Task UploadFile_Should_Return_StatusCode_Server()
        {
            //Arrange

            RequestModel requestModel = new RequestModel();
            var fileMock = new Mock<IFormFile>();
            IUpload.uploadFileDetails.DtCmsList = CMSMappingData();
            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(DataTable());
            _iUpload.Setup(u => u.PreviewData(It.IsAny<DataTable>(), It.IsAny<string>())).ReturnsAsync(DataTable());
            _unitOfWorkMock.Setup(u => u.CmsMapFieldDataRepo.GetAllCMSMappingFieldData()).ReturnsAsync(IUpload.uploadFileDetails.DtCmsList);

            //Act  
            var expectedStatusCode = (int)UploadFileFailureSuccess.Server;
            var result = await _controllerMock.UploadFile(requestModel, fileMock.Object);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseUploadModel responseUploadModel = (ResponseUploadModel)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, responseUploadModel.StatusCode);

        }

        [Theory]
        [InlineData("Limestone_outptfile1 idght Divid.csv", "11111", true, "Limestone_outptfile1 idght Divid.csv")]
        [InlineData("Limestone_outptfileidght Divid.csv", "111222", false, "Limestone_outptfile1.csv")]
        public async Task<bool> CheckFileName_Should_Return_True(string filename, string facilityId, bool expectedOutput, string GetFileName)
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockFileDetailsRepo = new Mock<IUploadFileDetails>();

            mockFileDetailsRepo
                .Setup(repo => repo.GetUploadFileDetailsByFileName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UploadFileDetails { FileName = GetFileName });

            mockUnitOfWork.Setup(uow => uow.UploadFileDetailsRepo).Returns(mockFileDetailsRepo.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var service = new UploadService(mockUnitOfWork.Object, httpContextAccessorMock.Object);
            bool result = await service.CheckFileName(filename, facilityId);
            Assert.Equal(expectedOutput, result);
            return result;
        }

        [Theory]
        [InlineData("Limestone_outptil1 idghtDivid.csv")]
        [InlineData("UploadTest.xlsx")]
        public async Task<DataTable> CheckFileExtension_Should_Return_DataTable(string filename)
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var dataTable = new DataTable();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string projectRootPath = GetProjectRootPath(assembly);
            string filePath = Path.Combine(projectRootPath, "PBJSnap/UploadFiles/" + filename);
            if (File.Exists(filePath))
            {
                //file exist
            }
            var fileStream = System.IO.File.OpenRead(filePath);

            using var _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            _fileStream.Position = 0;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var fileName = Path.GetFileName(filePath);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(_fileStream.Length);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(_fileStream);
            fileMock.Setup(_ => _.ContentDisposition).Returns(string.Format("inline; filename={0}", fileName));

            _iUpload.Setup(service => service.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(dataTable);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var uploadService = new UploadService(mockUnitOfWork.Object, httpContextAccessorMock.Object);
            var result = await uploadService.CheckFileExtension(fileMock.Object);
            Assert.IsType<DataTable>(result);
            return result;
        }

        private static string GetProjectRootPath(Assembly assembly)
        {
            string assemblyLocation = assembly.Location;
            string projectRootPath = Path.GetFullPath(Path.Combine(assemblyLocation, "../../../../"));
            return projectRootPath;
        }

        public async Task<DataTable> PreviewData_Should_Return_DataTable(DataTable dataTable)
        {
            //Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            List<CMSMappingFieldData> model = CMSMappingData();
            unitOfWorkMock.Setup(u => u.CmsMapFieldDataRepo.GetAllCMSMappingFieldData()).ReturnsAsync(model);

            List<UserMappingFieldData> Usermodel = UserMappingFieldDataList();
            unitOfWorkMock.Setup(u => u.UserMappingFieldData.GetUserMappingFieldDataByFacility(It.IsAny<int>())).ReturnsAsync(Usermodel);

            var service = new UploadService(unitOfWorkMock.Object, httpContextAccessorMock.Object);
            string facilityId = "11";

            //Act
            DataTable result = await service.PreviewData(dataTable, facilityId);

            //Assert
            Assert.IsType<DataTable>(result);
            return result;
        }

        [Fact]
        public async Task MappingData_Should_Return_StatusCode_Success()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            mappingRequestData.requestModel = new RequestModel();
            int FileNameId = 111;
            DataTable UploadTable = DataTable();
            List<TimesheetEmployeeDataResponse> timesheets = new List<TimesheetEmployeeDataResponse> { new TimesheetEmployeeDataResponse() };
            mappingRequestData.MappingDatas = MappingData();

            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>())).ReturnsAsync(FileNameId);
            _iUpload.Setup(u => u.HasPresentRequiredCmsColumn(It.IsAny<List<MappingData>>())).ReturnsAsync(new List<string>());
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);
            _iUpload.Setup(upload => upload.UpdateColumnName(It.IsAny<MappingRequestData>(), It.IsAny<DataTable>()))
               .ReturnsAsync(UploadTable);
            _iUpload.Setup(upload => upload.CheckEmployee(It.IsAny<DataTable>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Employee>());
            _unitOfWorkMock.Setup(u => u.TimesheetRepo.GetWorkdayDetails(It.IsAny<TimesheetEmployeeDataRequest>())).
                ReturnsAsync(timesheets);
            _unitOfWorkMock.Setup(uow => uow.JobCodesRepo.GetAll()).Returns(new List<JobCodes>());
            _unitOfWorkMock.Setup(uow => uow.PayTypeCodesRepo.GetAll()).Returns(new List<PayTypeCodes>());


            var expectedStatusCode = (int)MappingDataStatus.Success;
            IUpload.uploadFileDetails.FileDataTable = UploadTable;
            IUpload.uploadFileDetails.FileName = "test";
            //Act 

            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);

        }

        [Fact]
        public async Task MappingData_Should_Return_StatusCode_MultipleCMSFieldSelected()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            int FileNameId = 111;
            DataTable UploadTable = new DataTable();
            mappingRequestData.MappingDatas = MappingData();
            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>())).ReturnsAsync(FileNameId);
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.UpdateColumnName(It.IsAny<MappingRequestData>(), UploadTable)).ReturnsAsync(UploadTable);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);

            var expectedStatusCode = (int)MappingDataStatus.MultipleCMS;
            //Act 

            IUpload.uploadFileDetails.FileDataTable = DataTable();
            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task MappingData_Should_Return_StatusCode_FileUploadFailed()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            int FileNameId = 0;
            DataTable UploadTable = new DataTable();
            mappingRequestData.MappingDatas = MappingData();
            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>())).ReturnsAsync(FileNameId);
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.UpdateColumnName(It.IsAny<MappingRequestData>(), UploadTable)).ReturnsAsync(UploadTable);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.HasPresentRequiredCmsColumn(It.IsAny<List<MappingData>>())).ReturnsAsync(new List<string>());
            var expectedStatusCode = (int)MappingDataStatus.FileUploadFailed;
            //Act 

            IUpload.uploadFileDetails.FileDataTable = DataTable();
            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);

        }

        [Fact]
        public async Task MappingData_Should_Return_StatusCode_MapProperly()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            mappingRequestData.requestModel = new RequestModel();
            int FileNameId = 111;
            DataTable UploadTable = DataTable();
            List<TimesheetEmployeeDataResponse> timesheets = new List<TimesheetEmployeeDataResponse> { new TimesheetEmployeeDataResponse() };
            mappingRequestData.MappingDatas = MappingData();

            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>())).ReturnsAsync(FileNameId);
            _iUpload.Setup(u => u.HasPresentRequiredCmsColumn(It.IsAny<List<MappingData>>())).ReturnsAsync(new List<string>());
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);
            _iUpload.Setup(upload => upload.UpdateColumnName(It.IsAny<MappingRequestData>(), It.IsAny<DataTable>()))
               .ReturnsAsync(new DataTable());
            _iUpload.Setup(upload => upload.CheckEmployee(It.IsAny<DataTable>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Employee>());
            _unitOfWorkMock.Setup(u => u.TimesheetRepo.GetWorkdayDetails(It.IsAny<TimesheetEmployeeDataRequest>())).
                ReturnsAsync(timesheets);
            _unitOfWorkMock.Setup(uow => uow.JobCodesRepo.GetAll()).Returns(new List<JobCodes>());
            _unitOfWorkMock.Setup(uow => uow.PayTypeCodesRepo.GetAll()).Returns(new List<PayTypeCodes>());
            //Act 
            var expectedStatusCode = (int)MappingDataStatus.MapProperly;
            IUpload.uploadFileDetails.FileDataTable = UploadTable;
            IUpload.uploadFileDetails.FileName = "test";

            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);

        }

        public List<MappingData> MappingData()
        {
            List<MappingData> mappingData = new List<MappingData>();
            MappingData mapping = new MappingData()
            {
                CmsFieldName = "First Name",
                ColumnName = "Name",
                CmsMappingID = 6
            };
            mappingData.Add(mapping);
            MappingData mapping1 = new MappingData()
            {
                CmsFieldName = "Employee Id",
                ColumnName = "Employee Id",
                CmsMappingID = 1
            };
            mappingData.Add(mapping1);
            MappingData mapping2 = new MappingData()
            {
                CmsFieldName = "Pay Code",
                ColumnName = "Pay Code",
                CmsMappingID = 1
            };
            mappingData.Add(mapping2);

            MappingData mapping3 = new MappingData()
            {
                CmsFieldName = "Employee Id",
                ColumnName = "Employee Id",
                CmsMappingID = 1
            };
            mappingData.Add(mapping3);
            MappingData mapping4 = new MappingData()
            {
                CmsFieldName = "Job Title",
                ColumnName = "Job Title",
                CmsMappingID = 1
            };
            mappingData.Add(mapping4);
            MappingData mapping5 = new MappingData()
            {
                CmsFieldName = "Job Title",
                ColumnName = "Job Title",
                CmsMappingID = 1
            };
            mappingData.Add(mapping5);
            MappingData mapping6 = new MappingData()
            {
                CmsFieldName = "Work Day",
                ColumnName = "Work Day",
                CmsMappingID = 8
            };
            mappingData.Add(mapping6);
            MappingData Hours = new MappingData()
            {
                CmsFieldName = "Hours",
                ColumnName = "Hours",
                CmsMappingID = 8
            };
            mappingData.Add(Hours);

            return mappingData;
        }

        public List<CMSMappingFieldData> CMSMappingData()
        {
            List<CMSMappingFieldData> mappingData = new List<CMSMappingFieldData>();
            CMSMappingFieldData mapping = new CMSMappingFieldData()
            {
                Id = 1,
                FieldName = "Employee Id",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping);
            CMSMappingFieldData mapping1 = new CMSMappingFieldData()
            {
                Id = 2,
                FieldName = "Hire Date",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping1);
            CMSMappingFieldData mapping2 = new CMSMappingFieldData()
            {
                Id = 3,
                FieldName = "Pay Type Code",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping2);
            CMSMappingFieldData mapping3 = new CMSMappingFieldData()
            {
                Id = 4,
                FieldName = "Job Title Code",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping3);
            CMSMappingFieldData mapping4 = new CMSMappingFieldData()
            {
                Id = 5,
                FieldName = "Employee Status",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping4);
            CMSMappingFieldData mapping5 = new CMSMappingFieldData()
            {
                Id = 6,
                FieldName = "First Name",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping5);
            CMSMappingFieldData mapping6 = new CMSMappingFieldData()
            {
                Id = 7,
                FieldName = "Last Name",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping6);
            CMSMappingFieldData mapping7 = new CMSMappingFieldData()
            {
                Id = 8,
                FieldName = "Work Day",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping7);

            CMSMappingFieldData mapping8 = new CMSMappingFieldData()
            {
                Id = 9,
                FieldName = "Hours",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping8);

            CMSMappingFieldData mapping9 = new CMSMappingFieldData()
            {
                Id = 10,
                FieldName = "Pay Code",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping9);
            CMSMappingFieldData mapping10 = new CMSMappingFieldData()
            {
                Id = 11,
                FieldName = "Job Title",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping10);

            CMSMappingFieldData mapping11 = new CMSMappingFieldData()
            {
                Id = 12,
                FieldName = "Clock Punch",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = true
            };
            mappingData.Add(mapping11);

            return mappingData;
        }

        public List<UserMappingFieldData> UserMappingFieldDataList()
        {
            List<UserMappingFieldData> model = new List<UserMappingFieldData>();
            model.Add(new UserMappingFieldData
            {
                FacilityId = 11,
                CMSMappingId = 1,
                ColumnName = "Emp Id"
            });
            model.Add(new UserMappingFieldData
            {
                FacilityId = 11,
                CMSMappingId = 8,
                ColumnName = "Work Day"
            });
            model.Add(new UserMappingFieldData
            {
                FacilityId = 11,
                CMSMappingId = 1,
                ColumnName = "Hours"
            });
            model.Add(new UserMappingFieldData
            {
                FacilityId = 11,
                CMSMappingId = 6,
                ColumnName = "First Name"
            });

            return model;
        }

        public DataTable DataTable()
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("First name", typeof(string));
            dataTable.Columns.Add("Emp Id", typeof(string));
            dataTable.Columns.Add("WorkDay", typeof(string));
            dataTable.Columns.Add("Pay Type", typeof(int));
            dataTable.Columns.Add("Job Type", typeof(int));
            dataTable.Columns.Add("Hours", typeof(decimal));

            DataRow newRow = dataTable.NewRow();
            newRow["First name"] = "sayan";
            newRow["Emp Id"] = 10;
            newRow["WorkDay"] = DateTime.Now.ToString();
            newRow["Pay Type"] = 1;
            newRow["Job Type"] = 2;
            newRow["Hours"] = 10;
            dataTable.Rows.Add(newRow);


            return dataTable;
        }

        [Fact]
        public async Task Upload_Return_FacilityList_NUll()
        {
            List<FacilityModel> facilities = new List<FacilityModel>();
            FacilityModel model = new FacilityModel()
            {
                FacilityID = "1",
                FacilityName = "Test"
            };
            facilities.Add(model);
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(facilities);
            var result = await _controllerMock.Upload();

            //Assert
            Assert.NotNull(result);
            var Upload = Assert.IsType<ViewResult>(result);
            var response = Assert.IsType<UploadView>(Upload.Model);
            Assert.Equal(1, Convert.ToInt32(response.FacilityList.Count));
        }

        //Rahuls test cases

        [Fact]
        public async Task Upload_Throws_Exception()
        {
            //Arrange
            _unitOfWorkMock.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).Throws(new Exception("An error occured while getting facilities"));

            //Act
            var result = await _controllerMock.Upload();

            //Assert
            Assert.NotNull(result);
            var Upload = Assert.IsType<ViewResult>(result);
            var response = Assert.IsType<UploadView>(Upload.Model);
            Assert.Null(response.FacilityList);
            Assert.Equal(0, response.FacilityId);
        }

        [Fact]
        public async Task UploadFile_WhenUsingOneDrive_ReturnsJsonResult()
        {
            // Arrange
            var request = new RequestModel
            {
                IsOneDrive = true,
                FileURL = "https://trelli-my.sharepoint.com/personal/prahul_trellissoft_ai/_layouts/15/download.aspx?UniqueId=f1df44be-2500-434f-be46-2e2d8f170070&Translate=false&tempauth=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvdHJlbGxpLW15LnNoYXJlcG9pbnQuY29tQDFkMzkzYzM5LWEyYzAtNDc2My1hZTM0LWUzZDc3Yzg2MzIyNiIsImlzcyI6IjAwMDAwMDAzLTAwMDAtMGZmMS1jZTAwLTAwMDAwMDAwMDAwMCIsIm5iZiI6IjE2OTYzMzgxNjUiLCJleHAiOiIxNjk2MzQxNzY1IiwiZW5kcG9pbnR1cmwiOiJtMlg0aWtoa095d2JnblZVOFQ2RWRtY2J5Njk0Z05yQURCQkR6b0JiSys4PSIsImVuZHBvaW50dXJsTGVuZ3RoIjoiMTUxIiwiaXNsb29wYmFjayI6IlRydWUiLCJjaWQiOiJKYVBZZHdndG8wV1lrcXRwclcyTUdnPT0iLCJ2ZXIiOiJoYXNoZWRwcm9vZnRva2VuIiwic2l0ZWlkIjoiTWpjME5qZGpZekV0WldJNE5pMDBNemc1TFRrNVpqUXROV0l5TldWak9UTXhabUU1IiwiYXBwX2Rpc3BsYXluYW1lIjoiRGVtb1BCSkFwcGxpY2F0aW9uIiwiZ2l2ZW5fbmFtZSI6IlJhaHVsIiwiZmFtaWx5X25hbWUiOiJQYXdhciIsImFwcGlkIjoiM2MzOWI0NDQtOWNkZS00ZGRjLTk4NzgtYTljOGI2OTE5N2QyIiwidGlkIjoiMWQzOTNjMzktYTJjMC00NzYzLWFlMzQtZTNkNzdjODYzMjI2IiwidXBuIjoicHJhaHVsQHRyZWxsaXNzb2Z0LmFpIiwicHVpZCI6IjEwMDMyMDAyMTA4QTMzMEEiLCJjYWNoZWtleSI6IjBoLmZ8bWVtYmVyc2hpcHwxMDAzMjAwMjEwOGEzMzBhQGxpdmUuY29tIiwic2NwIjoibXlmaWxlcy53cml0ZSBhbGxwcm9maWxlcy5yZWFkIiwidHQiOiIyIiwiaXBhZGRyIjoiMjAuMTkwLjE0NS4xNzEifQ.dQEwvnq4vC9D7uqe4SLscu1WYlu5e-vzg2I0A8u51TU&ApiVersion=2.0\r\n",
                FacilityId = 123,
                Month = 6,
                year = 2023,
                Quarter = 2,
                UploadType = "1",
                Filename = "OneDriveExcelFinalFile.xlsx"
            };

            var fileStreamMock = new Mock<Stream>(); // Mock the file stream for OneDrive
            var dataTableMock = new DataTable(); // Mock the data table returned by CheckOneDriveFileExtension
            var expectedPreviewTableData = new DataTable(); // Mock the expected preview table data

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(request.Filename);

            var webRequestMock = new Mock<HttpWebRequest>();
            var webResponseMock = new Mock<HttpWebResponse>();
            webRequestMock.Setup(r => r.GetResponse()).Returns(webResponseMock.Object);
            webResponseMock.Setup(r => r.GetResponseStream()).Returns(fileStreamMock.Object);

            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.CheckOneDriveFileExtension(fileStreamMock.Object, request.Filename)).ReturnsAsync(dataTableMock);
            _iUpload.Setup(u => u.GetAllCMSFieldData()).ReturnsAsync(new List<CMSMappingFieldData>());
            _iUpload.Setup(u => u.PreviewData(dataTableMock, request.FacilityId.ToString())).ReturnsAsync(expectedPreviewTableData);

            // Act
            var result = await _controllerMock.UploadFile(request, fileMock.Object);

            //Assert


            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var responseUploadModel = Assert.IsType<ResponseUploadModel>(objectResult.Value);
            Assert.Equal((int)UploadFileFailureSuccess.FileExtension, responseUploadModel.StatusCode);
        }

        [Fact]
        public async Task UploadFile_Throws_Exception()
        {
            // Arrange
            var request = new RequestModel
            {
                IsOneDrive = true,
                FileURL = "https://trelli-my.sharepoint.com/personal/prahul_trellissoft_ai/_layouts/15/download.aspx?UniqueId=f1df44be-2500-434f-be46-2e2d8f170070&Translate=false&tempauth=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvdHJlbGxpLW15LnNoYXJlcG9pbnQuY29tQDFkMzkzYzM5LWEyYzAtNDc2My1hZTM0LWUzZDc3Yzg2MzIyNiIsImlzcyI6IjAwMDAwMDAzLTAwMDAtMGZmMS1jZTAwLTAwMDAwMDAwMDAwMCIsIm5iZiI6IjE2OTYzMzgxNjUiLCJleHAiOiIxNjk2MzQxNzY1IiwiZW5kcG9pbnR1cmwiOiJtMlg0aWtoa095d2JnblZVOFQ2RWRtY2J5Njk0Z05yQURCQkR6b0JiSys4PSIsImVuZHBvaW50dXJsTGVuZ3RoIjoiMTUxIiwiaXNsb29wYmFjayI6IlRydWUiLCJjaWQiOiJKYVBZZHdndG8wV1lrcXRwclcyTUdnPT0iLCJ2ZXIiOiJoYXNoZWRwcm9vZnRva2VuIiwic2l0ZWlkIjoiTWpjME5qZGpZekV0WldJNE5pMDBNemc1TFRrNVpqUXROV0l5TldWak9UTXhabUU1IiwiYXBwX2Rpc3BsYXluYW1lIjoiRGVtb1BCSkFwcGxpY2F0aW9uIiwiZ2l2ZW5fbmFtZSI6IlJhaHVsIiwiZmFtaWx5X25hbWUiOiJQYXdhciIsImFwcGlkIjoiM2MzOWI0NDQtOWNkZS00ZGRjLTk4NzgtYTljOGI2OTE5N2QyIiwidGlkIjoiMWQzOTNjMzktYTJjMC00NzYzLWFlMzQtZTNkNzdjODYzMjI2IiwidXBuIjoicHJhaHVsQHRyZWxsaXNzb2Z0LmFpIiwicHVpZCI6IjEwMDMyMDAyMTA4QTMzMEEiLCJjYWNoZWtleSI6IjBoLmZ8bWVtYmVyc2hpcHwxMDAzMjAwMjEwOGEzMzBhQGxpdmUuY29tIiwic2NwIjoibXlmaWxlcy53cml0ZSBhbGxwcm9maWxlcy5yZWFkIiwidHQiOiIyIiwiaXBhZGRyIjoiMjAuMTkwLjE0NS4xNzEifQ.dQEwvnq4vC9D7uqe4SLscu1WYlu5e-vzg2I0A8u51TU&ApiVersion=2.0\r\n",
                FacilityId = 123,
                Month = 6,
                year = 2023,
                Quarter = 2,
                UploadType = "1",
                Filename = "OneDriveExcelFinalFile.xlsx"
            };

            var fileStreamMock = new Mock<Stream>(); // Mock the file stream for OneDrive
            var dataTableMock = new DataTable(); // Mock the data table returned by CheckOneDriveFileExtension
            var expectedPreviewTableData = new DataTable(); // Mock the expected preview table data

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(request.Filename);

            var webRequestMock = new Mock<HttpWebRequest>();
            var webResponseMock = new Mock<HttpWebResponse>();
            webRequestMock.Setup(r => r.GetResponse()).Returns(webResponseMock.Object);
            webResponseMock.Setup(r => r.GetResponseStream()).Returns(fileStreamMock.Object);

            _iUpload.Setup(u => u.CheckFileName(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("An error occured while checking file name"));
            _iUpload.Setup(u => u.CheckOneDriveFileExtension(fileStreamMock.Object, request.Filename)).ReturnsAsync(dataTableMock);
            _iUpload.Setup(u => u.GetAllCMSFieldData()).ReturnsAsync(new List<CMSMappingFieldData>());
            _iUpload.Setup(u => u.PreviewData(dataTableMock, request.FacilityId.ToString())).ReturnsAsync(expectedPreviewTableData);

            // Act
            var result = await _controllerMock.UploadFile(request, fileMock.Object);

            //Assert


            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var responseUploadModel = Assert.IsType<ResponseUploadModel>(objectResult.Value);
            Assert.Equal((int)UploadFileFailureSuccess.Server, responseUploadModel.StatusCode);
        }

        [Fact]
        public async Task MappingData_Should_Return_StatusCode_Not_Found_CMSFields()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            int FileNameId = 111;
            DataTable UploadTable = new DataTable();
            var validatedCmsColumn = new List<string> { "Column1", "Column2" }; // Simulate multiple CMS columns
            mappingRequestData.MappingDatas = MappingData();
            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.HasPresentRequiredCmsColumn(mappingRequestData.MappingDatas)).ReturnsAsync(validatedCmsColumn);
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>())).ReturnsAsync(FileNameId);
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.UpdateColumnName(It.IsAny<MappingRequestData>(), UploadTable)).ReturnsAsync(UploadTable);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);

            var expectedStatusCode = (int)MappingDataStatus.MultipleCMS;
            //Act 

            IUpload.uploadFileDetails.FileDataTable = DataTable();
            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal("Please add these many CMS field Column1,Column2", response.Message);

        }

        [Fact]
        public async Task MappingData_WhenExceptionThrown_ReturnsStatusCodeServer()
        {
            //Arrange
            MappingRequestData mappingRequestData = new MappingRequestData();
            int FileNameId = 111;
            DataTable UploadTable = new DataTable();
            mappingRequestData.MappingDatas = MappingData();
            _iUpload.Setup(u => u.HasDuplicateColumns(It.IsAny<MappingRequestData>())).ReturnsAsync(false);
            _iUpload.Setup(u => u.HasPresentRequiredCmsColumn(mappingRequestData.MappingDatas)).ReturnsAsync(new List<string>());
            _iUpload.Setup(u => u.AddFilenName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RequestModel>()))
            .ThrowsAsync(new Exception("An Error Occured"));
            _iUpload.Setup(u => u.AddUpdateMappingField(It.IsAny<MappingRequestData>())).ReturnsAsync(true);
            _iUpload.Setup(u => u.UpdateColumnName(It.IsAny<MappingRequestData>(), UploadTable)).ReturnsAsync(UploadTable);
            _iUpload.Setup(u => u.BusinessLogic(It.IsAny<RequestModel>(), It.IsAny<DataTable>())).ReturnsAsync(true);

            var expectedStatusCode = (int)MappingDataStatus.Server;
            //Act 

            IUpload.uploadFileDetails.FileDataTable = DataTable();
            var result = await _controllerMock.MappingData(mappingRequestData);

            //Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            ResponseMapping response = (ResponseMapping)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal("Server failed. Please try again!", response.Message);

        }
    }
}
