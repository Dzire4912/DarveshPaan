using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext db;
        private readonly IHttpContextAccessor _httpContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        public UnitOfWork(DatabaseContext _db, IHttpContextAccessor httpContext, UserManager<AspNetUser> userManager)
        {
            db = _db;
            _httpContext = httpContext;
            _UserManager = userManager;

        }

        private IModuleRepository _ModuleRepo;
        private IModulePermissionRepository _ModulePermissionRepo;
        private IPermissionsRepository _PermissionsRepo;
        private IApplicationRepository _ApplicationRepo;
        private ITANEmployeeRepository _TANEmployeeRepo;
        private IUserApplicationRepository _UserApplicationRepo;
        private IAgencyRepository _AgencyRepo;
        private IUserService _UserServiceRepo;
        private IFacilityRepository _FacilityRepo;
        private IOrganizationRepository _OrganizationRepo;

        private IAspNetUserRoleRepository _AspNetUserRoleRepo;
        private IUserType _UserTypeRepo;
        private IAspNetRolesRepository _AspNetRolesRepo;
        private IFacilityApplicationRepository _FacilityApplicationRepo;
        private IUploadFileDetails _UploadFileDetails;
        private IUserMappingFieldData _UserMappingFieldData;
        private ICmsMapFieldData _CmsMapFieldDataRepo;
        private ITimesheetRepository _TimesheetRepo;
        private IFileErrorRecordsRepository _FileErrorRecordsRepo;
        private IStateRepository _stateRepo;
        private ICensus _CensusRepo;
        private IPayTypeCodes _PayTypeCodesRepo;
        private IEmployee _EmployeeRepo;
        private IUserOrganizationFacilitiesRepository _userOrganizationFacilitiesRepo;
        private IJobCodes _JobCodesRepo;
        private IHistory _HistoryRepo;
        private IUploadHistoryRepository _UploadHistoryRepo;
        public ILoginAudit _AuditLogs;
        private IKronosPunchRepository _KronosPunchRepository;
        private IKronosPbjMapping _kronosPbjMapRepository;
        private IkronosFacility _kronosFacility;
        private IkronosDataTracking _kronosDataTrack;
        private IKronosBlobCredsRepository _kronosBlobCredsRepos;
        private IKronosSftpCredsRepository _KronosSftpCredsRepos;
        private IKronoFileEndPointsRepository _kronosFileEndPointRepos;
        private IKronosMappedRepository _kronosmapped;
        private IKronosSpResults _spresult;
        private IKronosPendingRecords _KronosPendingRecordsRepo;
        private IMonths _MonthsRepo;
        private IOneTimeDeduction _oneTimeDeductionRepo;
        private IBreakDeduction _breakDeduction;
        private IExecuteSp _exeExecuteSp;
        private IKronosUnprocessed _kronosUnproccesdRepo;
        public ICalls _BandwidthCallEventsRepo;
        public IBandwidthCallLogs _BandwidthCallLogsRepo;
        public IOrganizationServiceRepository _OrganizationServiceRepo;
        private IDataServiceType _DataServiceTypeRepo;
        private IDataService _DataServiceRepo;
        public IUploadClientFileDetailsRepository _UploadClientFileDetailsRepo;
        public ICarrierInformation _CarrierInformationRepo;
        public IInvoiceData _InvoiceDataRepo;

        public IKronosUnprocessed KronosUnprocessedRepo
        {
            get
            {
                if (_kronosUnproccesdRepo == null)
                    _kronosUnproccesdRepo = new KronosUnprocessedRepository(db);

                return _kronosUnproccesdRepo;
            }
        }
        public IExecuteSp executeSpRepo
        {
            get
            {
                if (_exeExecuteSp == null)
                    _exeExecuteSp = new ExecuteSpRepository(db);

                return _exeExecuteSp;
            }
        }

        public IModuleRepository ModuleRepo
        {
            get
            {
                if (_ModuleRepo == null)
                    _ModuleRepo = new ModuleRepository(db);

                return _ModuleRepo;
            }
        }
        public IModulePermissionRepository ModulePermissionRepo
        {
            get
            {
                if (_ModulePermissionRepo == null)
                    _ModulePermissionRepo = new ModulePermissionRepository(db);

                return _ModulePermissionRepo;
            }
        }
        public IPermissionsRepository PermissionsRepo
        {
            get
            {
                if (_PermissionsRepo == null)
                    _PermissionsRepo = new PermissionRepository(db);

                return _PermissionsRepo;
            }
        }
        public IApplicationRepository ApplicationRepo
        {
            get
            {
                if (_ApplicationRepo == null)
                    _ApplicationRepo = new ApplicationRepository(db);

                return _ApplicationRepo;
            }
        }
        public ITANEmployeeRepository TANEmployeeRepo
        {
            get
            {
                if (_TANEmployeeRepo == null)
                    _TANEmployeeRepo = new TANEmployeeRepository(db);

                return _TANEmployeeRepo;
            }
        }
        public IUserApplicationRepository UserApplicationRepo
        {
            get
            {
                if (_UserApplicationRepo == null)
                    _UserApplicationRepo = new UserApplicationRepository(db);

                return _UserApplicationRepo;
            }
        }

        public IOrganizationRepository OrganizationRepo
        {
            get
            {
                if (_OrganizationRepo == null)
                    _OrganizationRepo = new OrganizationRepository(db, _httpContext, _UserManager);

                return _OrganizationRepo;
            }
        }

        public IFacilityRepository FacilityRepo
        {
            get
            {
                if (_FacilityRepo == null)
                    _FacilityRepo = new FacilityRepository(db, _httpContext, _UserManager);

                return _FacilityRepo;
            }
        }

        public IAgencyRepository AgencyRepo
        {
            get
            {
                if (_AgencyRepo == null)
                    _AgencyRepo = new AgencyRepository(db, _httpContext);

                return _AgencyRepo;
            }
        }

        public IUserService UserServiceRepo
        {
            get
            {
                if (_UserServiceRepo == null)
                    _UserServiceRepo = new UserService(_httpContext);
                return _UserServiceRepo;
            }
        }

        public IStateRepository StateRepo
        {
            get
            {
                if (_stateRepo == null)
                    _stateRepo = new StateRepository(db);
                return _stateRepo;
            }
        }

        public IAspNetUserRoleRepository AspNetUserRoleRepo
        {
            get
            {
                if (_AspNetUserRoleRepo == null)
                    _AspNetUserRoleRepo = new AspNetUserRoleRepository(db);
                return _AspNetUserRoleRepo;
            }
        }

        public IUserType UserTypeRepo
        {
            get
            {
                if (_UserTypeRepo == null)
                    _UserTypeRepo = new UserTypeRepository(db);
                return _UserTypeRepo;
            }
        }

        public IFacilityApplicationRepository FacilityApplicationRepo
        {
            get
            {
                if (_FacilityApplicationRepo == null)
                    _FacilityApplicationRepo = new FacilityApplicationRepository(db);
                return _FacilityApplicationRepo;
            }
        }
        public IAspNetRolesRepository AspNetRolesRepo
        {
            get
            {
                if (_AspNetRolesRepo == null)
                    _AspNetRolesRepo = new AspNetRolesRepository(db);
                return _AspNetRolesRepo;
            }
        }


        public IUploadFileDetails UploadFileDetailsRepo
        {
            get
            {
                if (_UploadFileDetails == null)
                    _UploadFileDetails = new UploadFileDetailsRepository(db, _httpContext);

                return _UploadFileDetails;
            }
        }

        public IUserMappingFieldData UserMappingFieldData
        {
            get
            {
                if (_UserMappingFieldData == null)
                    _UserMappingFieldData = new UserMappingFieldDataRepository(db);

                return _UserMappingFieldData;
            }
        }

        public ICmsMapFieldData CmsMapFieldDataRepo
        {
            get
            {
                if (_CmsMapFieldDataRepo == null)
                    _CmsMapFieldDataRepo = new CmsMapFieldDataRepository(db);

                return _CmsMapFieldDataRepo;
            }
        }

        public ITimesheetRepository TimesheetRepo
        {
            get
            {
                if (_TimesheetRepo == null)
                    _TimesheetRepo = new TimesheetRepository(db);

                return _TimesheetRepo;
            }
        }

        public IFileErrorRecordsRepository FileErrorRecordsRepo
        {
            get
            {
                if (_FileErrorRecordsRepo == null)
                    _FileErrorRecordsRepo = new FileErrorRecordsRepository(db);

                return _FileErrorRecordsRepo;
            }
        }

        public IPayTypeCodes PayTypeCodesRepo
        {
            get
            {
                if (_PayTypeCodesRepo == null)
                    _PayTypeCodesRepo = new PayTypeCodesRepository(db);

                return _PayTypeCodesRepo;
            }
        }

        public ICensus CensusRepo
        {
            get
            {
                if (_CensusRepo == null)
                    _CensusRepo = new CensusRepository(db, _httpContext);

                return _CensusRepo;
            }
        }

        public IEmployee EmployeeRepo
        {
            get
            {
                if (_EmployeeRepo == null)
                    _EmployeeRepo = new EmployeeRepository(db, _httpContext);

                return _EmployeeRepo;
            }
        }
        public IUserOrganizationFacilitiesRepository UserOrganizationFacilitiesRepo
        {
            get
            {
                if (_userOrganizationFacilitiesRepo == null)
                    _userOrganizationFacilitiesRepo = new UserOrganizationFacilitiesRepository(db);
                return _userOrganizationFacilitiesRepo;
            }
        }

        public IJobCodes JobCodesRepo
        {
            get
            {
                if (_JobCodesRepo == null)
                    _JobCodesRepo = new JobCodesRepository(db);
                return _JobCodesRepo;
            }
        }

        public IHistory HistoryRepo
        {
            get
            {
                if (_HistoryRepo == null)
                    _HistoryRepo = new HistoryRepository(db, _httpContext);
                return _HistoryRepo;
            }
        }

        public IUploadHistoryRepository UploadHistoryRepo
        {
            get
            {
                if (_UploadHistoryRepo == null)
                    _UploadHistoryRepo = new UploadHistoryRepository(db, _httpContext);
                return _UploadHistoryRepo;
            }
        }

        public ILoginAudit loginAuditRepo
        {
            get
            {
                if (_AuditLogs == null)
                    _AuditLogs = new LoginAuditRepo(db);
                return _AuditLogs;
            }
        }

        public ICalls BandwidthCallEventsRepo
        {
            get
            {
                if (_BandwidthCallEventsRepo == null)
                    _BandwidthCallEventsRepo = new CallsRepository(db, _httpContext, _UserManager);
                return _BandwidthCallEventsRepo;
            }
        }

        public IBandwidthCallLogs BandwidthCallLogsRepo
        {
            get
            {
                if (_BandwidthCallLogsRepo == null)
                    _BandwidthCallLogsRepo = new BandwidthCallLogsRepo(db, _httpContext);
                return _BandwidthCallLogsRepo;
            }
        }

        public IOrganizationServiceRepository OrganizationServiceRepo
        {
            get
            {
                if (_OrganizationServiceRepo == null)
                    _OrganizationServiceRepo = new OrganizationServiceRepository(db);
                return _OrganizationServiceRepo;
            }
        }
        public IDataServiceType DataServiceTypeRepo
        {
            get
            {
                if (_DataServiceTypeRepo == null)
                    _DataServiceTypeRepo = new DataServiceTypeRepository(db, _httpContext);
                return _DataServiceTypeRepo;
            }
        }

        public IDataService DataServiceRepo
        {
            get
            {
                if (_DataServiceRepo == null)
                    _DataServiceRepo = new DataServiceRepository(db, _httpContext);
                return _DataServiceRepo;
            }
        }
        public IUploadClientFileDetailsRepository UploadClientFileDetailsRepo
        {
            get
            {
                if (_UploadClientFileDetailsRepo == null)
                    _UploadClientFileDetailsRepo = new UploadClientFileDetailsRepository(db, _httpContext);
                return _UploadClientFileDetailsRepo;
            }
        }
        public IKronosPunchRepository KronosPunchRepo
        {
            get
            {
                if (_KronosPunchRepository == null)
                    _KronosPunchRepository = new KronosPunchRepository(db);
                return _KronosPunchRepository;

            }
        }

        public IOneTimeDeduction OneTimeDeductionRepo
        {
            get
            {
                if (_oneTimeDeductionRepo == null)
                    _oneTimeDeductionRepo = new OneTimeUpdateRepository(db);
                return _oneTimeDeductionRepo;
            }
        }
        public IMonths MonthsRepo
        {
            get
            {
                if (_MonthsRepo == null)
                    _MonthsRepo = new MonthsRepository(db);
                return _MonthsRepo;
            }
        }
        public IKronosPendingRecords KronosPendingRecordsRepo
        {
            get
            {
                if (_KronosPendingRecordsRepo == null)
                    _KronosPendingRecordsRepo = new KronosPendingRecordsRepository(db);
                return _KronosPendingRecordsRepo;
            }
        }

        public IKronosSpResults kronosSpResultsRepo
        {
            get
            {
                if (_spresult == null)
                    _spresult = new KronosSpResultRepository(db);
                return _spresult;
            }
        }

        public IKronosMappedRepository kronosMappedRepo
        {
            get
            {
                if (_kronosmapped == null)
                    _kronosmapped = new KronosMappedRepository(db);
                return _kronosmapped;
            }
        }
        public IKronosBlobCredsRepository kronosBlobCredsRepo
        {
            get
            {
                if (_kronosBlobCredsRepos == null)
                    _kronosBlobCredsRepos = new KronosBlobRepository(db);

                return _kronosBlobCredsRepos;
            }
        }
        public IKronosSftpCredsRepository KronosSftpCredsRepo
        {
            get
            {
                if (_KronosSftpCredsRepos == null)
                    _KronosSftpCredsRepos = new KronosSftpCredsRepository(db);
                return _KronosSftpCredsRepos;
            }
        }
        public IKronoFileEndPointsRepository kronosFileEndPointRepo
        {
            get
            {
                if (_kronosFileEndPointRepos == null)
                    _kronosFileEndPointRepos = new KronosFileEndPointsRepository(db);
                return _kronosFileEndPointRepos;
            }
        }

        public IkronosDataTracking kronosDataTrackRepo
        {
            get
            {
                if (_kronosDataTrack == null)
                    _kronosDataTrack = new KronosDataTrackingRepository(db);

                return _kronosDataTrack;
            }
        }

        public IKronosPbjMapping kronosPbjMapRepo
        {
            get
            {
                if (_kronosPbjMapRepository == null)
                    _kronosPbjMapRepository = new KronosPbjMapping(db);

                return _kronosPbjMapRepository;
            }
        }
        public IkronosFacility kronosfacilitRepo
        {
            get
            {
                if (_kronosFacility == null)
                    _kronosFacility = new KronosFacilityRepository(db);

                return _kronosFacility;
            }
        }
        public IBreakDeduction BreakDeductionRepo
        {
            get
            {
                if (_breakDeduction == null)
                    _breakDeduction = new BreakDeductionRepository(db);

                return _breakDeduction;
            }
        }

        public ICarrierInformation CarrierInformationRepo
        {
            get
            {
                if (_CarrierInformationRepo == null)
                    _CarrierInformationRepo = new CarrierInfoRepo(db, _httpContext);
                return _CarrierInformationRepo;
            }
        }

        public IInvoiceData InvoiceDataRepo
        {
            get
            {
                if (_InvoiceDataRepo == null)
                    _InvoiceDataRepo = new InvoiceDataRepository(db, _httpContext, _UserManager);
                return _InvoiceDataRepo;
            }
        }

        #region transaction


        /// <summary>
        /// When every transaction completed commit is called.
        /// </summary>
        public void Commit()
        {
            bool saveFailed = false;
            do
            {
                saveFailed = false;
                try
                {
                    db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;


                }
            }
            while (saveFailed);
        }



        /// <summary>
        /// Dispose is called to clear out memory
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        public IDatabaseTransaction BeginTransaction()
        {
            return new EntityDatabaseTransaction(db);
        }
        /* public async Task<int>SaveChanges()
         {            
             return await db.SaveChangesAsync();
         }
 */
        public int SaveChanges()
        {
            return db.SaveChanges();
        }

        #endregion

    }
}
