using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;
using System.Linq;
using System.Security.Claims;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;
using TANWeb.Interface;
using static TAN.DomainModels.Helpers.Permissions;
using static TAN.DomainModels.Models.UploadModel;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [Authorize]
    public class EmployeeMasterController : Controller
    {
        private readonly IUnitOfWork _uow;
        public readonly IUpload _iupload;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        public EmployeeMasterController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IUpload iupload)
        {
            _uow = uow;
            _UserManager = UserManager;
            _iupload = iupload;
        }

        [Route("employees")]
        public async Task<IActionResult> Index()
        {
            EmployeeViewModel employee = new EmployeeViewModel();
            employee.EmployeeDetailModel = new EmployeeModel();
            employee.EmployeeDetailModel.employeePopupModel = new EmployeePopupModel();
            try
            {
                employee.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
                employee.EmployeeDetailModel.employeePopupModel.FacilityList = employee.FacilityList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured EmployeeMasterController Index :{ErrorMsg}", ex.Message);
            }
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeList(string facilityId, string searchValue, string start, string length, string sortColumn, string sortDirection, int draw)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            int EmployeeCount = 0;
            //var draw = Request.Form["draw"].FirstOrDefault();
            try
            {
                //var start = Request.Form["start"].FirstOrDefault();
                //var length = Request.Form["length"].FirstOrDefault();
                //var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                int skip = Convert.ToInt32(start ?? "0");
                string sortOrder = string.Empty;
                //int sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                //string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                //string sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"];
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumn + "_" + sortDirection).ToLower();
                }
                employeeModel.EmployeeDataList = _uow.EmployeeRepo.GetAllEmployeesData(pageSize, skip, facilityId, out EmployeeCount, searchValue, sortOrder);
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = EmployeeCount,
                    recordsFiltered = EmployeeCount,
                    data = employeeModel.EmployeeDataList.ToList()
                };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in EmployeeList:{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = EmployeeCount,
                recordsFiltered = EmployeeCount,
                data = employeeModel.EmployeeDataList.ToList()
            };
            return Json(_returnObj);
        }
        [Authorize(Permissions.EmployeeMaster.PBJSnapCreate)]
        [HttpPost]
        public async Task<IActionResult> SaveEmployeeDetail(string FacilityId, IFormFile file)
        {
            ImportFileResponse response = new ImportFileResponse();
            try
            {
                if (file != null && !string.IsNullOrEmpty(FacilityId))
                {
                    string extension = Path.GetExtension(file.FileName);
                    if (extension != ".csv")
                    {
                        response.StatusCode = 401;
                        response.Message = "You have uploaded wrong file. Please upload csv file only";
                        return Json(response);
                    }
                    DataTable dataTable = await _iupload.CheckFileExtension(file);
                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        List<string> desiredStrings = new List<string>()
        {
            "Employee Id","First Name","Last Name","Hire Date","Termination Date","Pay Type","Job Title"
        };
                        bool flag = false;
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            if (!desiredStrings.Exists(s => s.ToLower().Contains(column.ColumnName.ToLower())))
                            {
                                flag = true;
                                response.StatusCode = 403;
                                response.Message = "You have uploaded wrong format file.";
                                return Json(response);
                            }
                        }
                        if (flag)
                        {
                            response.StatusCode = 403;
                            response.Message = "You have uploaded wrong format file.";
                            return Json(response);
                        }

                        response = await _iupload.ImportEmployee(dataTable, Convert.ToInt32(FacilityId));
                        return Json(response);
                    }
                    else
                    {
                        response.StatusCode = 402;
                        response.Message = "You have uploaded empty file.";
                        return Json(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in SaveEmployeeDetail:{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Failed to import";
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeById(GetEmployeeDetailsRequest request)
        {
            Employee employee = new Employee();
            try
            {
                employee = await _uow.EmployeeRepo.GetEmployeeById(request);
                return Json(employee);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetEmployeeById:{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }
    }
}
