using Azure.Core.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TAN.DomainModels.Models;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using TANWeb.Services;
using Microsoft.AspNetCore.RateLimiting;
using static TAN.DomainModels.Helpers.Permissions;
using DataService = TAN.DomainModels.Entities.DataService;

namespace TANWeb.Areas.TelecomReporting.Controllers
{
    [Area("TelecomReporting")]
    [Authorize]
    public class DataServiceController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StorageService _storageService;
        public DataServiceController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _uow = uow;
            _userManager = UserManager;
            _configuration = configuration;
            string connectionString = _configuration.GetConnectionString("blobConn").ToString();
            _storageService = new StorageService(connectionString);
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Permissions.DataService.TelecomDataServiceView)]
        [Route("telecomreporting/dataservice")]
        public async Task<IActionResult> Index()
        {
            DataServiceModel dataService = new DataServiceModel();
            try
            {
                dataService.facilityLocationAccesses = await _uow.FacilityRepo.GetAllFacilitiesLocationIdByOrgId();
                dataService.addDataService.facilityLocationAccesses = dataService.facilityLocationAccesses;
                dataService.ServiceList = await ServiceList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Index Method :{ErrorMsg}", ex.Message);
            }
            return View(dataService);
        }


        [HttpPost]
        public async Task<IActionResult> GetDataServiceList(DataServiceListRequest serviceListRequest, string searchValue, string start, string length, string sortColumn, string sortDirection, int draw)
        {
            List<FacilityDataServiceModel> list = new List<FacilityDataServiceModel>();
            int TotalCount = 0;
            string sortOrder = "";
            try
            {
                if (serviceListRequest != null)
                {
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                    {
                        sortOrder = (sortColumn + "_" + sortDirection).ToLower();
                    }
                    GetDataServiceListRequest request = new GetDataServiceListRequest()
                    {
                        facilityId = serviceListRequest.facilityId,
                        skip = Convert.ToInt32(start ?? "0"),
                        PageSize = Convert.ToInt32(length ?? "0"),
                        search = searchValue,
                        sortOrder = sortOrder,
                        serviceId = serviceListRequest.serviceId
                    };
                    list = _uow.DataServiceRepo.GetDataServiceList(request, out TotalCount);
                    var returnObj = new
                    {
                        draw = draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = list
                    };
                    return Json(returnObj);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured " + ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + " :{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = TotalCount,
                recordsFiltered = TotalCount,
                data = list
            };
            return Json(_returnObj);
        }

        public async Task<IActionResult> DataService()
        {
            try
            {
                //jdisajdsadasd
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured " + ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + " :{ErrorMsg}", ex.Message);
            }
            return View();
        }

        [Route("telecomreporting/dataservice/uploadfile")]
        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileView)]
        public async Task<IActionResult> UploadFile()
        {
            UploadClientFileDetailsViewModel uploadClientFileDetails = new();
            var allOrganizations = _uow.OrganizationRepo.GetAll().Where(o => o.IsActive).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();
            var dataServiceOrganizations = _uow.OrganizationServiceRepo.GetAll().Where(o => o.IsActive && o.ServiceType == Convert.ToInt32(ServiceType.Data)).ToList();
            var filteredOrganizations = allOrganizations.Where(org => dataServiceOrganizations.Any(dsOrg => dsOrg.OrganizationId == org.Value)).ToList();
            uploadClientFileDetails.OrganizationList = filteredOrganizations;
            var organizationIds = filteredOrganizations.Select(org => org.Value).ToList();
            var filteredFacilities = _uow.FacilityRepo.GetAll().Where(f => organizationIds.Contains(f.OrganizationId)).Select(f => new Itemlist { Text = f.FacilityName, Value = f.Id }).ToList();
            uploadClientFileDetails.FacilityList = filteredFacilities;
            uploadClientFileDetails.CarrierList = _uow.CarrierInformationRepo.GetAll().Where(c => c.IsActive).Select(c => new Itemlist { Text = c.CarrierName, Value = c.Id }).ToList();

            bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
            if (!isThinkAnewUser)
            {
                var organizationInfoDetails = _uow.FacilityRepo.GetOrganizationUserDetails();
                if (organizationInfoDetails != null)
                {
                    uploadClientFileDetails.OrganizationList = filteredOrganizations.Where(dso => dso.Value == Convert.ToInt32(organizationInfoDetails)).ToList();
                    uploadClientFileDetails.FacilityList = _uow.FacilityRepo.GetAll().Where(f => f.OrganizationId == Convert.ToInt32(organizationInfoDetails)).Select(f => new Itemlist { Text = f.FacilityName, Value = f.Id }).ToList();
                }
            }
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            uploadClientFileDetails.UserType = currentUser.UserType;

            return View(uploadClientFileDetails);
        }

        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileUpload)]
        [EnableRateLimiting("AddPolicy")]
        public async Task<IActionResult> UploadFileToAzureBlobStorage(UploadClientFileRequestViewModel request)
        {
            var containerName = _configuration.GetValue<string>("AzureBlobStorageContainerName:ContainerName");
            string fileName;
            Stream stream;
            //var maxFileSizeInBytes = 100 * 1024 * 1024; // 100 MB in bytes

            try
            {
                if (request.IsOneDrive)
                {
                    using (var httpClient = _httpClientFactory.CreateClient())
                    {
                        var response = await httpClient.GetAsync(request.FileURL);

                        if (!response.IsSuccessStatusCode)
                        {
                            return Json("error");
                        }

                        stream = await response.Content.ReadAsStreamAsync();
                        fileName = request.FileName;
                        request.FileSize = stream.Length;
                    }
                }
                else
                {
                    if (request.UploadedFile == null || request.UploadedFile.Length == 0)
                    {
                        // Handle invalid or empty file
                        return BadRequest();
                    }

                    stream = request.UploadedFile.OpenReadStream();
                    fileName = request.UploadedFile.FileName;
                    request.FileSize = request.UploadedFile.Length;
                }

                // Check the file extension
                var allowedExtensions = new[] { ".pdf", ".jpeg", ".png", ".jpg" };
                var fileExtension = Path.GetExtension(fileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json("invalidextension");
                }
                else
                {
                    request.Result = await _storageService.UploadFileAsync(containerName, fileName, stream);
                    if (request.Result == "success")
                    {
                        var responseFromDB = await _uow.UploadClientFileDetailsRepo.UploadFileDetailsToDB(request);
                        return Json(responseFromDB);
                    }
                    if (request.Result == "exists")
                    {
                        return Json("exists");
                    }

                    if (request.Result != "exists" && request.Result != "success" && request.Result == "failed")
                    {
                        return Json("error");
                    }
                    return Json(request.Result);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Upload File To Azure Blob Storage and Database :{ErrorMsg}", ex.Message);
                return Json("Failed");
            }
        }

        public JsonResult GetOrganizationFaciltyList(string? orgId)
        {
            try
            {
                var facilityList = _uow.FacilityRepo.GetAll().Where(c => c.OrganizationId == Convert.ToInt32(orgId) && c.IsActive).Select(p => new Itemlist { Text = p.FacilityName, Value = p.Id }).ToList();
                return Json(facilityList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Facility List :{ErrorMsg}", ex.Message);
                return Json(new FacilityModel());
            }
        }

        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileView)]
        [HttpPost]
        public JsonResult GetAllUploadedFileList(string searchValue, string start, string length, string sortColumn, string sortDirection, int draw, string orgName, string facilityName, string carrierName, string orgId)
        {
            int totalRecord = 0;
            int filterRecord = 0;

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<UploadClientFileDetailsViewModel> data;

            int fileCount = 0;


            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.UploadClientFileDetailsRepo.GetAllUploadedFiles(skip, pageSize, searchValue, sortOrder, orgName, facilityName, carrierName, orgId, out fileCount);

            totalRecord = fileCount;

            // get total count of records after search 
            filterRecord = fileCount;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        public IActionResult GetLoggedInUserInfo()
        {
            try
            {
                bool isThinkAnewUser = _uow.FacilityRepo.GetLoggedInUserInfo();
                return Json(isThinkAnewUser);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting LoggedIn User Information :{ErrorMsg}", ex.Message);

                return Json(false);
            }
        }

        public IActionResult GetOrganizationUserInfo()
        {
            try
            {
                var organizationId = _uow.FacilityRepo.GetOrganizationUserDetails();
                return Json(organizationId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Agency Controller / Getting Organization Details :{ErrorMsg}", ex.Message);
                return Json(null);
            }
        }

        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileDownload)]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var containerName = _configuration.GetValue<string>("AzureBlobStorageContainerName:ContainerName");
            var stream = await _storageService.GetFileAsync(containerName, fileName);

            if (stream != null)
            {
                return File(stream, "application/octet-stream", fileName);
            }
            else
            {
                // Handle the scenario when the file is not found
                // For example, return a specific error view or a JSON response
                return NotFound("File not found.");
            }
        }

        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileDelete)]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var containerName = _configuration.GetValue<string>("AzureBlobStorageContainerName:ContainerName");
            bool isDeleted = await _storageService.DeleteFileAsync(containerName, fileName);

            if (isDeleted)
            {
                var deleteFromDB = _uow.UploadClientFileDetailsRepo.DeleteFile(fileName);
                return Json(deleteFromDB);
            }
            else
            {
                // File or container not found
                return Json(isDeleted);
            }
        }

        [Authorize(Permissions.UploadClientFile.TelecomUploadClientFileFilePreview)]
        public async Task<IActionResult> DisplayFile(string fileName)
        {
            var containerName = _configuration.GetValue<string>("AzureBlobStorageContainerName:ContainerName");
            var stream = await _storageService.GetFileContentAsync(containerName, fileName);

            if (stream != null)
            {
                var contentType = GetContentType(fileName);

                return File(stream, contentType, fileName);
            }
            else
            {
                // Handle the scenario when the file is not found
                // For example, return a specific error view or a JSON response
                return NotFound("File not found.");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();

            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                default:
                    return "application/octet-stream";
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.DataService.TelecomDataServiceCreate)]
        public async Task<IActionResult> AddDataService(DataServiceRequest dataService, IFormFile file)
        {
            DataServiceResponse response = new DataServiceResponse();
            try
            {
                DataService data = new DataService();
                string result = "";
                if (dataService != null)
                {
                    if (file != null)
                    {
                        Stream stream;
                        if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                        {
                            response.StatusCode = (int)DataServiceStatusCode.INVALID_FILE;
                            response.Message = "The file is invalid!";
                            return Json(response);
                        }
                        CheckDataServiceFileName check = new CheckDataServiceFileName()
                        {
                            FacilityId = Convert.ToInt32(dataService.FacilityId),
                            filename = file.FileName,
                        };

                        if (await _uow.DataServiceRepo.CheckFileName(check))
                        {
                            response.StatusCode = (int)DataServiceStatusCode.FILE_EXIST;
                            response.Message = "The file is already exist!";
                            return Json(response);
                        }

                        data.ContractDocument = dataService.ProviderName + "_" + dataService.AccountNumber + "_" + file.FileName.ToString();
                        stream = file.OpenReadStream();
                        string container = _configuration.GetValue<string>("UploadDataServiceConfig:TempContainerName");
                        if (dataService.Id != "0")
                        {
                            bool isDeleted = await _storageService.DeleteFileAsync(container, data.ContractDocument);
                        }
                        result = await _storageService.UploadFileAsync(container, data.ContractDocument, stream);
                        if (result != "success")
                        {
                            response.StatusCode = (int)DataServiceStatusCode.UPLOAD_FILE_ERROR;
                            response.Message = "Upload failed!";
                            return Json(response);
                        }
                    }

                    if (await _uow.DataServiceRepo.CheckAccountNumber(new CheckAccountNumberRequest() { AccountNo = dataService.AccountNumber, FacilityId = Convert.ToInt32(dataService.FacilityId), DataServiceId = Convert.ToInt32(dataService.Id) }))
                    {
                        response.StatusCode = (int)DataServiceStatusCode.ACCOUNTNUMBER_EXIST;
                        response.Message = "The Account number is already exist!";
                        return Json(response);
                    }

                    data.Id = Convert.ToInt32(dataService.Id);
                    data.ProviderName = dataService.ProviderName;
                    data.AccountNumber = dataService.AccountNumber;
                    data.SupportNumber = dataService.SupportNumber;
                    data.LocalIP = dataService.LocalIP;
                    data.ContractEndDate = dataService.ContractEndDate;
                    data.ContractStatus = dataService.ContractStatus;
                    data.Createby = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    data.CreateDate = DateTime.Now;
                    data.IsActive = true;
                    data.ServicesId = dataService.ServicesId;
                    data.FacilityId = Convert.ToInt32(dataService.FacilityId);
                    if (await _uow.DataServiceRepo.AddDataService(data))
                    {
                        response.StatusCode = (int)DataServiceStatusCode.SUCCESS;
                        response.Message = "Record saved successfully.";
                        return Json(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured " + ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + " :{ErrorMsg}", ex.Message);
            }
            response.StatusCode = (int)DataServiceStatusCode.FAILED;
            response.Message = "Failed to saved!";
            return Json(response);
        }

        [Authorize(Permissions.DataService.TelecomDataServiceDelete)]
        public async Task<IActionResult> DeleteDataService(string Id)
        {
            try
            {
                if (Id != null && Id != "")
                {
                    if (await _uow.DataServiceRepo.DeleteDataService(Id))
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Deleting Data Service :{ErrorMsg}", ex.Message);
                return NotFound();
            }
            return NotFound();
        }

        [Authorize(Permissions.DataService.TelecomDataServiceEdit)]
        public async Task<IActionResult> GetDataServiceById(string Id)
        {
            DataServiceRequest response = new DataServiceRequest();
            try
            {
                if (Id != null && Id != "")
                {
                    response = await _uow.DataServiceRepo.GetDataServiceById(Id);
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Data Service By Id :{ErrorMsg}", ex.Message);
                return NotFound();
            }
            return Json(response);
        }

        [Route("telecomreporting/carriers")]
        [Authorize(Permissions.Carrier.TelecomCarrierView)]
        public IActionResult CarrierInfo()
        {
            return View();
        }

        [Authorize(Permissions.Carrier.TelecomCarrierCreate)]
        [EnableRateLimiting("AddPolicy")]
        public async Task<IActionResult> SaveCarrier(CarrierInfomationViewModel carrier)
        {
            if (ModelState.IsValid)
            {
                var result = await _uow.CarrierInformationRepo.AddCarrier(carrier);
                return Json(result);
            }
            return Json("failed");
        }

        [Authorize(Permissions.Carrier.TelecomCarrierView)]
        public JsonResult GetCarriersList(string searchValue, string start, string length, string sortColumn, string sortDirection, int draw)
        {
            int totalRecord = 0;
            int filterRecord = 0;

            int pageSize = Convert.ToInt32(length ?? "0");
            int skip = Convert.ToInt32(start ?? "0");
            string sortOrder = string.Empty;

            IEnumerable<CarrierInfomationViewModel> data;

            int carrierCount = 0;


            //sort data
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                sortOrder = (sortColumn + "_" + sortDirection).ToLower();
            }

            data = _uow.CarrierInformationRepo.GetAllCarriers(skip, pageSize, searchValue, sortOrder, out carrierCount);

            totalRecord = carrierCount;

            // get total count of records after search 
            filterRecord = carrierCount;

            var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = filterRecord, data = data.ToList() };
            return Json(returnObj);
        }

        public async Task<IActionResult> CheckForCarrierName(string carrierName, int carrierId)
        {
            try
            {
                bool isNameExists = await _uow.CarrierInformationRepo.GetCarrierName(carrierName, carrierId);
                return Json(isNameExists);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Checking For Valid Name :{ErrorMsg}", ex.Message);
                return Json(false);
            }
        }

        [Authorize(Permissions.Carrier.TelecomCarrierEdit)]
        public async Task<IActionResult> EditCarrier(int carrierId)
        {
            try
            {
                var carrierViewModel = await _uow.CarrierInformationRepo.GetCarrierDetail(carrierId);
                return Json(carrierViewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Carrier Details for Edit :{ErrorMsg}", ex.Message);
                return Json(new CarrierInfomationViewModel());
            }
        }

        [Authorize(Permissions.Carrier.TelecomCarrierEdit)]
        public async Task<IActionResult> UpdateCarrier(CarrierInfomationViewModel carrier)
        {
            try
            {
                var result = "failed";
                if (ModelState.IsValid)
                {
                    result = await _uow.CarrierInformationRepo.UpdateCarrier(carrier);
                    return Json(result);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting Carrier Details for Edit :{ErrorMsg}", ex.Message);
                return Json("failed");
            }
        }

        [Authorize(Permissions.Carrier.TelecomCarrierDelete)]
        public async Task<IActionResult> DeleteCarrier(int carrierId)
        {
            try
            {
                var result = await _uow.CarrierInformationRepo.DeleteCarrierDetail(carrierId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Deleting Carrier Details :{ErrorMsg}", ex.Message);
                return Json("failed");
            }
        }

        private async Task<List<SelectListText>> ServiceList()
        {
            List<SelectListText> selectLists = new List<SelectListText>();
            try
            {
                var data = _uow.DataServiceTypeRepo.GetAll().Where(x => x.IsActive).ToList();
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        SelectListText listText = new SelectListText();
                        listText.Text = item.ServiceName;
                        listText.Value = item.Id.ToString();
                        selectLists.Add(listText);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Data Service Controller / Getting ServiceList :{ErrorMsg}", ex.Message);
            }
            return selectLists;
        }
    }
}
