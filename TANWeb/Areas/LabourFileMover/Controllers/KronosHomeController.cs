using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Interface;


namespace TANWeb.Areas.LabourFileMover.Controllers
{
    [Area("LabourFileMover")]
    public class KronosHomeController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        private readonly IUpload _upload;     
        public KronosHomeController(IUnitOfWork uow, UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager, IUpload upload)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _upload = upload;
           
        }
        #region Index
        [Route("kronos/home")]
        public async Task<IActionResult> Index()
        {
            KronosFacilitiesVM facilitiesVm = new KronosFacilitiesVM();
            List<KronosUnprocessedData> unprocessedData = new List<KronosUnprocessedData>();
            unprocessedData = _uow.KronosUnprocessedRepo.GetAll().ToList();
            var organizationData = _uow.OrganizationRepo.GetAll().ToList();

            var processedData = unprocessedData
                .Join(organizationData,
                    kronosData => kronosData.OrganizationId,
                    orgData => orgData.OrganizationID,
                    (kronosData, orgData) => new KronosUnprocessedData
                    {
                        // Assign KronosUnprocessedData properties here
                        Id = kronosData.Id,
                        ADJUSTEDAPPLYDATE = kronosData.ADJUSTEDAPPLYDATE,
                        DepartmentDescription = kronosData.DepartmentDescription,
                        EmployeeID = kronosData.EmployeeID,
                        HrJob = kronosData.HrJob,
                        JOBCODE = kronosData.JOBCODE,
                        PayCode = kronosData.PayCode,
                        TIMEINHOURS = kronosData.TIMEINHOURS,
                        OrganizationId = kronosData.OrganizationId,
                        Reason = kronosData.Reason,
                        EmployeeName = kronosData.EmployeeName,
                        facilityId = kronosData.facilityId,
                        DEPARTMENTNUMBER = kronosData.DEPARTMENTNUMBER,
                        // Assign OrganizationName
                        OrganizationName = orgData?.OrganizationName
                    })
                .ToList();

            facilitiesVm.KronosUnprocessedData = processedData;

            _uow.kronosfacilitRepo.DeleteTimesheetFacilities();
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Other))
            {
                var userOrg = _uow.UserOrganizationFacilitiesRepo.GetAll().FirstOrDefault(c => c.UserId == currentUser.Id);
                if (userOrg != null)
                {
                    var newFacilityList = _uow.kronosfacilitRepo.GetAll().Where(c => c.OrganizationId == userOrg.OrganizationID).ToList();
                    facilitiesVm.OrganizationList = _uow.OrganizationRepo.GetAll().Where(o => o.IsActive && o.OrganizationID == userOrg.OrganizationID).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();
                    facilitiesVm.tracks = _uow.kronosDataTrackRepo.GetAll().OrderByDescending(c => c.CreatedDate).ToList();
                    facilitiesVm.kronos = newFacilityList;
                    facilitiesVm.KronosUnprocessedData = processedData.Where(x => x.OrganizationId == userOrg.OrganizationID).ToList();
                }

            }
            else
            {
                var newFacilityList = _uow.kronosfacilitRepo.GetAll().OrderByDescending(x => x.LastUpdatedDate).ToList();
                foreach (var facility in newFacilityList)
                {
                    var thisOrg = _uow.OrganizationRepo.GetAll().Where(x => x.OrganizationID == facility.OrganizationId).FirstOrDefault();
                    facility.OrgName = thisOrg.OrganizationName;
                }
                facilitiesVm.OrganizationList = _uow.OrganizationRepo.GetAll().Where(o => o.IsActive).Select(o => new Itemlist { Text = o.OrganizationName, Value = o.OrganizationID }).ToList();
                facilitiesVm.tracks = _uow.kronosDataTrackRepo.GetAll().OrderByDescending(c => c.CreatedDate).ToList();
                facilitiesVm.kronos = newFacilityList;
            }
            facilitiesVm.StateList = _uow.StateRepo.GetAll().Select(s => new StateModel { StateName = s.StateName, StateCode = s.StateCode }).ToList();
            facilitiesVm.ApplicationList = _uow.ApplicationRepo.GetAll().Select(a => new Itemlist { Text = a.Name, Value = a.ApplicationId }).ToList();
            ViewBag.FirstOrg = _uow.ApplicationRepo.GetAll().Where(c => c.Name == ApplicationNameWithId.PBJSnap.ToString()).Select(h => h.ApplicationId).FirstOrDefault();
            return View(facilitiesVm);
        }
        #endregion
        #region Delete Facility
        [Route("LabourFileMover/KronosHome/Delete")]
        public bool DeleteByFid(string id)
        {
            try
            {
                var item = _uow.kronosfacilitRepo.GetAll().Where(c => c.FacilityId == id).FirstOrDefault();
                if (item == null)
                {
                    return false;
                }
                item.IsAvailable = true;
                item.LastUpdatedDate = DateTime.UtcNow;
                _uow.kronosfacilitRepo.Update(item);
                _uow.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        #endregion
        #region Retreive data and send to TimeSheet
        [Route("LabourFileMover/KronosHome/ProcessData")]
        public async Task<ActionResult> ProcessData(string id, string orgId)
        {
            var result = "";
            // create datatable
            var dataTable = new DataTable();
            //Full Name  e.g . first name last name
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.FullNameWithLastNamefirst), typeof(string));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.EmployeeId), typeof(string));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.FacilityId), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.WorkDay), typeof(string));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.PayTypeCode), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.JobTitleCode), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.Hours), typeof(decimal));

            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var parameters = new object[] { new SqlParameter("@facility_id", id) };

                    var SpParameters = new object[]
                                 {
                                     new SqlParameter("@facility_id",id),
                                     new SqlParameter("@OrgId",orgId)
                                  };
                    var facilitycheck = _uow.FacilityRepo.GetAll().FirstOrDefault(x => x.FacilityID == id && x.OrganizationId.ToString() == orgId);
                    if (facilitycheck != null)
                    {
                        //Map from punch export
                        var spResult = await _uow.executeSpRepo.ExecWithStoreProcedure("exec sp_loaddatainto_kronosTimesheetMapped_wo_date @facility_id,@OrgId ", SpParameters);
                        if (spResult != null && spResult.ToList().Count > 0)
                        {
                            if (spResult.FirstOrDefault().result != Convert.ToInt32(SPExecResult.Success))
                            {
                                Log.Error("ex{}", "Load Mapping failed");
                                result = OperationResult.NoDataExists.ToString();
                                return Json(result);
                            }

                            //Get Insertables data
                            var results = await _uow.kronosSpResultsRepo.ExecWithStoreProcedure("exec sp_kronosandtimecomparationdata_wo_date_w_facility @facility_id,@OrgId ", SpParameters);

                            if (results != null && results.ToList().Count > 0)
                            {
                                //bind to datatable
                                foreach (var data in results)
                                {
                                    var row = dataTable.NewRow();

                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.FullNameWithLastNamefirst)] = data.FullName;
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.EmployeeId)] = data.EmployeeId; // Map entity properties to DataTable columns
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.FacilityId)] = data.FacilityId;
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.WorkDay)] = data.Workday;
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.PayTypeCode)] = data.PayTypeCode;
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.JobTitleCode)] = data.JobTitleCode;
                                    row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.Hours)] = data.THours;
                                    dataTable.Rows.Add(row);
                                }
                                if (dataTable.Rows.Count > 0)
                                {
                                    //send to validate
                                    var final = await _upload.KronosLogic(dataTable, 0);
                                    result = OperationResult.Success.ToString();
                                }
                            }
                            else
                            {
                                result = OperationResult.Success.ToString();
                                return Json(result);
                            }
                        }
                    }
                    else
                    {
                        result = "No Facility";
                        return Json(result);
                    }
                }
                if (result == OperationResult.Success.ToString())
                {
                    var deletable = _uow.kronosfacilitRepo.GetAll().Where(x => x.FacilityId == id).FirstOrDefault();
                    if (deletable != null)
                    {
                        _uow.kronosfacilitRepo.Delete(deletable);
                        _uow.SaveChanges();
                    }
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                //log
                KronosDataTrack kdatalog = new KronosDataTrack();
                Guid guid = Guid.NewGuid();
                kdatalog.Id = guid.ToString();
                kdatalog.ToTable = KronosSource.TimeSheet.ToString();
                kdatalog.Description = "Error Retrieving Data";
                kdatalog.CreatedDate = DateTime.UtcNow;
                kdatalog.RecordsCount = 0;
                kdatalog.AutoSyncCount = 0;
                kdatalog.Source = "User Action";
                _uow.kronosDataTrackRepo.Add(kdatalog);
                _uow.SaveChanges();
                return Json(OperationResult.Failed.ToString());
            }

        }

        #endregion

        //public async Task<ActionResult> TriggerWork()
        //{
        //    var dataDate = _uow.kronosDataTrackRepo.GetAll().OrderByDescending(c => c.CreatedDate).FirstOrDefault().CreatedDate;
        //    var currentDateTime = DateTime.UtcNow;
        //    TimeSpan timeDifference = currentDateTime - dataDate;
        //    try
        //    {
        //        if (timeDifference.Hours > 1)
        //        {
        //            _bgService.TriggerDoWork();
        //        }
        //        else
        //        {
        //            TempData["WarningmsgPop"] = "Please wait for an hour and try Again";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "An Error Occurred in TriggerWork{c}", ex.Message);
        //    }
        //    return RedirectToAction("Index"); // Redirect to an appropriate page after triggering the work

        //}
    }
}
