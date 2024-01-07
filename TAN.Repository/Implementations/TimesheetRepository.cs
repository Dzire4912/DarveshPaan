using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;


namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class TimesheetRepository : Repository<Timesheet>, ITimesheetRepository
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public TimesheetRepository(DbContext db)
        {
            this.db = db;
        }

        public async Task<List<TimesheetEmployeeDataResponse>> GetWorkdayDetails(TimesheetEmployeeDataRequest timeReq)
        {
            List<TimesheetEmployeeDataResponse> timesheet = new List<TimesheetEmployeeDataResponse>();
            try
            {
                timesheet = await Context.Timesheet.Where(x => x.IsActive
                && x.FacilityId == Convert.ToInt32(timeReq.FacilityId)
                && timeReq.EmployeeId.Contains(x.EmployeeId)
                 && (x.Workday >= timeReq.StartDate && x.Workday <= timeReq.EndDate)
               
                && (x.Status == (int)TimesheetStatus.New || x.Status == (int)TimesheetStatus.NextDay || x.Status == (int)TimesheetStatus.Approved || x.Status == (int)TimesheetStatus.Validate))
                    .Select(z => new TimesheetEmployeeDataResponse()
                {
                    EmployeeId = z.EmployeeId,
                    Workday = z.Workday,
                    FacilityId = z.FacilityId,
                    THours = z.THours
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetWorkdayDetails:{ErrorMsg}", ex.Message);
                throw;
            }
            return timesheet;
        }

        public async Task<bool> InsertTimesheet(List<Timesheet> timesheet)
        {
            try
            {
                if (timesheet.Count > 0)
                {
                    await Context.Timesheet.AddRangeAsync(timesheet);
                    await Context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured InsertTimesheet :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public List<StaffingDepartmentDetails> GetStaffingDepartmentDetails(StaffingDepartmentCsvRequest csvRequest, int PageSize, int PageNo, out int TotalCount, string search, string sortOrder)
        {
            IEnumerable<StaffingDepartmentDetails> StaffDetails;
            TotalCount = 0;
            try
            {
                StaffDetails = (from t in Context.Timesheet
                                join jc in Context.JobCodes on t.JobTitleCode equals jc.Id
                                join f in Context.Facilities on t.FacilityId equals f.Id
                                where t.Year == csvRequest.Year && t.ReportQuarter == csvRequest.ReportQuarter && t.FacilityId == csvRequest.FacilityID
                                 && t.IsActive && f.IsActive
                                group new { t, jc, f } by new { jc.Title, jc.Id, f.FacilityName } into grouped
                                select new StaffingDepartmentDetails
                                {
                                    Title = grouped.Key.Title,
                                    TotalHours = grouped.Sum(x => x.t.THours).ToString(),
                                    FacilityName = grouped.Key.FacilityName,
                                    StaffCount = Context.Employee.Count(e => e.FacilityId == csvRequest.FacilityID && e.JobTitleCode == grouped.Key.Id
                                    && e.IsActive).ToString(),
                                    Exempt = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.Exempt && e.FacilityId == csvRequest.FacilityID
                                    && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString(),
                                    NonExempt = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.NonExempt && e.FacilityId == csvRequest.FacilityID
                                    && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString(),
                                    Contractors = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.Contractor && e.FacilityId == csvRequest.FacilityID
                                    && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString()
                                }).ToList();

                //       var groupedTimesheets = (from t in Context.Timesheet
                //                                join jc in Context.JobCodes on t.JobTitleCode equals jc.Id
                //                                join f in Context.Facilities on t.FacilityId equals f.Id
                //                                where t.Year == csvRequest.Year &&
                //                                      t.ReportQuarter == csvRequest.ReportQuarter &&
                //                                      t.FacilityId == csvRequest.FacilityID &&
                //                                      t.IsActive && f.IsActive &&
                //                                      (string.IsNullOrEmpty(search) || jc.Title.Contains(search))
                //                                select new
                //                                {
                //                                    JobCodeTitle = jc.Title, // Distinct name
                //                                    JobCodeId = jc.Id, // Distinct name
                //                                    FacilityId = f.Id,
                //                                    FacilityName = f.FacilityName,// Distinct name
                //                                    Timesheet = t // Include the Timesheet entity in the result
                //                                }).ToList();

                //       var groupedAndFiltered = groupedTimesheets
                //           .GroupBy(x => new { x.JobCodeTitle, x.JobCodeId, x.FacilityId, x.FacilityName }) // Group by distinct names
                //           .Where(grouped => grouped.All(g => g.Timesheet.IsActive)); // Filter based on IsActive

                //       StaffDetails = groupedAndFiltered
                //           .Select(grouped => new StaffingDepartmentDetails
                //           {
                //               Title = grouped.Key.JobCodeTitle,
                //               TotalHours = grouped.Sum(x => x.Timesheet.THours).ToString(),
                //               FacilityId = grouped.Key.FacilityId.ToString(),
                //               Id = grouped.Key.JobCodeId,
                //               FacilityName = grouped.Key.FacilityName
                //           })
                //           .ToList();

                //       foreach (var detail in StaffDetails)
                //       {
                //           var employeeIds = groupedTimesheets
                //               .Where(t => t.JobCodeTitle == detail.Title && t.JobCodeId == detail.Id && t.FacilityId == Convert.ToInt32(detail.FacilityId)) // Filter based on distinct names
                //               .Select(t => t.Timesheet.EmployeeId)
                //               .Distinct()
                //               .ToList();

                //           detail.StaffCount = Context.Employee
                //               .Count(e => employeeIds.Contains(e.EmployeeId) && e.FacilityId == csvRequest.FacilityID)
                //               .ToString();

                //           detail.Exempt = Context.Timesheet
                //.Count(e => employeeIds.Contains(e.EmployeeId) && e.PayTypeCode == (int)PayTypeCode.Exempt && e.FacilityId == csvRequest.FacilityID && e.JobTitleCode == detail.Id)
                //.ToString();

                //           // Calculate NonExempt count
                //           detail.NonExempt = Context.Timesheet
                //               .Count(e => employeeIds.Contains(e.EmployeeId) && e.PayTypeCode == (int)PayTypeCode.NonExempt && e.FacilityId == csvRequest.FacilityID && e.JobTitleCode == detail.Id)
                //               .ToString();

                //           // Calculate Contractors count
                //           detail.Contractors = Context.Timesheet
                //               .Count(e => employeeIds.Contains(e.EmployeeId) && e.PayTypeCode == (int)PayTypeCode.Contractor && e.FacilityId == csvRequest.FacilityID && e.JobTitleCode == detail.Id)
                //               .ToString();

                //       }

                if (StaffDetails.Count() > 0)
                {
                    TotalCount = StaffDetails.Count();
                    StaffDetails = StaffDetails.Skip(PageNo).Take(PageSize).ToList();
                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "title_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.Title).ToList();
                                break;
                            case "title_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.Title).ToList();
                                break;
                            case "totalhours_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.TotalHours).ToList();
                                break;
                            case "totalhours_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.TotalHours).ToList();
                                break;
                            case "staffcount_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.StaffCount).ToList();
                                break;
                            case "staffcount_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.StaffCount).ToList();
                                break;
                            case "exempt_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.Exempt).ToList();
                                break;
                            case "exempt_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.Exempt).ToList();
                                break;
                            case "nonexempt_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.NonExempt).ToList();
                                break;
                            case "nonexempt_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.NonExempt).ToList();
                                break;
                            case "contractors_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.Contractors).ToList();
                                break;
                            case "contractors_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.Contractors).ToList();
                                break;
                            case "facilityName_asc":
                                StaffDetails = StaffDetails.OrderBy(x => x.FacilityName).ToList();
                                break;
                            case "facilityName_desc":
                                StaffDetails = StaffDetails.OrderByDescending(x => x.FacilityName).ToList();
                                break;
                        }
                    }
                }
                return (List<StaffingDepartmentDetails>)StaffDetails;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return new List<StaffingDepartmentDetails>();
            }

        }

        public async Task<List<EmployeeTimesheetData>> GetEmployeeTimesheetData(EmployeeTimesheetDataRequest timesheetDataRequest)
        {
            List<EmployeeTimesheetData> TimesheetList = new List<EmployeeTimesheetData>();
            try
            {
                TimesheetList = await Context.Timesheet.Where(t => t.EmployeeId == timesheetDataRequest.EmployeeId && t.IsActive
                && t.FacilityId == timesheetDataRequest.FacilityId

                /* && t.Year == timesheetDataRequest.Year && t.ReportQuarter == timesheetDataRequest.ReportQuarter*/

                && (t.Workday >= timesheetDataRequest.StartDate && t.Workday <= timesheetDataRequest.EndDate)
                && (t.Status == (int)TimesheetStatus.NextDay || t.Status == (int)TimesheetStatus.New || t.Status == (int)TimesheetStatus.Validate || t.Status == (int)TimesheetStatus.Approved))
      //.GroupBy(t => new { t.Workday, t.Id, t.EmployeeId, t.FacilityId })
      .Select(g => new EmployeeTimesheetData
      {
          Workday = g.Workday,
          TimesheetId = Convert.ToInt32(g.Id),
          EmployeeId = g.EmployeeId,
          FacilityId = g.FacilityId,
          TotalHours = g.THours.ToString(),
          Color = (g.Status == (int)TimesheetStatus.Approved) ? "orange" : (g.Status == (int)TimesheetStatus.Validate) ? "green" : "red",
          Editable = (g.Status != (int)TimesheetStatus.Approved),
          IsButtonEditable = ((g.Status == (int)TimesheetStatus.New) || (g.Status == (int)TimesheetStatus.NextDay)) ? true : false
      }).Distinct().ToListAsync();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return TimesheetList;
        }

        public async Task<bool> AddTimesheet(Timesheet timesheet)
        {
            try
            {
                if (timesheet != null)
                {
                    if (timesheet.Id == 0)
                    {
                        await Context.Timesheet.AddAsync(timesheet);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        Context.Timesheet.Where(x => x.IsActive && x.Id == timesheet.Id)
                        .ExecuteUpdate(e => e.SetProperty(s => s.THours, s => timesheet.THours)
                        .SetProperty(s => s.UpdateBy, s => timesheet.UpdateBy)
                        .SetProperty(e => e.JobTitleCode, e => timesheet.JobTitleCode)
                        .SetProperty(e => e.PayTypeCode, e => timesheet.PayTypeCode)
                        .SetProperty(s => s.UpdateDate, s => timesheet.UpdateDate));
                        await Context.SaveChangesAsync();
                        return true;
                        /*Context.Timesheet.Update(timesheet);
                        await Context.SaveChangesAsync();
                        return true;*/
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<Timesheet> GetEmpTimesheetData(EmployeeTimesheetDataRequest dataRequest)
        {
            Timesheet timesheet = new Timesheet();
            try
            {
                timesheet = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == dataRequest.FacilityId && x.EmployeeId == dataRequest.EmployeeId &&
                (dataRequest.TimesheetId == null || x.Id == Convert.ToInt32(dataRequest.TimesheetId))
                && x.Year == dataRequest.Year && x.Workday == Convert.ToDateTime(dataRequest.Workday)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeTimesheetData :{ErrorMsg}", ex.Message);
            }
            return timesheet;
        }

        public async Task<ApproveTimesheetResponse> ApproveTimesheetData(ApproveTimesheetRequest approve)
        {
            ApproveTimesheetResponse response = new ApproveTimesheetResponse();
            try
            {
                //Approve Timesheet Data 
                var data = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == approve.FacilityId
                && x.Year == approve.Year && x.ReportQuarter == approve.ReportQuarter && x.THours != 0
                && (x.Status == (int)TimesheetStatus.Validate)).ToListAsync();
                if (data.Count > 0)
                {
                    List<Timesheet> timesheetsList = new List<Timesheet>();
                    foreach (var item in data)
                    {
                        item.Status = (int)TimesheetStatus.Approved;
                        item.UpdateDate = DateTime.Now;
                        item.UpdateBy = approve.UserId;
                        timesheetsList.Add(item);
                    }
                    if (timesheetsList.Count > 0)
                    {
                        await SaveReportApprovedInformation(approve);
                        Context.UpdateRange(timesheetsList);
                        await Context.SaveChangesAsync();
                        response.Success = true;
                        response.Message = "Record approved successfully";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ApproveTimesheetData :{ErrorMsg}", ex.Message);
                response.Success = false;
                response.Message = "Failed to approved the data";
                return response;
            }
            response.Success = false;
            response.Message = "No new record(s) found to approved";
            return response;
        }

        private async Task<bool> SaveReportApprovedInformation(ApproveTimesheetRequest approve)
        {
            try
            {
                ReportVerification reportVerification = new();
                reportVerification.ApprovedDate = DateTime.UtcNow;
                reportVerification.ApprovedByUserId = approve.UserId;
                reportVerification.FacilityId = approve.FacilityId;
                reportVerification.Year = approve.Year;
                reportVerification.Quarter = approve.ReportQuarter;
                reportVerification.Month = 0;
                reportVerification.CreatedDate = DateTime.UtcNow;
                reportVerification.CreatedBy = approve.UserId;
                reportVerification.IsActive = true;
                reportVerification.ApprovedStatus = "Approved";

                Context.ReportVerification.Add(reportVerification);
                await Context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured SaveReportApprovedInformation :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<List<DateTime>> GetMissingDatesByJobTitle(int FacilityId, DateTime StartDate, DateTime EndDate, int JobTitle)
        {
            List<DateTime> List = new List<DateTime>();
            try
            {
                List = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == FacilityId && x.JobTitleCode == JobTitle
                && (x.Workday >= StartDate && x.Workday <= EndDate)).Select(x => x.Workday).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetMissingDatesByJobTitle :{ErrorMsg}", ex.Message);
            }
            return List;
        }

        public DataTable GetStaffingDepartmentDetailsForCsv(int FacilityId, int ReportQuarter, int year)
        {
            DataTable dataTable = new DataTable();
            try
            {
                var data = (from t in Context.Timesheet
                            join jc in Context.JobCodes on t.JobTitleCode equals jc.Id
                            join f in Context.Facilities on t.FacilityId equals f.Id
                            where t.Year == year && t.ReportQuarter == ReportQuarter && t.FacilityId == FacilityId
                             && t.IsActive && f.IsActive
                            group new { t, jc, f } by new { jc.Title, jc.Id, f.FacilityName } into grouped
                            select new StaffingDepartmentDetails
                            {
                                Title = grouped.Key.Title,
                                TotalHours = grouped.Sum(x => x.t.THours).ToString(),
                                FacilityName = grouped.Key.FacilityName,
                                StaffCount = Context.Employee.Count(e => e.FacilityId == FacilityId && e.JobTitleCode == grouped.Key.Id
                                && e.IsActive).ToString(),
                                Exempt = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.Exempt && e.FacilityId == FacilityId
                                && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString(),
                                NonExempt = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.NonExempt && e.FacilityId == FacilityId
                                && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString(),
                                Contractors = Context.Employee.Count(e => e.PayTypeCode == (int)PayTypeCode.Contractor && e.FacilityId == FacilityId
                                && e.JobTitleCode == grouped.Key.Id && e.IsActive).ToString()
                            }).ToList();

                dataTable.Columns.Clear();
                dataTable.Columns.Add(new DataColumn("Title"));
                dataTable.Columns.Add(new DataColumn("FacilityName"));
                dataTable.Columns.Add(new DataColumn("TotalHours"));
                dataTable.Columns.Add(new DataColumn("StaffCount"));
                dataTable.Columns.Add(new DataColumn("Exempt"));
                dataTable.Columns.Add(new DataColumn("NonExempt"));
                dataTable.Columns.Add(new DataColumn("Contractors"));
                foreach (var item in data)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["Title"] = item.Title;
                    dataRow["FacilityName"] = item.FacilityName;
                    dataRow["TotalHours"] = item.TotalHours;
                    dataRow["StaffCount"] = item.StaffCount;
                    dataRow["Exempt"] = item.Exempt;
                    dataRow["NonExempt"] = item.NonExempt;
                    dataRow["Contractors"] = item.Contractors;
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetStaffingDepartmentDetailsForCsv:{ErrorMsg}", ex.Message);
            }
            return dataTable;
        }

        public async Task<NursingHomeData> GenerateSubmitXMLFile(GenerateXml generateXml)
        {
            NursingHomeData nursing = new NursingHomeData();
            try
            {
                var headerData = await Context.Facilities.Where(x => x.IsActive && x.Id == generateXml.FacilityId).Select(s => new { FacilityID = s.FacilityID, State = s.State, FacilityName = s.FacilityName }).FirstOrDefaultAsync();
                //Header setup
                if (headerData != null)
                {
                    nursing.Header.StateCode = headerData.State;
                    nursing.Header.FacilityId = headerData.FacilityID;
                    nursing.Header.ReportQuarter = generateXml.ReportQuarter;
                    nursing.Header.FederalFiscalYear = generateXml.Year;
                    nursing.Header.SoftwareVendorName = "Think Anew";
                    nursing.Header.SoftwareVendorEmail = "sales@pbjsnap.com";
                    nursing.Header.SoftwareProductName = "PBJSNAP";
                    nursing.Header.SoftwareProductVersion = "1.4.2";
                }
                var Timesheetdata = Context.Timesheet.Where(t => t.IsActive && t.Status == (int)TimesheetStatus.Approved && t.FacilityId == generateXml.FacilityId && t.ReportQuarter == generateXml.ReportQuarter
                && t.Year == generateXml.Year).Select(s => new
                {
                    EmployeeId = s.EmployeeId,
                    JobTitleCode = s.JobTitleCode,
                    PayTypeCode = s.PayTypeCode,
                    Hours = s.THours,
                    WorkDay = s.Workday
                });
                nursing.StaffingHours = new List<StaffHours>();
                if (Timesheetdata.Any())
                {
                    nursing.Employees = new List<EmployeeIdData>();
                    //Employee ID setup
                    nursing.Employees = Timesheetdata.Select(s => new EmployeeIdData { EmployeeId = s.EmployeeId }).Distinct().ToList();

                    foreach (var item in nursing.Employees)
                    {
                        StaffHours staffHours = new StaffHours();
                        var distinctWorkdaysList = Timesheetdata.Where(e => e.EmployeeId == item.EmployeeId).Select(s => new { Workdays = s.WorkDay }).Distinct().ToList();
                        WorkDays workDays = new WorkDays();
                        workDays.WorkDayList = new List<WorkDay>();
                        foreach (var distinctWorkday in distinctWorkdaysList)
                        {
                            var data = Timesheetdata.Where(x => x.EmployeeId == item.EmployeeId && x.WorkDay == distinctWorkday.Workdays).Select(s => new { Hours = Convert.ToDouble(s.Hours), PayTypeCode = s.PayTypeCode, JobTitleCode = s.JobTitleCode, workday = s.WorkDay }).ToList();
                            WorkDay work = new WorkDay();
                            if (data.Any())
                            {
                                HourEntries hourEntries = new HourEntries();
                                hourEntries.HourEntry = new List<HourEntry>();
                                foreach (var d in data)
                                {
                                    HourEntry hour = new HourEntry();
                                    hour.Hours = Convert.ToDouble(d.Hours);
                                    hour.PayTypeCode = Convert.ToInt32(d.PayTypeCode);
                                    hour.JobTitleCode = Convert.ToInt32(d.JobTitleCode);
                                    hourEntries.HourEntry.Add(hour);
                                }
                                work.Date = distinctWorkday.Workdays;
                                work.HourEntries = hourEntries;
                                workDays.WorkDayList.Add(work);
                            }
                        }
                        staffHours.WorkDays = workDays;
                        staffHours.EmployeeId = item.EmployeeId;
                        nursing.StaffingHours.Add(staffHours);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GenerateSubmitXMLFile:{ErrorMsg}", ex.Message);
            }
            return nursing;
        }

        public async Task<ReportVerificationViewModel> GetReportValidationStatus(int facilityId, int reportQuarter, int year)
        {
            ReportVerificationViewModel reportVerificationViewModel = new ReportVerificationViewModel();
            try
            {
                var reportValidationInfo = (from rv in Context.ReportVerification
                                            join u in Context.Users on rv.ValidationUserId equals u.Id
                                            where rv.ValidationStatus == "Validated" && rv.FacilityId == facilityId && rv.Quarter == reportQuarter && rv.Year == year
                                            select new
                                            {
                                                FirstName = u.FirstName,
                                                LastName = u.LastName,
                                                ValidationDate = rv.ValidationDate
                                            }).OrderByDescending(d => d.ValidationDate).FirstOrDefault();

                if (reportValidationInfo != null)
                {
                    reportVerificationViewModel.ValidatedBy = reportValidationInfo.FirstName + " " + reportValidationInfo.LastName;
                    reportVerificationViewModel.ValidationDate = (reportValidationInfo.ValidationDate != DateTime.MinValue) ? reportValidationInfo.ValidationDate.ToString("MM-dd-yyyy HH:mm:ss") + " UTC" : "";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetReportValidationStatus:{ErrorMsg}", ex.Message);
            }

            return reportVerificationViewModel;
        }

        public async Task<ReportVerificationViewModel> GetReportApprovedStatus(int facilityId, int reportQuarter, int year)
        {
            ReportVerificationViewModel reportVerificationViewModel = new ReportVerificationViewModel();
            try
            {
                var reportValidationInfo = (from rv in Context.ReportVerification
                                            join u in Context.Users on rv.ApprovedByUserId equals u.Id
                                            where rv.ApprovedStatus == "Approved" && rv.FacilityId == facilityId && rv.Quarter == reportQuarter && rv.Year == year
                                            select new
                                            {
                                                FirstName = u.FirstName,
                                                LastName = u.LastName,
                                                ApprovedDate = rv.ApprovedDate
                                            }).OrderByDescending(d => d.ApprovedDate).FirstOrDefault();

                if (reportValidationInfo != null)
                {
                    reportVerificationViewModel.ApprovedBy = reportValidationInfo.FirstName + " " + reportValidationInfo.LastName;
                    reportVerificationViewModel.ApprovedDate = (reportValidationInfo.ApprovedDate != DateTime.MinValue) ? reportValidationInfo.ApprovedDate.ToString("MM-dd-yyyy HH:mm:ss") + " UTC" : "";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetReportApprovedStatus:{ErrorMsg}", ex.Message);
            }

            return reportVerificationViewModel;
        }

        public async Task<bool> SaveValidatedTimesheetData(int facilityId, int reportQuarter, int year, int month, string userId)
        {
            try
            {
                ReportVerification reportVerification = new();
                reportVerification.ValidationDate = DateTime.UtcNow;
                reportVerification.ValidationUserId = userId;
                reportVerification.FacilityId = facilityId;
                reportVerification.Year = year;
                reportVerification.Quarter = reportQuarter;
                reportVerification.Month = 0;
                reportVerification.CreatedDate = DateTime.UtcNow;
                reportVerification.CreatedBy = userId;
                reportVerification.IsActive = true;
                reportVerification.ValidationStatus = "Validated";

                Context.ReportVerification.Add(reportVerification);
                await Context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured SaveValidatedTimesheetData :{ErrorMsg}", ex.Message);
                return false;
            }
        }

        public async Task<StaffExportList> GetStaffExportData(EmployeeExportRequest request)
        {
            StaffExportList staff = new StaffExportList();
            staff.EmployeeData = new List<EmployeeData>();
            try
            {

                List<string> payTypeCode = request.PayType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> jobTitleCode = request.JobTitle.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> employeeIds = new List<string>();
                if (request.EmployeeIds != null)
                {
                    employeeIds = request.EmployeeIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                staff.staffExports = (from e in Context.Employee
                                      join t in Context.Timesheet on new { e.EmployeeId, e.JobTitleCode, e.PayTypeCode } equals new { t.EmployeeId, t.JobTitleCode, t.PayTypeCode } into timeJoin
                                      from t in timeJoin.DefaultIfEmpty()
                                      join ptc in Context.PayTypeCodes on e.PayTypeCode equals ptc.PayTypeCode
                                      join ptc1 in Context.PayTypeCodes on e.PayTypeCode equals ptc1.PayTypeCode
                                      join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                      join jc1 in Context.JobCodes on e.JobTitleCode equals jc1.Id
                                      where e.IsActive && t.IsActive && t.ReportQuarter == Convert.ToInt32(request.Quarter) &&
                                      (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000" ||
                                    e.TerminationDate > DateTime.Now || (e.TerminationDate >= request.StartDate && e.TerminationDate <= request.EndDate))
                                      && t.Year == Convert.ToInt32(request.Year) && e.FacilityId == Convert.ToInt32(request.FacilityId)
                                      && jobTitleCode.Contains(e.JobTitleCode.ToString()) &&
                                      payTypeCode.Contains(e.PayTypeCode.ToString())
                                      select new StaffExport
                                      {
                                          EmployeeId = e.EmployeeId,
                                          FirstName = (e.FirstName == null) ? "" : e.FirstName.ToString(),
                                          LastName = (e.LastName == null) ? "" : e.LastName.ToString(),
                                          HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.HireDate.ToString("MM-dd-yyyy"),
                                          TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                          PayTypeDescription = ptc.PayTypeDescription,
                                          JobTitle = jc.Title,
                                          Workday = t.Workday.ToString("MM-dd-yyyy"),
                                          THours = t.THours.ToString(),
                                          HoursPayType = ptc1.PayTypeDescription,
                                          HoursJobTitle = jc1.Title
                                      }).ToList();

                if (employeeIds.Count > 0)
                {
                    staff.staffExports = (from e in staff.staffExports
                                          where employeeIds.Contains(e.EmployeeId)
                                          select e).ToList();
                }
                staff.EmployeeData = staff.staffExports.GroupBy(e => e.EmployeeId).Select(g => g.First()).Select(s => new EmployeeData
                {
                    EmployeeId = s.EmployeeId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    HireDate = s.HireDate,
                    TerminationDate = s.TerminationDate,
                    PayType = s.PayTypeDescription,
                    JobTitle = s.JobTitle
                }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GenerateSubmitXMLFile:{ErrorMsg}", ex.Message);
            }
            return staff;
        }

        public async Task<ApproveTimesheetResponse> ValidateTimesheetData(ApproveTimesheetRequest approve)
        {
            ApproveTimesheetResponse response = new ApproveTimesheetResponse();
            try
            {
                //Validate Timesheet Data 
                var data = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == approve.FacilityId
                && x.Year == approve.Year && x.ReportQuarter == approve.ReportQuarter && x.THours != 0
                && (x.Status == (int)TimesheetStatus.New || x.Status == (int)TimesheetStatus.NextDay)).ToListAsync();
                if (data.Count > 0)
                {
                    List<Timesheet> timesheetsList = new List<Timesheet>();
                    foreach (var item in data)
                    {
                        item.Status = (int)TimesheetStatus.Validate;
                        item.UpdateDate = DateTime.Now;
                        item.UpdateBy = approve.UserId;
                        timesheetsList.Add(item);
                    }
                    if (timesheetsList.Count > 0)
                    {
                        await SaveValidatedTimesheetData(approve.FacilityId, approve.ReportQuarter, approve.Year, approve.Month, approve.UserId);
                        Context.UpdateRange(timesheetsList);
                        await Context.SaveChangesAsync();
                        response.Success = true;
                        response.Message = "Record validated successfully";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ValidateTimesheetData :{ErrorMsg}", ex.Message);
                response.Success = false;
                response.Message = "Failed to validate the data";
                return response;
            }
            response.Success = false;
            response.Message = "No new record(s) found to validate";
            return response;
        }

        public async Task<bool> CheckAllValidatedTimesheet(ApproveTimesheetRequest approve)
        {
            try
            {
                var data = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == approve.FacilityId
                && x.Year == approve.Year && x.ReportQuarter == approve.ReportQuarter && x.THours != 0
                && (x.Status == (int)TimesheetStatus.New || x.Status == (int)TimesheetStatus.NextDay)).ToListAsync();
                if (data.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ApproveTimesheetData :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<IEnumerable<ReportVerificationViewModel>> GetAllReportValidationStatus(int facilityId, int reportQuarter, int year)
        {
            List<ReportVerificationViewModel> reportVerificationList = new List<ReportVerificationViewModel>();
            try
            {
                var reportValidationInfos = Context.ReportVerification
    .Join(Context.Users, rv => rv.ValidationUserId, u => u.Id, (rv, u) => new { rv, u })
    .Where(rvu => rvu.rv.FacilityId == facilityId && rvu.rv.Year == year && rvu.rv.Quarter == reportQuarter && rvu.rv.ValidationStatus == "Validated")
    .OrderByDescending(rvu => rvu.rv.ValidationDate)
    .Select(rvu => new
    {
        FirstName = rvu.u.FirstName,
        LastName = rvu.u.LastName,
        ValidationDate = rvu.rv.ValidationDate
    });

                if (reportValidationInfos != null)
                {
                    foreach (var validationInfo in reportValidationInfos)
                    {
                        ReportVerificationViewModel reportVerificationViewModel = new();
                        reportVerificationViewModel.ValidatedBy = validationInfo.FirstName + " " + validationInfo.LastName;
                        reportVerificationViewModel.ValidationDate = (validationInfo.ValidationDate != DateTime.MinValue) ? validationInfo.ValidationDate.ToString("MM-dd-yyyy HH:mm:ss") + " UTC" : "";

                        reportVerificationList.Add(reportVerificationViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetAllValidationStatus:{ErrorMsg}", ex.Message);
            }

            return reportVerificationList;
        }

        public async Task<IEnumerable<ReportVerificationViewModel>> GetAllReportApprovedStatus(int facilityId, int reportQuarter, int year)
        {
            List<ReportVerificationViewModel> reportVerificationList = new List<ReportVerificationViewModel>();
            try
            {
                var reportApprovedInfos = Context.ReportVerification
    .Join(Context.Users, rv => rv.ApprovedByUserId, u => u.Id, (rv, u) => new { rv, u })
    .Where(rvu => rvu.rv.FacilityId == facilityId && rvu.rv.Year == year && rvu.rv.Quarter == reportQuarter && rvu.rv.ApprovedStatus == "Approved")
    .OrderByDescending(rvu => rvu.rv.ApprovedDate)
    .Select(rvu => new
    {
        FirstName = rvu.u.FirstName,
        LastName = rvu.u.LastName,
        ApprovedDate = rvu.rv.ApprovedDate
    });

                if (reportApprovedInfos != null)
                {
                    foreach (var approvedInfo in reportApprovedInfos)
                    {
                        ReportVerificationViewModel reportVerificationViewModel = new();
                        reportVerificationViewModel.ApprovedBy = approvedInfo.FirstName + " " + approvedInfo.LastName;
                        reportVerificationViewModel.ApprovedDate = (approvedInfo.ApprovedDate != DateTime.MinValue) ? approvedInfo.ApprovedDate.ToString("MM-dd-yyyy HH:mm:ss") + " UTC" : "";

                        reportVerificationList.Add(reportVerificationViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetAllApprovedStatus:{ErrorMsg}", ex.Message);
            }

            return reportVerificationList;
        }

        public async Task<bool> DeleteTimesheetRecord(DeleteTimesheetDataRequest request)
        {
            try
            {
                if (request != null)
                {
                    Context.Timesheet.Where(x => x.IsActive && x.Id == Convert.ToInt32(request.TimesheetId))
                        .ExecuteUpdate(e => e.SetProperty(s => s.IsActive, s => !s.IsActive)
                        .SetProperty(s => s.UpdateBy, s => request.UserID)
                        .SetProperty(e => e.UpdateDate, e => DateTime.Now));
                    await Context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<List<Timesheet>> GetEmpTimesheetDataList(EmployeeTimesheetDataRequest dataRequest)
        {
            List<Timesheet> timesheet = new List<Timesheet>();
            try
            {
                timesheet = await Context.Timesheet.Where(x => x.IsActive && x.FacilityId == dataRequest.FacilityId && x.EmployeeId == dataRequest.EmployeeId &&
                (dataRequest.TimesheetId == null || x.Id == Convert.ToInt32(dataRequest.TimesheetId))
                && x.Year == dataRequest.Year && x.Workday == Convert.ToDateTime(dataRequest.Workday)).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmpTimesheetDataList :{ErrorMsg}", ex.Message);
            }
            return timesheet;
        }
    }
}