using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;
using TANWeb.Interface;
using TANWeb.Resources;

namespace TANWeb.Areas.LabourFileMover.Controllers
{
    [Area("LabourFileMover")]
    public class KronosPendingController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private IUnitOfWork _uow;
        private readonly IUpload _upload;
        #region Constructor
        public KronosPendingController(IUnitOfWork uow, UserManager<AspNetUser> userManager, RoleManager<AspNetRoles> roleManager, IUpload upload)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _upload = upload;
        }
        #endregion
        #region List view for Pending to compare Records
        [Route("kronos/conflicts")]
        public async Task<IActionResult> Index(string? userName, string? orgId, string? facilityId, string? empId, string? quarter)
        {
            KronosPendingVM kronosPendingVM = new KronosPendingVM();
            var kronosLastAction=_uow.kronosDataTrackRepo.GetAll().OrderByDescending(x=>x.CreatedDate).FirstOrDefault();
            if (kronosLastAction != null && kronosLastAction.Status==Convert.ToInt32(OperationResult.Success)) 
            {
                TempData["WarningmsgPop"] = "Please wait until the Sync Completes";
            }
            var allpendingRecords = _uow.KronosPendingRecordsRepo.GetAll().ToList();
            var onetimeUpdates = _uow.OneTimeDeductionRepo.GetAll().ToList();
            List<KronosPendingRecords> removablePendingRecords =
                        (from pendingRecord in allpendingRecords
                         join oneTimeUpdate in onetimeUpdates
                         on new { pendingRecord.EmployeeId, pendingRecord.FacilityId, pendingRecord.WorkDay }
                         equals new { oneTimeUpdate.EmployeeId, oneTimeUpdate.FacilityId, oneTimeUpdate.WorkDay }
                         select pendingRecord).ToList();
            if (removablePendingRecords.Any())
            {
                _uow.KronosPendingRecordsRepo.RemoveRange(removablePendingRecords);
                _uow.SaveChanges();
            }
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null && currentUser.UserType == Convert.ToInt32(UserTypes.Other))
            {
                var userOrg = _uow.UserOrganizationFacilitiesRepo.GetAll().FirstOrDefault(c => c.UserId == currentUser.Id);
                if (userOrg != null)
                {
                    orgId = userOrg.OrganizationID.ToString();
                }
            }
            var pendingRecords = _uow.KronosPendingRecordsRepo.GetAll().ToList();
            var orgList = _uow.OrganizationRepo.GetAll().Select(c => new Itemlist { Text = c.OrganizationName, Value = c.OrganizationID }).ToList();
            var facilitylist = _uow.FacilityRepo.GetAll().Select(c => new Itemlist { Text = c.FacilityName, Value = c.Id }).ToList();
            var emplist = _uow.EmployeeRepo.GetAll().Select(x => new Itemlist { Text = x.EmployeeId, Value = x.Id }).ToList();
            var current = _uow.MonthsRepo.GetAll().Select(c => new Itemlist { Text = c.Name, Value = c.Id }).ToList();
            if (!string.IsNullOrEmpty(orgId))
            {
                facilitylist = _uow.FacilityRepo.GetAll().Where(x => x.OrganizationId.ToString() == orgId).Select(c => new Itemlist { Text = c.FacilityName, Value = c.Id }).ToList();
                List<int> ids = facilitylist.Select(c => c.Value).ToList();
                pendingRecords = pendingRecords.Where(c => ids.Contains(c.FacilityId)).ToList();
            }
            if (!string.IsNullOrEmpty(facilityId))
            {
                emplist = _uow.EmployeeRepo.GetAll().Where(x => x.FacilityId.ToString() == facilityId).Select(x => new Itemlist { Text = x.EmployeeId, Value = x.Id }).ToList();
                pendingRecords = pendingRecords.Where(c => c.FacilityId.ToString() == facilityId).ToList();
            }
            if (!string.IsNullOrEmpty(empId))
            {
                pendingRecords = pendingRecords.Where(x => x.EmployeeId == empId).ToList();
            }
            if (!string.IsNullOrEmpty(quarter))
            {
                var months = _uow.MonthsRepo.GetAll().Where(x => x.Quarter.ToString() == quarter).Select(x => x.Id).ToList();
                pendingRecords = pendingRecords.Where(c => months.Contains(c.WorkDay.Month)).ToList();
            }
            if (!string.IsNullOrEmpty(userName))
            {
                pendingRecords = pendingRecords
                    .Where(c => (!string.IsNullOrEmpty(c.EmployeeName) && c.EmployeeName.ToUpper().Contains(userName.ToUpper())) ||
                                c.EmployeeId.Contains(userName))
                    .ToList();
            }

            List<Itemlist> Qarters = new List<Itemlist>();
            Qarters.Add(new Itemlist { Text = "Oct to Dec :1st Quarter", Value = 1 });
            Qarters.Add(new Itemlist { Text = "Jan to Mar :2nd Quarter", Value = 2 });
            Qarters.Add(new Itemlist { Text = "Apr to Jun :3rd Quarter", Value = 3 });
            Qarters.Add(new Itemlist { Text = "Jul to Sep :4th Quarter", Value = 4 });


            kronosPendingVM.kronosPendingdata = pendingRecords;
            kronosPendingVM.QuarterList = Qarters;
            kronosPendingVM.Orglist = orgList;
            kronosPendingVM.EmployeeList = emplist;
            kronosPendingVM.facilityList = facilitylist;
            return View(kronosPendingVM);
        }
        #endregion
        #region Get Comparision data from Kronos and Pbj Timesheet
        public IActionResult GetComparisionData(string rowiD)
        {
            List<KronosComarisionVM> kronosComarisionVMList = new List<KronosComarisionVM>();


            var pemdingRecords = _uow.KronosPendingRecordsRepo.GetAll().Where(c => c.rowid.ToString() == rowiD).ToList();
            if (pemdingRecords.Count > 0)
            {
                //Get Kronos TimeSheet Maped Data
                var KronosTimeSheetData = _uow.kronosMappedRepo.GetAll().Where(x => x.EmployeeId == pemdingRecords[0].EmployeeId && x.FacilityId == pemdingRecords[0].FacilityId && x.Workday == pemdingRecords[0].WorkDay).ToList();
                //Get PBjNao TimeSheet
                var TimeSheetData = _uow.TimesheetRepo.GetAll().Where(x => x.EmployeeId == pemdingRecords[0].EmployeeId && x.FacilityId == pemdingRecords[0].FacilityId && x.Workday == pemdingRecords[0].WorkDay).ToList();

                //Bind Kronos TimeSheet Maped Data to View model
                foreach (var item in KronosTimeSheetData)
                {
                    KronosComarisionVM kronosComarisionVM = new KronosComarisionVM();
                    kronosComarisionVM.facilityId = item.FacilityId;
                    kronosComarisionVM.FacilityName = _uow.FacilityRepo.GetById(item.FacilityId).FacilityName;
                    kronosComarisionVM.Hours = item.THours;
                    kronosComarisionVM.WorkDay = item.Workday;
                    kronosComarisionVM.EmployeeId = item.EmployeeId;
                    kronosComarisionVM.PayTypeCode = item.PayTypeCode;
                    kronosComarisionVM.JobTypeCode = item.JobTitleCode;
                    kronosComarisionVM.PayType = _uow.PayTypeCodesRepo.GetAll().Where(c => c.PayTypeCode == item.PayTypeCode).FirstOrDefault().PayTypeDescription;
                    kronosComarisionVM.JobType = _uow.JobCodesRepo.GetAll().Where(c => c.Id == item.JobTitleCode).FirstOrDefault().Title;
                    kronosComarisionVM.Source = KronosSource.Kronos.ToString();

                    kronosComarisionVMList.Add(kronosComarisionVM);

                }
                //Bind  TimeSheet Data to View model
                foreach (var item in TimeSheetData)
                {
                    KronosComarisionVM kronosComarisionVM = new KronosComarisionVM();
                    kronosComarisionVM.facilityId = item.FacilityId;
                    kronosComarisionVM.FacilityName = _uow.FacilityRepo.GetById(item.FacilityId).FacilityName;
                    kronosComarisionVM.Hours = item.THours;
                    kronosComarisionVM.WorkDay = item.Workday;
                    kronosComarisionVM.EmployeeId = item.EmployeeId;
                    kronosComarisionVM.PayTypeCode = item.PayTypeCode;
                    kronosComarisionVM.JobTypeCode = item.JobTitleCode;
                    kronosComarisionVM.Source = KronosSource.TimeSheet.ToString();
                    kronosComarisionVM.PayType = _uow.PayTypeCodesRepo.GetAll().Where(c => c.PayTypeCode == item.PayTypeCode).FirstOrDefault().PayTypeDescription;
                    kronosComarisionVM.JobType = _uow.JobCodesRepo.GetAll().Where(c => c.Id == item.JobTitleCode).FirstOrDefault().Title;
                    kronosComarisionVM.Id = item.Id.ToString();

                    kronosComarisionVMList.Add(kronosComarisionVM);

                }
            }
            return PartialView("~/Areas/LabourFileMover/Views/KronosPending/_GetComparisionData.cshtml", kronosComarisionVMList);
        }
        #endregion
        #region Delete and Send data to Timesheet
        public async Task<IActionResult> UpdateData(List<KronosComarisionVM> viewModel)
        {
            //define Data Tble
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.EmployeeId), typeof(string));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.FacilityId), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.WorkDay), typeof(string));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.PayTypeCode), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.JobTitleCode), typeof(int));
            dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.Hours), typeof(decimal));
            //Initialize Transaction
            using (var transaction = _uow.BeginTransaction())
            {
                try
                {
                    if (viewModel != null || viewModel.Count > 0)
                    {
                        //get deletable records from KronosPendingRecords,PbjsnapTimesheet,KronosTimeSheet
                        var deletablePendingRecords = _uow.KronosPendingRecordsRepo.GetAll().Where(x => x.EmployeeId == viewModel[0].EmployeeId && x.FacilityId == viewModel[0].facilityId && x.WorkDay == viewModel[0].WorkDay).FirstOrDefault();
                        var timeSheetDeletables = _uow.TimesheetRepo.GetAll().Where(x => x.EmployeeId == viewModel[0].EmployeeId && x.FacilityId == viewModel[0].facilityId && x.Workday == viewModel[0].WorkDay).ToList();
                        var kronosTimeDeletables = _uow.kronosMappedRepo.GetAll().Where(x => x.EmployeeId == viewModel[0].EmployeeId && x.FacilityId == viewModel[0].facilityId && x.Workday == viewModel[0].WorkDay).ToList();
                        //Delete Gathered Records
                        _uow.kronosMappedRepo.RemoveRange(kronosTimeDeletables);
                        _uow.TimesheetRepo.RemoveRange(timeSheetDeletables);
                        _uow.KronosPendingRecordsRepo.Delete(deletablePendingRecords);
                        _uow.SaveChanges();
                        //Bind Data with DataTable
                        foreach (var data in viewModel)
                        {
                            var row = dataTable.NewRow();                           
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.EmployeeId)] = data.EmployeeId;
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.FacilityId)] = data.facilityId;
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.WorkDay)] = data.WorkDay;
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.PayTypeCode)] = data.PayTypeCode;
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.JobTitleCode)] = data.JobTypeCode;
                            row[EnumHelper.GetEnumDescription(KronosTimeSheetColums.Hours)] = data.Hours;
                            dataTable.Rows.Add(row);
                        }
                        //Send datatable to validate
                        var result = await _upload.KronosLogic(dataTable, 0);
                        if (result)
                        {
                            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                            KronosOneTimeUpdate addPending = new KronosOneTimeUpdate();
                            addPending.WorkDay = viewModel[0].WorkDay;
                            addPending.FacilityId = viewModel[0].facilityId;
                            addPending.EmployeeId = viewModel[0].EmployeeId;
                            addPending.CreatedAt = DateTime.UtcNow;
                            addPending.CreatedBy = currentUser.Id;
                            _uow.OneTimeDeductionRepo.Add(addPending);
                            _uow.SaveChanges();
                        }

                    }

                    transaction.Commit();
                    return Json(OperationResult.Success.ToString());
                }
                catch (Exception ex)
                {
                    //Revert Transaction if error occurs
                    transaction.Rollback();
                    return Json(OperationResult.Failed.ToString());
                }
            }

        }
        #endregion
        #region Get Facilities by OrgId
        public async Task<List<Itemlist>> GetFacilities(string id)
        {
            var facilities = _uow.FacilityRepo.GetAll().Where(x => x.OrganizationId.ToString() == id).Select(c => new Itemlist { Text = c.FacilityName, Value = c.Id }).ToList();
            return facilities;
        }
        #endregion
        #region Get Employees by Facility Id
        public async Task<List<Itemlist>> GetEmployees(string id)
        {
            var emplist = _uow.EmployeeRepo.GetAll().Where(x => x.FacilityId.ToString() == id).Select(c => new Itemlist { Text = c.EmployeeId, Value = c.Id }).ToList();
            return emplist;
        }
        #endregion
    }
}
