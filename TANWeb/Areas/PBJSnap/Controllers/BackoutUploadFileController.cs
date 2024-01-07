using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Helpers.Permissions;
using static TAN.DomainModels.Models.UploadModel;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [Authorize]
    public class BackoutUploadFileController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        public readonly IUpload _iupload;
        public BackoutUploadFileController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IUpload iupload)
        {
            _uow = uow;
            _UserManager = UserManager;
            _iupload = iupload;
        }
        [Route("backout")]
        public async Task<IActionResult> Backout()
        {
            BackoutModal backoutModal = new BackoutModal();
            try
            {
                backoutModal.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Backout Backout :{ErrorMsg}", ex.Message);
            }
            return View(backoutModal);
        }

        [HttpPost]
        public IActionResult GetUploadFileDetails(BackoutTableRequest request)
        {
            List<FileDetailsResponse> FileDetailsList = new List<FileDetailsResponse>();
            int BackoutTotalCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                request.Search = Request.Form["search[value]"].FirstOrDefault();
                int sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"].FirstOrDefault());
                string sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"];
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumnName + "_" + sortDirection).ToLower();
                }
                FileDetailsList = _uow.UploadFileDetailsRepo.GetFileRecord(request, pageSize, Convert.ToInt32(start), out BackoutTotalCount, sortOrder);
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = BackoutTotalCount,
                    recordsFiltered = BackoutTotalCount,
                    data = FileDetailsList
                };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout GetUploadFileDetails :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = BackoutTotalCount,
                recordsFiltered = BackoutTotalCount,
                data = FileDetailsList
            };
            return Json(_returnObj);
        }

        [Authorize(Permissions.Backout.PBJSnapDelete)]
        [HttpPost]
        public async Task<IActionResult> BackOutAllData(BackOutAllRecordRequest request)
        {
            BackOutAllRecordResponse response = new BackOutAllRecordResponse();
            try
            {
                response = await _uow.UploadFileDetailsRepo.BackOutAllRecord(request);
                return Json(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout BackOutAllData :{ErrorMsg}", ex.Message);
            }
            response.status = false;
            response.Message = "Failed to Delete";
            return Json(response);
        }

        [Authorize(Permissions.Backout.PBJSnapViewInvalidRecord)]
        [HttpPost]
        public async Task<IActionResult> GetFileErrorData(BackOutAllRecordRequest request)
        {
            List<FileErrorList> response = new List<FileErrorList>();
            int ErrorDataTotalCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    sortOrder = (sortColumn + "_" + sortColumnDirection).ToLower();
                }
                response = _uow.UploadFileDetailsRepo.GetFileErrorList(request, pageSize, Convert.ToInt32(start), out ErrorDataTotalCount, sortOrder);
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = ErrorDataTotalCount,
                    recordsFiltered = ErrorDataTotalCount,
                    data = response
                };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout GetFileErrorData :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = ErrorDataTotalCount,
                recordsFiltered = ErrorDataTotalCount,
                data = response
            };
            return Json(_returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetErrorRowData(string Id)
        {
            ErrorData errorData = new ErrorData();
            try
            {
                errorData = await _uow.UploadFileDetailsRepo.GetErrorRawData(Convert.ToInt32(Id));
                return Json(errorData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout GetUploadFileDetails :{ErrorMsg}", ex.Message);
            }
            return Json(errorData);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateErrorRowData(ErrorData errorData)
        {
            try
            {
                if (errorData != null)
                {
                    ValidatDataResponse dataResponse = await _iupload.ReValidatedWorkingHours(errorData);
                    if (dataResponse != null)
                    {
                        if (dataResponse.Success == true)
                        {
                            bool result = await _uow.FileErrorRecordsRepo.UpdateErrorValidationRecords(errorData.Id);
                            if (result)
                            {
                                return Json(dataResponse);
                            }
                            else
                            {
                                dataResponse.ErrorMessage = "The record is validated and added. Kindly delete the error record manually.";
                            }
                        }
                    }
                    return Json(dataResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout UpdateErrorRowData :{ErrorMsg}", ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteErrorRowData(string Id)
        {
            Response response = new Response();
            try
            {
                if (await _uow.UploadFileDetailsRepo.DeleteErrorRawData(Convert.ToInt32(Id)))
                {
                    response.StatusCode = 200;
                    response.Message = "Record deleted successfully";
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout DeleteErrorRowData :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Failed to delete the record";
            return Json(response);
        }

        [Authorize(Permissions.Backout.PBJSnapDeleteInvalidRecord)]
        public async Task<IActionResult> DeleteInvalidRecord(string FileDetailId)
        {
            Response response = new Response();
            try
            {
                if (await _uow.UploadFileDetailsRepo.DeleteInvalidRecord(Convert.ToInt32(FileDetailId)))
                {
                    response.StatusCode = 200;
                    response.Message = "Invalid record deleted successfully";
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout DeleteInvalidRecord :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = 400;
            response.Message = "Failed to delete invalid record";
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> ListValidationErrorByFileId(ValidationErrorListRequest request)
        {
            ValidationErrorListResponse response = new ValidationErrorListResponse();
            try
            {
                int totalCount = 0;
                response.dynamicDataTableModel = _uow.UploadFileDetailsRepo.ListValidationByFileId(request, out totalCount);
                response.TotalCount = totalCount;
                response.RowCount = 0;
                response.uploadFileDetails = await _uow.UploadFileDetailsRepo.GetUploadFileDetailsByFileId(request.FileDetailId);
                if (response.dynamicDataTableModel.Count > 0)
                {
                    response.RowCount = ((request.PageNo - 1) * request.PageSize) + response.dynamicDataTableModel.Count;
                    response.RowStart = ((request.PageNo - 1) * request.PageSize) + 1;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout ListValidationByFileId :{ErrorMsg}", ex.Message);
            }
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllFileNameByFacility(FileDetailListRequest request)
        {
            List<FileDetailList> model = new List<FileDetailList>();
            try
            {
                model = await _uow.UploadFileDetailsRepo.GetAllFileDetails(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout ListValidationByFileId :{ErrorMsg}", ex.Message);
            }
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetFileValidData(BackOutAllRecordRequest request)
        {
            List<ValidDataViewModel> response = new List<ValidDataViewModel>();
            int validDataTotalCount = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            string sortOrder = string.Empty;
            try
            {
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                int pageSize = Convert.ToInt32(length ?? "0");
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    sortOrder = (sortColumn + "_" + sortColumnDirection).ToLower();
                }
                response = _uow.UploadFileDetailsRepo.GetFileValidRecordList(request, pageSize, Convert.ToInt32(start), out validDataTotalCount, sortOrder);
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = validDataTotalCount,
                    recordsFiltered = validDataTotalCount,
                    data = response
                };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout GetFileValidData :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = validDataTotalCount,
                recordsFiltered = validDataTotalCount,
                data = response
            };
            return Json(_returnObj);
        }

        public FileResult ExportInvalidFileErrorDataToCsv(string facilityId, string year, string reportQuarter, string fileName, string fileDetailsId)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable errorFileDetails = _uow.UploadFileDetailsRepo.GetInvalidErrorFileDataForCsv(Convert.ToInt32(facilityId), Convert.ToInt32(reportQuarter), Convert.ToInt32(year), Convert.ToInt32(fileDetailsId));
                if (errorFileDetails.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = errorFileDetails.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in errorFileDetails.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = fileName;
                    errorFileDetails.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportInvalidFileErrorDataToCsv :{ErrorMsg}", ex.Message);
            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", fileName + ".csv");
        }


        public FileResult ExportValidFileDataToCsv(string facilityId, string fileName, string fileDetailsId)
        {
            try
            {
                StringBuilder csv = new StringBuilder();
                DataTable validFileDetails = _uow.UploadFileDetailsRepo.GetValidFileDataForCsv(Convert.ToInt32(facilityId), Convert.ToInt32(fileDetailsId));
                if (validFileDetails.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = validFileDetails.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in validFileDetails.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = fileName;
                    validFileDetails.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportValidFileDataToCsv :{ErrorMsg}", ex.Message);
            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", fileName + ".csv");
        }

        public async Task<IActionResult> GetFileNameByFacility(FileDetailListRequest request)
        {
            FileDetailList fileDetail = new FileDetailList();
            try
            {
                fileDetail = await _uow.UploadFileDetailsRepo.GetFileDetails(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout ListValidationByFileId :{ErrorMsg}", ex.Message);
            }
            return Json(fileDetail);
        }

        public FileResult ExportInvalidFileErrorDynamicDataToCsv(string facilityId, string year, string reportQuarter, string fileName, string fileDetailsId)
        {

            ValidationErrorListResponse response = new ValidationErrorListResponse();
            try
            {
                int totalCount = 0;
                DataTable dynamicDataTableMode = _uow.UploadFileDetailsRepo.GetInvalidErrorFileDynamicDataForCsv(Convert.ToInt32(fileDetailsId));
                StringBuilder csv = new StringBuilder();
                if (dynamicDataTableMode.Rows.Count > 0)
                {

                    IEnumerable<string> columnNames = dynamicDataTableMode.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                    csv.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in dynamicDataTableMode.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        csv.AppendLine(string.Join(",", fields));
                    }
                    string filename = fileName;
                    dynamicDataTableMode.Clear();
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ExportInvalidFileErrorDataToCsv :{ErrorMsg}", ex.Message);
            }
            return File(Encoding.UTF8.GetBytes(""), "text/csv", fileName + ".csv");
        }

        public IActionResult GetFacilityName(string facilityId)
        {
            try
            {
                var facilityName = _uow.UploadFileDetailsRepo.GetFacilityName(Convert.ToInt32(facilityId));
                return Json(facilityName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Backout Controller / Getting Facility Name :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }

    }
}
