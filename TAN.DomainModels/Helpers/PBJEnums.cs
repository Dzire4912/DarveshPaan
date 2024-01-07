using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
    public enum Roles
    {
        [Description("SuperAdmin")]
        SuperAdmin,
        [Description("Admin")]
        Admin,
        [Description("User")]
        User

    }
    //public enum UserType
    //{
    //    [Description("Organization User")]
    //    OrganizationUser = 4001,
    //    [Description("User")]
    //    TANUser = 4002
    //}
    public enum ApplicationNameWithId
    {
        [Description("PBJSnap")]
        PBJSnap = 1,
        Inventory = 2,
        [Description("Telecom Reporting")]
        Reporting = 3

    }
    public enum AllModules
    {
        UserManagement,
        RoleManagement,
        Admin,
        Dashboard,
        Upload,
        Review,
        Submit,
        SubmissionHistory,
        Storage,
        Configuration,
        Organization,
        Facility,
        Agency,
        Stores,
        Products,
        RentalManagement,
        StockManagement,
        Inventory,
        DefectManagement,
        ShipmentManagement,
        Backout,
        EmployeeMaster,
        Reports,
        Voice,
        Data,
        DataService,
        UploadClientFile,
        Carrier,
        DataServiceType
    }

    public enum UploadWorkdayStatus
    {
        UpdateTHoursToNextDay = 1
    }
    public enum TotalHours
    {
        Zero = 0,
        Fullday = 24,
        Eight = 8,
        Sixteen = 16
    }
    public enum UploadFileFailureSuccess
    {
        FileExist = 401,
        FileExtension = 402,
        Success = 200,
        Server = 400
    }
    public enum MappingDataStatus
    {
        Success = 200,
        MultipleCMS = 201,
        FileUploadFailed = 202,
        Server = 400,
        MapProperly = 203,
        CmsValidation = 204,
        DiffQuarter = 205,
        InvalidRecord = 206
    }

    public enum UserTypes
    {
        Thinkanew = 1,
        Other = 2,
    }
    public enum IdentityRolesStatus
    {
        [Description("DuplicateRoleName")]
        DuplicateRoleName
    }

    public enum PayTypeCode
    {
        Exempt = 1,
        NonExempt = 2,
        Contractor = 3
    }

    public enum RoleTypes
    {
        [Description("TANApplicationUser")]
        TANApplicationUser
    }

    public enum AuditType
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }

    public enum LoginAuditStatus
    {
        Success = 1001,
        LockedOut = 1002,
        Verification = 1003,
        Failure = 1004
    }

    public enum TimesheetStatus
    {
        New = 0, // Current day total hours 
        NextDay = 1, //when the user out clock punch is after 12 AM.
        Validate = 2, // Ready for Approval.
        Approved = 3 // Ready for Generate XML file.
    }

    public enum BuildVersion
    {
        [Description("Build Version v1.2.0 © 2023 Think Anew")]
        version
    }

    public enum TimeInMinutes
    {
        HalfMinute = 30,
        Minute = 60,
        TwoMinutes = 120
    }

    public static class ContentType
    {
        public const string JSON = "application/json";
    }

    public enum BackoutRequestStatus
    {
        SelectAll = 0,
        Invalid = 1,
        Valid = 2
    }

    public enum ReportingQuarter
    {
        Oct_Dec = 1,
        Jan_Mar = 2,
        Apr_Jun = 3,
        Jul_Sept = 4
    }

    public enum FileErrorRecordsStatus
    {
        Valided = 0,
        Invalid = 1
    }
    public enum SPExecResult
    {
        Failed = 0,
        Success = 1,
        Error= -1,
    }
    public enum OperationResult
    {
        [Description("Success")]
        Success=1,
        [Description("InProgress")]
        InProgress = 2,
        [Description("Failed")]
        Failed=3,
        [Description("No Data Exists")]
        NoDataExists=4,
        [Description("Data Exists")]
        DataExists=5,
        [Description("Error")]
        Error
    }
    public enum KronosTrailStatus
    {
        
        [Description("InProgress")]
        InProgress = 1,
        [Description("Success")]
        Success = 2,
        [Description("Failed")]
        Failed = 3,      
    }
    public enum KronosSource
    {
        [Description("TimeSheet")]
        TimeSheet,
        [Description("Kronos")]
        Kronos,
    }
    public enum KronosTimeSheetColums
    {
        [Description("Employee Id")]
        EmployeeId,
        [Description("FacilityId")]
        FacilityId,
        [Description("Work Day")]
        WorkDay,
        [Description("Pay Type Code")]
        PayTypeCode,
        [Description("Job Title Code")]
        JobTitleCode,
        [Description("Hours")]
        Hours,
        [Description("Full Name with Last Name first")]       
        FullNameWithLastNamefirst
    }
    public enum ServiceType
    {
        Voice = 0,
        Data = 1
    }
    public enum UploadClientFilesFailureSuccess
    {
        RequestFailed = 404,
        FileExtension = 402,
        Success = 200,
        Server = 400
    }
}
