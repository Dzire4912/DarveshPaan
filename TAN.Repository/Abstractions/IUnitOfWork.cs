using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Abstractions
{
    public interface IUnitOfWork
    {
        IModuleRepository ModuleRepo { get; }
        IModulePermissionRepository ModulePermissionRepo { get; }
        IPermissionsRepository PermissionsRepo { get; }
        IApplicationRepository ApplicationRepo { get; }
        ITANEmployeeRepository TANEmployeeRepo { get; }
        IUserApplicationRepository UserApplicationRepo { get; }

        IOrganizationRepository OrganizationRepo { get; }
        IFacilityRepository FacilityRepo { get; }
        IAgencyRepository AgencyRepo { get; }

        IUserService UserServiceRepo { get; }
        IAspNetUserRoleRepository AspNetUserRoleRepo { get; }
        IUserType UserTypeRepo { get; }
        //IAspNetRolesRepository AspNetRolesRepo { get; }
        IFacilityApplicationRepository FacilityApplicationRepo { get; }
        IUploadFileDetails UploadFileDetailsRepo {get;} 
        ICmsMapFieldData CmsMapFieldDataRepo { get;}
        IUserMappingFieldData UserMappingFieldData { get; }
        ITimesheetRepository TimesheetRepo { get; }
        IFileErrorRecordsRepository FileErrorRecordsRepo { get; }
        IStateRepository StateRepo { get; }
        IAspNetRolesRepository  AspNetRolesRepo { get; }
        IUserOrganizationFacilitiesRepository UserOrganizationFacilitiesRepo { get; }
        ICensus CensusRepo { get; }
        IPayTypeCodes PayTypeCodesRepo { get; }
        IEmployee EmployeeRepo { get; }
        IJobCodes JobCodesRepo { get; }
        IHistory HistoryRepo { get; }
        IUploadHistoryRepository UploadHistoryRepo { get; }
        ILoginAudit loginAuditRepo { get; }
        IKronosPunchRepository KronosPunchRepo { get; }
        IkronosFacility kronosfacilitRepo { get; }
        ICalls BandwidthCallEventsRepo { get; }
        IBandwidthCallLogs BandwidthCallLogsRepo { get; }
        IOrganizationServiceRepository OrganizationServiceRepo { get; }
        IDataService DataServiceRepo { get; }
        IDataServiceType DataServiceTypeRepo { get; }
        IUploadClientFileDetailsRepository UploadClientFileDetailsRepo { get; }
        ICarrierInformation CarrierInformationRepo { get; }
        IInvoiceData InvoiceDataRepo { get; }
        void Commit();
        IDatabaseTransaction BeginTransaction();
        IKronosPbjMapping kronosPbjMapRepo { get; }
        IkronosDataTracking kronosDataTrackRepo { get; }
        IKronosBlobCredsRepository kronosBlobCredsRepo { get; }
        IKronosSftpCredsRepository KronosSftpCredsRepo { get; }
        IKronoFileEndPointsRepository kronosFileEndPointRepo { get; }
        IKronosMappedRepository kronosMappedRepo { get; }
        IKronosSpResults kronosSpResultsRepo {  get; }
        IExecuteSp executeSpRepo { get; }   
  
        IKronosPendingRecords KronosPendingRecordsRepo { get; }
        IMonths MonthsRepo { get; }
        IOneTimeDeduction OneTimeDeductionRepo {  get; }
        IBreakDeduction BreakDeductionRepo { get; }
        IKronosUnprocessed KronosUnprocessedRepo { get; }
        //Task<int> SaveChanges();
        int SaveChanges();
       

    }
}
