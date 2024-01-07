using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Serilog;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Security.Policy;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.Repository.Abstractions;
using TANWeb.Interface;
using TANWeb.Services;
using static TAN.DomainModels.Models.UploadModel;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [Authorize]
    public class UploadController : Controller
    {
        public readonly IUpload _iupload;
        private readonly IUnitOfWork _uow;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        private readonly IDataProtector _protector;
        public UploadController(IUpload iupload, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IUnitOfWork uow, IDataProtectionProvider dataProvider)
        {
            _iupload = iupload;
            _UserManager = UserManager;
            _uow = uow;
            _protector = dataProvider.CreateProtector("Code");
        }

        [Route("/get-started/upload")]
        public async Task<IActionResult> Upload()
        {
            UploadView uploadView = new UploadView();
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var _user = _UserManager.FindByEmailAsync(userEmail).Result;
                uploadView.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in UploadController Upload :{ErrorMsg}", ex.Message);
            }
            return View(uploadView);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(RequestModel request, IFormFile file)
        {
            ResponseUploadModel responseUploadModel = new ResponseUploadModel();
            IUpload.uploadFileDetails = new UploadFileDetailsWithFilter();

            try
            {
                bool isOneDrive = request.IsOneDrive;
                string fileName = isOneDrive ? request.Filename.ToString() : Path.GetFileName(file.FileName);

                bool isFileNameValid = await _iupload.CheckFileName(fileName, request.FacilityId.ToString());
                if (isFileNameValid)
                {
                    responseUploadModel.StatusCode = (int)UploadFileFailureSuccess.FileExist;
                    responseUploadModel.Message = "The Filename is already exist.";
                    return StatusCode(400, responseUploadModel);
                }

                IUpload.uploadFileDetails.FileName = fileName;

                IUpload.uploadFileDetails.FileDataTable = null;
                Stream fileStream = null;

                if (isOneDrive)
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(request.FileURL);
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    fileStream = (Stream)resp.GetResponseStream();
                    IUpload.uploadFileDetails.FileDataTable = await _iupload.CheckOneDriveFileExtension(fileStream, fileName);
                }
                else if (file != null)
                {
                    fileStream = file.OpenReadStream();
                    IUpload.uploadFileDetails.FileDataTable = await _iupload.CheckFileExtension(file);
                }

                if (IUpload.uploadFileDetails.FileDataTable == null || IUpload.uploadFileDetails.FileDataTable.Rows.Count == 0)
                {
                    responseUploadModel.StatusCode = (int)UploadFileFailureSuccess.FileExtension;
                    responseUploadModel.Message = "File data is empty or file extension may be invalid!";
                    return StatusCode(400, responseUploadModel);
                }

                IUpload.uploadFileDetails.FacilityId = request.FacilityId;
                IUpload.uploadFileDetails.Month = request.Month;
                IUpload.uploadFileDetails.Year = request.year;
                IUpload.uploadFileDetails.Quarter = request.Quarter;
                IUpload.uploadFileDetails.UploadType = Convert.ToInt32(request.UploadType);
                IUpload.uploadFileDetails.DtCmsList = await _iupload.GetAllCMSFieldData();
                IUpload.uploadFileDetails.PreviewTableData = await _iupload.PreviewData(IUpload.uploadFileDetails.FileDataTable, request.FacilityId.ToString());

                if (IUpload.uploadFileDetails.PreviewTableData != null && IUpload.uploadFileDetails.DtCmsList != null)
                {
                    responseUploadModel.StatusCode = (int)UploadFileFailureSuccess.Success;
                    responseUploadModel.previewTable = IUpload.uploadFileDetails.PreviewTableData;
                    responseUploadModel.CmsMappingFieldDataList = IUpload.uploadFileDetails.DtCmsList;
                    responseUploadModel.UploadFileData = IUpload.uploadFileDetails.FileDataTable;
                    return PartialView(responseUploadModel);
                }

                responseUploadModel.StatusCode = (int)UploadFileFailureSuccess.Server;
                responseUploadModel.Message = "Failed to Upload";
                return StatusCode((int)UploadFileFailureSuccess.Server, responseUploadModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in UploadFile: {ErrorMsg}", ex.Message);
                responseUploadModel.StatusCode = (int)UploadFileFailureSuccess.Server;
                responseUploadModel.Message = "Something went wrong";
            }

            return StatusCode((int)UploadFileFailureSuccess.Server, responseUploadModel);
        }

        [HttpPost]
        public async Task<IActionResult> MappingData(MappingRequestData mappingRequestData)
        {
            ResponseMapping responseUploadModel = new ResponseMapping();
            IUpload.uploadFileDetails.DiffQuarterTimesheet = new List<Timesheet>();
            IUpload.uploadFileDetails.IsAllRecordInserted = false;
            try
            {
                if (await _iupload.HasDuplicateColumns(mappingRequestData))
                {
                    responseUploadModel.StatusCode = (int)MappingDataStatus.MultipleCMS;
                    responseUploadModel.Message = "Multiple CMS Field Selected";
                    return StatusCode((int)MappingDataStatus.MultipleCMS, responseUploadModel);
                }
                List<string> ValidatedCmsColumn = await _iupload.HasPresentRequiredCmsColumn(mappingRequestData.MappingDatas);
                if (ValidatedCmsColumn.Count > 0)
                {
                    responseUploadModel.StatusCode = (int)MappingDataStatus.MultipleCMS;
                    responseUploadModel.Message = "Please add these many CMS field " + string.Join(",", ValidatedCmsColumn);
                    return StatusCode((int)MappingDataStatus.MultipleCMS, responseUploadModel);
                }
                IUpload.uploadFileDetails.FileNameId = await _iupload.AddFilenName(IUpload.uploadFileDetails.FileName, IUpload.uploadFileDetails.FacilityId.ToString(), IUpload.uploadFileDetails.FileDataTable.Rows.Count, mappingRequestData.requestModel);
                if (IUpload.uploadFileDetails.FileNameId == 0)
                {
                    responseUploadModel.StatusCode = (int)MappingDataStatus.FileUploadFailed;
                    responseUploadModel.Message = "File upload failed!";
                    return StatusCode((int)MappingDataStatus.FileUploadFailed, responseUploadModel);
                }
                await _iupload.AddUpdateMappingField(mappingRequestData);
                DataTable fileDataTable = await _iupload.UpdateColumnName(mappingRequestData, IUpload.uploadFileDetails.FileDataTable);
                if (fileDataTable.Rows.Count == 0 || fileDataTable == null)
                {
                    responseUploadModel.StatusCode = (int)MappingDataStatus.MapProperly;
                    responseUploadModel.Message = "Please Map properly";
                    return StatusCode((int)MappingDataStatus.MapProperly, responseUploadModel);
                }
                if (IUpload.uploadFileDetails.JobCode == null)
                {
                    IUpload.uploadFileDetails.JobCode = _uow.JobCodesRepo.GetAll().Where(x => x.IsActive).ToList();
                }
                if (IUpload.uploadFileDetails.PayTypeCode == null)
                {
                    IUpload.uploadFileDetails.PayTypeCode = _uow.PayTypeCodesRepo.GetAll().Where(x => x.IsActive).ToList();
                }
                IUpload.uploadFileDetails.ValidEmployeeList = new List<Employee>();
                IUpload.uploadFileDetails.ValidEmployeeList = await _iupload.CheckEmployee(IUpload.uploadFileDetails.FileDataTable, IUpload.uploadFileDetails.FacilityId);
                mappingRequestData.requestModel.Filename = IUpload.uploadFileDetails.FileName;
                mappingRequestData.requestModel.FileNameId = IUpload.uploadFileDetails.FileNameId;
                await _iupload.BusinessLogic(mappingRequestData.requestModel, fileDataTable);
                if (IUpload.uploadFileDetails.DiffQuarterTimesheet != null)
                {
                    if (IUpload.uploadFileDetails.DiffQuarterTimesheet.Count > 0)
                    {
                        responseUploadModel.StatusCode = (int)MappingDataStatus.DiffQuarter;
                        responseUploadModel.Message = "Prior quarter record(s) are contained in the uploaded file. Do you want to keep uploading the record(s)?";
                        responseUploadModel.DiffQuarterTimesheet = IUpload.uploadFileDetails.DiffQuarterTimesheet;
                        return Json(responseUploadModel);
                    }
                }
                if (IUpload.uploadFileDetails.IsAllRecordInserted)
                {
                    responseUploadModel.StatusCode = (int)MappingDataStatus.InvalidRecord;
                    responseUploadModel.Message = "The uploaded file has validation errors. To review and address these errors, click the \"Confirm\" button, and you'll find the validation list under the \"Review\" step.";
                    responseUploadModel.URL =  Url.Action("Index", "Review", new { 
                        facilityId = Convert.ToString(_protector.Protect(IUpload.uploadFileDetails.FacilityId.ToString())), 
                        fileId = Convert.ToString(_protector.Protect(IUpload.uploadFileDetails.FileNameId.ToString()))
                    }); 
                    return Json(responseUploadModel);
                }
                responseUploadModel.StatusCode = (int)MappingDataStatus.Success;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Mapping Data :{ErrorMsg}", ex.Message);
                responseUploadModel.Message = "Server failed. Please try again!";
                responseUploadModel.StatusCode = (int)MappingDataStatus.Server;
                return StatusCode((int)MappingDataStatus.Server, responseUploadModel);
            }
            return StatusCode((int)MappingDataStatus.Success, responseUploadModel);
        }

        [HttpGet]
        public async Task<IActionResult> InsertDifferntQuarterData()
        {
            ResponseMapping responseUploadModel = new ResponseMapping();
            try
            {
                if (IUpload.uploadFileDetails.DiffQuarterTimesheet != null)
                {
                    if (IUpload.uploadFileDetails.DiffQuarterTimesheet.Count > 0)
                    {
                        await _uow.TimesheetRepo.InsertTimesheet(IUpload.uploadFileDetails.DiffQuarterTimesheet);
                        if (IUpload.uploadFileDetails.IsAllRecordInserted)
                        {
                            responseUploadModel.StatusCode = (int)MappingDataStatus.InvalidRecord;
                            responseUploadModel.Message = "The uploaded file has validation errors. To review and address these errors, click the \"Confirm\" button, and you'll find the validation list under the \"Review\" step.";
                            responseUploadModel.URL = Url.Action("Index", "Review", new
                            {
                                facilityId = Convert.ToString(_protector.Protect(IUpload.uploadFileDetails.FacilityId.ToString())),
                                fileId = Convert.ToString(_protector.Protect(IUpload.uploadFileDetails.FileNameId.ToString()))
                            });
                            return Json(responseUploadModel);
                        }
                        responseUploadModel.StatusCode= (int)MappingDataStatus.Success;
                        responseUploadModel.Message = "Record uploaded successfully";
                        return Json(responseUploadModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured " + ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + " :{ErrorMsg}", ex.Message);
                return NotFound();
            }
            return NotFound();
        }
    }
}