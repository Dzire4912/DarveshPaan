using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TAN.DomainModels;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;

namespace TAN.ApplicationCore
{
    [ExcludeFromCodeCoverage]
    public class DatabaseContext : IdentityDbContext<AspNetUser, AspNetRoles, string, IdentityUserClaim<string>, AspNetUserRoles, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly string _userId;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, IHttpContextAccessor httpContext) : base(options)
        {
            // Get the claims principal from the HttpContext
            var claimsPrincipal = httpContext.HttpContext?.User;

            if (claimsPrincipal != null && claimsPrincipal.Identity.IsAuthenticated)
            {
                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {   // Get the user ID from the claim value
                    _userId = userIdClaim.Value;
                }
                else
                {
                    _userId = null;
                }
            }
            else
            {
                _userId = null;
            }

        }
        public DbSet<AspNetRoles> AspNetRoles { get; set; }
        public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<ModulePermission> ModulePermissions { get; set; }
        public DbSet<RolePermissionsViewModel> RolePermissions { get; set; }
        public DbSet<Application> Application { get; set; }
        public DbSet<UserApplication> UserApplications { get; set; }
        public DbSet<TANEmployeeDetails> TANEmployeeDetails { get; set; }
        public DbSet<OrganizationModel> Organizations { get; set; }
        public DbSet<FacilityModel> Facilities { get; set; }
        public DbSet<UserOrganizationFacility> UserOrganizationFacilities { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<StateModel> States { get; set; }
        public DbSet<AgencyModel> Agencies { get; set; }
        public DbSet<OrganizationFacilityModel> OrganizationFacilities { get; set; }
        public DbSet<FacilityApplicationModel> FacilityApplications { get; set; }
        public DbSet<UploadFileDetails> UploadFileDetails { get; set; }
        public DbSet<CMSMappingFieldData> CMSMappingFieldData { get; set; }
        public DbSet<UserMappingFieldData> UserMappingFieldData { get; set; }
        public DbSet<Timesheet> Timesheet { get; set; }
        public DbSet<FileErrorRecords> FileErrorRecords { get; set; }
        public DbSet<Census> Census { get; set; }
        public DbSet<PayTypeCodes> PayTypeCodes { get; set; }
        public DbSet<JobCodes> JobCodes { get; set; }
        public DbSet<LaborCodes> LaborCodes { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<UploadHistory> UploadHistory { get; set; }
        public DbSet<Audit> AuditLogs { get; set; }
        public DbSet<LoginAudit> LoginAuditLogs { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<Months> Months { get; set; }
        public DbSet<ReportVerification> ReportVerification { get; set; }
        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<KronosPunchExportModel> KronosPunchExport { get; set; }
        public DbSet<KronosToPbjMap> KronosToPbjMappings { get; set; }
        public DbSet<KronosPaytypeMapping> KronosPaytypeMappings { get; set; }
        public DbSet<KronosFacilities> kronosFacilities { get; set; }
        public DbSet<KronosDataTrack> KronosDataTracker { get; set; }
        public DbSet<KronosBlobStorageCreds> KronosBlobStorageCreds { get; set; }
        public DbSet<KronosSftpCredentials> kronosSftpCredentials { get; set; }
        public DbSet<KronosSftpFileEndpoints> kronosSftpFileEndpoints { get; set; }
        public DbSet<KronosTimeSheetMapped> kronosTimeSheetMapped { get; set; }
        public DbSet<KronosPendingRecords> kronosPendingRecords { get; set; }
        public virtual DbSet<KronosSpResult> KronosSpResults { get; set; }
        public DbSet<KronosOneTimeUpdate> KronosOneTimeUpdates { get; set; }        
        public DbSet<BreakDeduction> BreakDeductions { get; set; }
        public virtual DbSet<ExecutionSpResult> ExecutionSpResults { get; set; }
        public DbSet<KronosUnprocessedData> KronosPunchExportUnprocessedData { get; set; }

        
        public DbSet<BandwidthCallEvents> BandwidthCallEvents { get; set; }
        public DbSet<BandwidthMaster> BandwidthMaster { get; set; }
        public DbSet<OrganizationServices> OrganizationServices { get; set; }
        public DbSet<BandwidthCallLogs> BandwidthCallLogs { get; set; }
        public DbSet<DataService> DataService { get; set; }
        public DbSet<DataServiceType> DataServiceType { get; set; }
        public DbSet<UploadClientFileDetails> UploadClientFileDetails { get; set; }
        public DbSet<CarrierInfomation> CarrierInfomation { get; set; }
        public DbSet<InvoiceData> InvoiceData { get; set; }

        public override async Task<int> SaveChangesAsync( CancellationToken cancellationToken = default)
        {          
            var result = (dynamic)null;
            OnBeforeSaveChanges();
            try
            {
                result = await base.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //log
            }
            return result;
        }

        public override int SaveChanges()
        {
            var result = (dynamic)null;
            OnBeforeSaveChanges();
            try
            {
                result = base.SaveChanges();
            }
            catch (Exception ex)
            {
                //log
            }
            return result;
        }

        private async void OnBeforeSaveChanges(string userId = null)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            // var tablesToSkipAudit = new List<string> { "AspNetUser" }; // Add the table names to skip here

            foreach (var entry in ChangeTracker.Entries())
            {
                var tableName = entry.Metadata.GetTableName();

                if (entry.Entity is Audit || ShouldSkipAudit(entry) || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = _userId;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    //  string propertyName = property.Metadata.Name;

                    // Skip auditing certain properties marked with SkipAuditPropertyAttribute
                    if (ShouldSkipAuditProperty(entry.Entity, propertyName))
                    {
                        continue;
                    }
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }


        private bool ShouldSkipAudit(EntityEntry entry)
        {
            // Check if the entity has the SkipAudit attribute
            return entry.Entity.GetType().GetCustomAttributes(typeof(SkipAuditAttribute), false).Any();
        }
        private bool ShouldSkipAuditProperty(object entity, string propertyName)
        {
            // Check if the property has the SkipAuditProperty attribute
            var propertyInfo = entity.GetType().GetProperty(propertyName);
            return propertyInfo?.GetCustomAttributes(typeof(SkipAuditAttribute), false).Any() == true;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AspNetRoles>(builder =>
            {
                builder.ToTable("AspNetRoles");
                modelBuilder.Entity<AspNetRoles>()
               .ToTable("AspNetRoles")
               .HasKey(r => r.Id);

                modelBuilder.Entity<AspNetRoles>()
                    .HasIndex(r => r.NormalizedName)
                    .IsUnique(false);
            });

            modelBuilder.Entity<PayTypeCodes>(e => { e.HasNoKey(); });
            modelBuilder.Entity<JobCodes>(e => { e.HasNoKey(); });
            modelBuilder.Entity<LaborCodes>(e => { e.HasNoKey(); });
            modelBuilder.Entity<KronosPunchExportModel>().ToTable("KronosPunchExport");
            modelBuilder.Entity<KronosToPbjMap>().ToTable("KronosToPbj");
            modelBuilder.Entity<KronosToPbjMap>(e => { e.HasNoKey(); });
            modelBuilder.Entity<KronosPaytypeMapping>(e => { e.HasNoKey(); });
            modelBuilder.Entity<KronosSpResult>(e => { e.HasNoKey(); });
            modelBuilder.Entity<ExecutionSpResult>(e => { e.HasNoKey(); });

        }
    }

}