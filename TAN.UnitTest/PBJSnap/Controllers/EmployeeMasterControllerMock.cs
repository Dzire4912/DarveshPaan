using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Areas.PBJSnap.Controllers;
using TANWeb.Interface;
using TANWeb.Services;
using static TAN.DomainModels.Models.UploadModel;

namespace TAN.UnitTest.PBJSnap.Controllers
{
    public class EmployeeMasterControllerMock
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IUpload> _iUpload;
        private Mock<UserManager<AspNetUser>> _userManagerMock;
        public EmployeeMasterControllerMock()
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
        public async Task LoadIndexPageReturnViewResult()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new EmployeeMasterController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

            mockUow.Setup(u => u.FacilityRepo.GetAllFacilitiesByOrgId()).ReturnsAsync(new List<FacilityModel>());
            var result = await controller.Index() as ViewResult;
            Assert.NotNull(result);
            var viewModel = result?.Model as EmployeeViewModel;
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task Index_Returns_View_throws_Exception()
        {
            // Arrange
            var mockFacilityRepo = new Mock<IFacilityRepository>(); // Assuming you have an IFacilityRepository interface

            // Set up the mock behavior to throw an exception when GetAllFacilitiesByOrgId is called
            mockFacilityRepo.Setup(repo => repo.GetAllFacilitiesByOrgId())
                .Throws(new Exception("An error occured"));

            // Set up the mock unit of work to return the mock facility repository
            _unitOfWorkMock.Setup(uow => uow.FacilityRepo).Returns(mockFacilityRepo.Object);

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

        }


        [Theory]
        [InlineData("employeeimporttemplate.csv", 200, "1")]
        [InlineData("employeeimporttemplate.csv", 402, "1")]
        [InlineData("UploadTest.xlsx", 401, "1")]
        [InlineData("employeeimporttemplate.csv", 403, "1")]
        public async Task SaveEmployeeDetail_Returns_StatusCode(string filename, int expectedStatusCode, string facility)
        {
            var mockUow = new Mock<IUnitOfWork>();
            var controller = new EmployeeMasterController(mockUow.Object, _userManagerMock.Object, _iUpload.Object);

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
            DataTable dataTable = new DataTable();
            if (expectedStatusCode == 200)
            {
                dataTable = DataTable();
            }
            else if (expectedStatusCode == 403)
            {
                dataTable = DataTableForStatus403();
            }
            else if (expectedStatusCode == 402 || expectedStatusCode == 401)
            {
                dataTable = new DataTable();
            }
            _iUpload.Setup(service => service.CheckFileExtension(It.IsAny<IFormFile>())).ReturnsAsync(dataTable);
            ImportFileResponse importFileResponse = new ImportFileResponse();
            importFileResponse.StatusCode = 200;
            _iUpload.Setup(service => service.ImportEmployee(It.IsAny<DataTable>(), It.IsAny<int>())).ReturnsAsync(importFileResponse);
            var result = await controller.SaveEmployeeDetail(facility, fileMock.Object);
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<JsonResult>(result);
            ImportFileResponse responseModel = (ImportFileResponse)statusCodeResult.Value;
            Assert.Equal(expectedStatusCode, responseModel?.StatusCode);
        }

        private static string GetProjectRootPath(Assembly assembly)
        {
            string assemblyLocation = assembly.Location;
            string projectRootPath = Path.GetFullPath(Path.Combine(assemblyLocation, "../../../../"));
            return projectRootPath;
        }

        public DataTable DataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id", typeof(string));
            dataTable.Columns.Add("First Name", typeof(string));
            dataTable.Columns.Add("Last Name", typeof(string));
            dataTable.Columns.Add("Hire Date", typeof(string));
            dataTable.Columns.Add("Termination Date", typeof(string));
            dataTable.Columns.Add("Pay Type", typeof(int));
            dataTable.Columns.Add("Job Title", typeof(int));

            DataRow newRow = dataTable.NewRow();
            newRow["Employee Id"] = "Emp001";
            newRow["First name"] = "DEF";
            newRow["Last Name"] = "Abc";
            newRow["Hire Date"] = DateTime.Now.ToString();
            newRow["Termination Date"] = DateTime.Now.ToString();
            newRow["Pay Type"] = 1;
            newRow["Job Title"] = 2;
            dataTable.Rows.Add(newRow);
            return dataTable;
        }

        public DataTable DataTableForStatus403()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Employ Id", typeof(string));
            dataTable.Columns.Add("First Name", typeof(string));
            dataTable.Columns.Add("Last Name", typeof(string));
            dataTable.Columns.Add("Hire Date", typeof(string));
            dataTable.Columns.Add("Termination Date", typeof(string));
            dataTable.Columns.Add("Pay Type", typeof(int));
            dataTable.Columns.Add("Job Title", typeof(int));

            DataRow newRow = dataTable.NewRow();
            newRow["Employ Id"] = "Emp001";
            newRow["First name"] = "DEF";
            newRow["Last Name"] = "Abc";
            newRow["Hire Date"] = DateTime.Now.ToString();
            newRow["Termination Date"] = DateTime.Now.ToString();
            newRow["Pay Type"] = 1;
            newRow["Job Title"] = 2;
            dataTable.Rows.Add(newRow);
            return dataTable;
        }

        [Fact]
        public async Task EmployeeList_Returns_Valid_Data()
        {
            // Arrange
            var facilityId = "10"; // Replace with a valid facility ID for your test
            var searchValue = ""; // Replace with a search value for your test
            var start = "0";
            var length = "10";
            var sortColumn = "firstName"; // Replace with the desired sort column for your test
            var sortDirection = "asc"; // Replace with the desired sort direction for your test
            var draw = 1;

            var mockEmployeeRepo = new Mock<IEmployee>(); // Assuming you have an employee repository interface

            // Set up mock data and method calls for a successful scenario
            var pageSize = 10;
            var skip = 0;
            var sortOrder = "firstname_asc"; // Adjust this to match your expected sortOrder format

            int totalRecord = 2;
            int filterRecord = 2;

            var employeeData = new List<EmployeeData>
            {
                new EmployeeData()
                {
                    Id = 1,
                    EmployeeId = "2002",
                    FacilityId = "10",
                    JobTitle = "16",
                    PayType = "1",
                    FirstName = "Rahul",
                    LastName = "Pawar",
                    HireDate = DateTime.UtcNow.ToString(),
                    TerminationDate = DateTime.UtcNow.ToString(),
                    CreateDate = DateTime.UtcNow.ToString(),
                    IsActive = true
                },
                new EmployeeData()
                {
                    Id = 4,
                    EmployeeId = "1001",
                    FacilityId = "10",
                    JobTitle = "16",
                    PayType = "2",
                    FirstName = "abc",
                    LastName = "xyz",
                    HireDate = DateTime.UtcNow.ToString(),
                    TerminationDate = DateTime.UtcNow.ToString(),
                    CreateDate = DateTime.UtcNow.ToString(),
                    IsActive = true
                },
            };

            var totalCount = employeeData.Count;

            mockEmployeeRepo.Setup(repo => repo.GetAllEmployeesData(pageSize, skip, facilityId, out totalCount, searchValue, sortOrder))
                .Returns(employeeData);

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);
            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Simulate request data
            var result = await controller.EmployeeList(facilityId, searchValue, start, length, sortColumn, sortDirection, draw) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<EmployeeData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(employeeData.Count(), dataObj.Count());
        }

        [Fact]
        public async Task EmployeeList_Throws_Exception()
        {
            // Arrange
            var facilityId = "10"; // Replace with a valid facility ID for your test
            var searchValue = ""; // Replace with a search value for your test
            var start = "0";
            var length = "10";
            var sortColumn = "firstName"; // Replace with the desired sort column for your test
            var sortDirection = "asc"; // Replace with the desired sort direction for your test
            var draw = 1;

            var mockEmployeeRepo = new Mock<IEmployee>(); // Assuming you have an employee repository interface

            // Set up mock data and method calls for a successful scenario
            var pageSize = 10;
            var skip = 0;
            var sortOrder = "firstname_asc"; // Adjust this to match your expected sortOrder format

            int totalRecord = 0;
            int filterRecord = 0;

            var employeeData = new List<EmployeeData>();

            var totalCount = employeeData.Count;

            mockEmployeeRepo.Setup(repo => repo.GetAllEmployeesData(pageSize, skip, facilityId, out totalCount, searchValue, sortOrder))
                 .Throws(new Exception("An error occured while getting employees data"));

            _unitOfWorkMock.Setup(uow => uow.EmployeeRepo).Returns(mockEmployeeRepo.Object);
            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Simulate request data
            var result = await controller.EmployeeList(facilityId, searchValue, start, length, sortColumn, sortDirection, draw) as JsonResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonResult>(result);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var dataObj = Assert.IsAssignableFrom<IEnumerable<EmployeeData>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Equal(totalRecord, jsonResult.Value.GetType().GetProperty("recordsTotal").GetValue(jsonResult.Value));
            Assert.Equal(filterRecord, jsonResult.Value.GetType().GetProperty("recordsFiltered").GetValue(jsonResult.Value));
            Assert.Equal(employeeData.Count(), dataObj.Count());
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsJsonResult()
        {
            // Arrange
            var request = new GetEmployeeDetailsRequest { FacilityId = "10", EmployeeId = "2002" };
            var expectedEmployee = new Employee
            {
                Id = 1,
                EmployeeId = "2002",
                FacilityId = 10,
                JobTitleCode = 16,
                PayTypeCode = 1,
                FirstName = "Rahul",
                LastName = "Pawar",
                HireDate = DateTime.UtcNow,
                TerminationDate = DateTime.UtcNow,
                IsActive = true
            };

            var employeeRepoMock = new Mock<IEmployee>();
            _unitOfWorkMock.Setup(u => u.EmployeeRepo).Returns(employeeRepoMock.Object);
            employeeRepoMock.Setup(repo => repo.GetEmployeeById(request)).ReturnsAsync(expectedEmployee);

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.GetEmployeeById(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var employee = Assert.IsType<Employee>(jsonResult.Value);
            Assert.Equal(expectedEmployee, employee);
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsThrowsException()
        {
            // Arrange
            var request = new GetEmployeeDetailsRequest { FacilityId = "10", EmployeeId = "2002" };
            var expectedStatusCode = 404;

            var employeeRepoMock = new Mock<IEmployee>();
            _unitOfWorkMock.Setup(u => u.EmployeeRepo).Returns(employeeRepoMock.Object);
            employeeRepoMock.Setup(repo => repo.GetEmployeeById(request)).Throws(new Exception("An error occured while getting employee details by Id"));

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.GetEmployeeById(request);

            // Assert
            var jsonResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(expectedStatusCode, jsonResult.StatusCode);
        }

        [Fact]
        public async Task SaveEmployeeDetail_ValidFile_ShouldReturnSuccessResponse()
        {
            // Arrange
            var facilityId = "1";
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.csv");
            var dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id");
            dataTable.Columns.Add("First Name");
            dataTable.Columns.Add("Last Name");
            dataTable.Columns.Add("Hire Date");
            dataTable.Columns.Add("Termination Date");
            dataTable.Columns.Add("Pay Type");
            dataTable.Columns.Add("Job Title");

            _iUpload.Setup(upload => upload.CheckFileExtension(file.Object)).ReturnsAsync(dataTable);
            _iUpload.Setup(upload => upload.ImportEmployee(dataTable, It.IsAny<int>()))
                .ReturnsAsync(new ImportFileResponse { StatusCode = 402 });

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.SaveEmployeeDetail(facilityId, file.Object);

            // Assert
            var response = Assert.IsType<JsonResult>(result);
            var importResponse = Assert.IsType<ImportFileResponse>(response.Value);
            Assert.Equal(402, importResponse.StatusCode);
        }

        [Fact]
        public async Task SaveEmployeeDetail_ValidFile_ShouldReturnWrongFileUploaded()
        {
            // Arrange
            var facilityId = "1";
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.txt");
            var dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id");
            dataTable.Columns.Add("First Name");
            dataTable.Columns.Add("Last Name");
            dataTable.Columns.Add("Hire Date");
            dataTable.Columns.Add("Termination Date");
            dataTable.Columns.Add("Pay Type");
            dataTable.Columns.Add("Job Title");

            _iUpload.Setup(upload => upload.CheckFileExtension(file.Object)).ReturnsAsync(dataTable);
            _iUpload.Setup(upload => upload.ImportEmployee(dataTable, It.IsAny<int>()))
                .ReturnsAsync(new ImportFileResponse { StatusCode = 401 });

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.SaveEmployeeDetail(facilityId, file.Object);

            // Assert
            var response = Assert.IsType<JsonResult>(result);
            var importResponse = Assert.IsType<ImportFileResponse>(response.Value);
            Assert.Equal(401, importResponse.StatusCode);
        }

        [Fact]
        public async Task SaveEmployeeDetail_ValidFileWithData_ShouldReturnSuccessResponse()
        {
            // Arrange
            var facilityId = "1";

            // Create a sample data table with at least one row of data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id");
            dataTable.Columns.Add("First Name");
            dataTable.Columns.Add("Last Name");
            dataTable.Columns.Add("Hire Date");
            dataTable.Columns.Add("Termination Date");
            dataTable.Columns.Add("Pay Type");
            dataTable.Columns.Add("Job Title");

            // Add a sample row of data to the table
            var sampleDataRow = dataTable.NewRow();
            sampleDataRow["Employee Id"] = "123";
            sampleDataRow["First Name"] = "John";
            sampleDataRow["Last Name"] = "Doe";
            sampleDataRow["Hire Date"] = "2023-01-01";
            sampleDataRow["Termination Date"] = DBNull.Value; // If no termination date
            sampleDataRow["Pay Type"] = "Salary";
            sampleDataRow["Job Title"] = "Software Engineer";
            dataTable.Rows.Add(sampleDataRow);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.csv");

            _iUpload.Setup(upload => upload.CheckFileExtension(file.Object)).ReturnsAsync(dataTable);

            // Mock ImportEmployee to handle the sample data table
            _iUpload.Setup(upload => upload.ImportEmployee(dataTable, It.IsAny<int>()))
                .ReturnsAsync(new ImportFileResponse { StatusCode = 402 });

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.SaveEmployeeDetail(facilityId, file.Object);

            // Assert
            var response = Assert.IsType<JsonResult>(result);
            var importResponse = Assert.IsType<ImportFileResponse>(response.Value);
            Assert.Equal(402, importResponse.StatusCode);
        }

        [Fact]
        public async Task SaveEmployeeDetail_InValidFileWithData_ShouldReturn403()
        {
            // Arrange
            var facilityId = "1";

            // Create a sample data table with at least one row of data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id");
            dataTable.Columns.Add("First Name");
            dataTable.Columns.Add("Last Name");
            dataTable.Columns.Add("Hire Date");
            dataTable.Columns.Add("Termination Date");
            dataTable.Columns.Add("PayType");
            dataTable.Columns.Add("Job Title");

            // Add a sample row of data to the table
            var sampleDataRow = dataTable.NewRow();
            sampleDataRow["Employee Id"] = "123";
            sampleDataRow["First Name"] = "John";
            sampleDataRow["Last Name"] = "Doe";
            sampleDataRow["Hire Date"] = "2023-01-01";
            sampleDataRow["Termination Date"] = DBNull.Value; // If no termination date
            sampleDataRow["PayType"] = "Salary";
            sampleDataRow["Job Title"] = "Software Engineer";
            dataTable.Rows.Add(sampleDataRow);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.csv");

            _iUpload.Setup(upload => upload.CheckFileExtension(file.Object)).ReturnsAsync(dataTable);

            // Mock ImportEmployee to handle the sample data table
            _iUpload.Setup(upload => upload.ImportEmployee(dataTable, It.IsAny<int>()))
                .ReturnsAsync(new ImportFileResponse { StatusCode = 403 });

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.SaveEmployeeDetail(facilityId, file.Object);

            // Assert
            var response = Assert.IsType<JsonResult>(result);
            var importResponse = Assert.IsType<ImportFileResponse>(response.Value);
            Assert.Equal(403, importResponse.StatusCode);
            Assert.Equal("You have uploaded wrong format file.", importResponse.Message);
        }

        [Fact]
        public async Task SaveEmployeeDetail_InValidFileWithData_ThrowsException()
        {
            // Arrange
            var facilityId = "1";

            // Create a sample data table with at least one row of data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Employee Id");
            dataTable.Columns.Add("First Name");
            dataTable.Columns.Add("Last Name");
            dataTable.Columns.Add("Hire Date");
            dataTable.Columns.Add("Termination Date");
            dataTable.Columns.Add("Pay Type");
            dataTable.Columns.Add("Job Title");

            // Add a sample row of data to the table
            var sampleDataRow = dataTable.NewRow();
            sampleDataRow["Employee Id"] = "123";
            sampleDataRow["First Name"] = "John";
            sampleDataRow["Last Name"] = "Doe";
            sampleDataRow["Hire Date"] = "2023-01-01";
            sampleDataRow["Termination Date"] = DBNull.Value; // If no termination date
            sampleDataRow["Pay Type"] = "Salary";
            sampleDataRow["Job Title"] = "Software Engineer";
            dataTable.Rows.Add(sampleDataRow);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.csv");

            _iUpload.Setup(upload => upload.CheckFileExtension(file.Object)).Throws(new Exception("An error occured while checking extension"));

            // Mock ImportEmployee to handle the sample data table
            _iUpload.Setup(upload => upload.ImportEmployee(dataTable, It.IsAny<int>()))
                .ReturnsAsync(new ImportFileResponse { StatusCode = 400 });

            var controller = new EmployeeMasterController(_unitOfWorkMock.Object, _userManagerMock.Object, _iUpload.Object);

            // Act
            var result = await controller.SaveEmployeeDetail(facilityId, file.Object);

            // Assert
            var response = Assert.IsType<JsonResult>(result);
            var importResponse = Assert.IsType<ImportFileResponse>(response.Value);
            Assert.Equal(400, importResponse.StatusCode);
            Assert.Equal("Failed to import", importResponse.Message);
        }
    }
}
