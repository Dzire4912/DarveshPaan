using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;

namespace TAN.Repository.Abstractions
{
    public interface ITimesheetRepository : IRepository<Timesheet>
    {
        Task<List<TimesheetEmployeeDataResponse>> GetWorkdayDetails(TimesheetEmployeeDataRequest timeReq);
        Task<bool> InsertTimesheet(List<Timesheet> timesheet);
        List<StaffingDepartmentDetails> GetStaffingDepartmentDetails(StaffingDepartmentCsvRequest csvRequest, int PageSize, int PageNo, out int TotalCount, string search, string sortOrder);
        Task<List<EmployeeTimesheetData>> GetEmployeeTimesheetData(EmployeeTimesheetDataRequest timesheetDataRequest);
        Task<bool> AddTimesheet(Timesheet timesheet);
        Task<Timesheet> GetEmpTimesheetData(EmployeeTimesheetDataRequest timesheetDataRequest);
        Task<ApproveTimesheetResponse> ApproveTimesheetData(ApproveTimesheetRequest approve);
        Task<List<DateTime>> GetMissingDatesByJobTitle(int FacilityId, DateTime StartDate, DateTime EndDate, int JobTitle);
        DataTable GetStaffingDepartmentDetailsForCsv(int FacilityId, int ReportQuarter, int year);
        Task<NursingHomeData> GenerateSubmitXMLFile(GenerateXml generateXml);
        Task<StaffExportList> GetStaffExportData(EmployeeExportRequest request);
        Task<ApproveTimesheetResponse> ValidateTimesheetData(ApproveTimesheetRequest approve);
        Task<ReportVerificationViewModel> GetReportValidationStatus(int facilityId, int reportQuarter, int year);
        Task<ReportVerificationViewModel> GetReportApprovedStatus(int facilityId, int reportQuarter, int year);
        Task<bool> CheckAllValidatedTimesheet(ApproveTimesheetRequest approve);
        Task<IEnumerable<ReportVerificationViewModel>> GetAllReportValidationStatus(int facilityId, int reportQuarter, int year);
        Task<IEnumerable<ReportVerificationViewModel>> GetAllReportApprovedStatus(int facilityId, int reportQuarter, int year);
        Task<bool> DeleteTimesheetRecord(DeleteTimesheetDataRequest request);
        Task<List<Timesheet>> GetEmpTimesheetDataList(EmployeeTimesheetDataRequest dataRequest);
    }
}
