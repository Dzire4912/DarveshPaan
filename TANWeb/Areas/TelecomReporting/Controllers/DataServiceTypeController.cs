using Microsoft.AspNetCore.Mvc;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using TAN.DomainModels.Helpers;
using Microsoft.AspNetCore.RateLimiting;

namespace TANWeb.Areas.TelecomReporting.Controllers
{

    [Area("TelecomReporting")]
    [Authorize]
    public class DataServiceTypeController : Controller
    {
        private readonly IUnitOfWork _uow;
        public DataServiceTypeController(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        [Route("telecomreporting/dataservicetypes")]
        [Authorize(Permissions.DataServiceType.TelecomDataServiceTypeView)]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Permissions.DataServiceType.TelecomDataServiceTypeCreate)]
        [EnableRateLimiting("AddPolicy")]
        public async Task<IActionResult> SaveDataServiceType(DataServiceTypeViewModel data)
        {
            Response response = new Response();
            try
            {
                if (data != null)
                {
                    response = await _uow.DataServiceTypeRepo.AddDataServiceType(data);
                    return Json(response);
                }
                else
                {
                    response.StatusCode = Convert.ToInt32(ServiceTypeStatus.Failed);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in SaveDataServiceType controller:{ErrorMsg}", ex.Message);
            }
            return Json(response);
        }
        [HttpPost]
        [Authorize(Permissions.DataServiceType.TelecomDataServiceTypeView)]
        public JsonResult GetDataServiceType(string? searchValue, string? start, string? length, int? draw, string? sortColumn, string? sortDirection)
        {
            IEnumerable<DataServiceTypeViewModel> datatypes = new List<DataServiceTypeViewModel>();
            int totalRecord = 0;
            try
            {
                string sortOrder = string.Empty;
                int dataServiceTypeCount = 0;
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    sortOrder = (sortColumn + "_" + sortDirection).ToLower();
                }
                datatypes = _uow.DataServiceTypeRepo.GetAllDataServiceType(searchValue, sortOrder, start, length, out dataServiceTypeCount);

                totalRecord = dataServiceTypeCount;

                var returnObj = new { draw = draw, recordsTotal = totalRecord, recordsFiltered = totalRecord, data = datatypes.ToList() };
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetDataServiceType controller:{ErrorMsg}", ex.Message);
            }
            var _returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                data = datatypes
            };
            return Json(_returnObj);

        }

        [Authorize(Permissions.DataServiceType.TelecomDataServiceTypeEdit)]
        public async Task<IActionResult> GetDataServiceTypeById(string dataServiceTypeId)
        {
            try
            {
                if (dataServiceTypeId != null || dataServiceTypeId != "")
                {
                    var serviceType = await _uow.DataServiceTypeRepo.GetDataServiceTypeById(dataServiceTypeId);
                    return Json(serviceType);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetDataServiceTypeById controller:{ErrorMsg}", ex.Message);
            }
            return Json(null);
        }

        [Authorize(Permissions.DataServiceType.TelecomDataServiceTypeDelete)]
        [HttpDelete]
        public async Task<IActionResult> DeleteDataServiceTypeById(string dataServiceTypeId)
        {
            Response response = new Response();
            try
            {
                if (dataServiceTypeId != null || dataServiceTypeId != "")
                {
                    response = await _uow.DataServiceTypeRepo.DeleteDataServiceTypeById(dataServiceTypeId);
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in DeleteDataServiceTypeById controller:{ErrorMsg}", ex.Message);
            }

            return Json(response);
        }
    }
}
