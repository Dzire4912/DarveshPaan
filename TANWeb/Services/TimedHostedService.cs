using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Areas.Inventory.Services;
using TANWeb.Interface;

namespace TANWeb.Services
{
    [ExcludeFromCodeCoverage]
    public class TimedHostedService : BackgroundService
    {
        private int _executionCount;
        private readonly SftpServices _sftpServices;
        private readonly DecryptionService _decryptionService;
        private readonly CsvReaderService _csvReaderService;
        private readonly KronosPunchExportService _kronosPunchExportService;
        private readonly IServiceProvider _serviceProvider;

        public TimedHostedService(SftpServices sftpServices, DecryptionService decryptionService, CsvReaderService csvReaderService, KronosPunchExportService kronosPunchExportService, IServiceProvider serviceProvider)
        {
            _sftpServices = sftpServices;
            _decryptionService = decryptionService;
            _csvReaderService = csvReaderService;
            _kronosPunchExportService = kronosPunchExportService;
            _serviceProvider = serviceProvider;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // When the timer should have no due-time, then do the work once now.
            DoWork();
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(20));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))

                {

                    //pls uncomment the below

                    //if (DateTime.Now.Hour == 14)
                    //{

                    DoWork();

                    // }

                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Error(ex, "An Error Occured in Kronos Scheduled BackgroundService ExecuteAsync :{ErrorMsg}", ex.Message);
            }
        }
        public async void DoWork(string? manual = null)
        {
            Log.Information("initializing {c}", "Background service");
            int count = Interlocked.Increment(ref _executionCount);
            int staticCount = 0;
            if (manual == null)
            {
                staticCount = count;

            }

            KronosDataTrack kdatalog = new KronosDataTrack();
            //Get required Scopes
            using (var outerScope = _serviceProvider.CreateScope())
            {
                var outerUnitOfWork = outerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var outerUploadService = outerScope.ServiceProvider.GetRequiredService<IUpload>();

                try
                {
                    List<KronosSftpCredentials> kronosSftpCreds = outerUnitOfWork.KronosSftpCredsRepo.GetAll().Where(x => x.IsActive).ToList();
                    List<KronosPunchExportModel> kronosPunchList = await GetAllKronosData(outerUnitOfWork, kronosSftpCreds);
                    if (kronosPunchList.Count > 0 && kronosPunchList != null)
                    {
                        var eachOrg = kronosPunchList.Select(c => c.OrganizationId).Distinct().ToList();
                        foreach (var org in eachOrg)
                        {
                            string OrgName = outerUnitOfWork.OrganizationRepo.GetAll().Where(c => c.OrganizationID == org).FirstOrDefault()?.OrganizationName;
                            List<KronosPunchExportModel> kronosPunchOrgWise = kronosPunchList.Where(c => c.OrganizationId == org).ToList();
                            DateTime minDate = (DateTime)kronosPunchOrgWise.Min(x => x.ADJUSTEDAPPLYDATE);
                            DateTime maxDate = (DateTime)kronosPunchOrgWise.Max(x => x.ADJUSTEDAPPLYDATE);
                            await AddKronosTrail(outerUnitOfWork, "NA", $"Initializing BackGround Service to pass Kronos Punch data to Timesheet for org : {OrgName}", Convert.ToInt32(KronosTrailStatus.InProgress), staticCount, 0);

                            try
                            {
                                //Get Kronos Server Data and insert into our KronosPunchExportTable
                                using (var transaction = outerUnitOfWork.BeginTransaction())
                                {
                                    try
                                    {
                                        await AddKronosTrail(outerUnitOfWork, "Kronos Punch Data", $"Getting Kronos Punch data for org{OrgName}", Convert.ToInt32(KronosTrailStatus.InProgress), staticCount, kronosPunchOrgWise.Count);
                                        await DeletePunch(outerUnitOfWork, minDate, maxDate, org);
                                        outerUnitOfWork.KronosPunchRepo.AddRange(kronosPunchOrgWise);
                                        outerUnitOfWork.SaveChanges();
                                        transaction.Commit();
                                        await AddKronosTrail(outerUnitOfWork, "Kronos Punch Data", $"Sent  Kronos Punch data for org{OrgName}", Convert.ToInt32(KronosTrailStatus.Success), staticCount, kronosPunchOrgWise.Count);
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        await AddKronosTrail(outerUnitOfWork, "KronosPunch", $"An Error Occurred in Insert/Delete Kronos Punch data for org{OrgName}", Convert.ToInt32(KronosTrailStatus.Failed), staticCount, kronosPunchOrgWise.Count);
                                        Log.Error(ex, "An Error Occurred in Insert/Delete Kronos Punch data : {errorMsg}", ex.Message);
                                        continue;
                                    }
                                }
                                //Get Missing Facilities
                                await GetNewFacilities(org, staticCount, outerUnitOfWork, OrgName);
                                //Load KronosTimesheet Modified to Match pbjsnap Timesheet
                                await LoadKronosTimeSheet(outerUnitOfWork, staticCount, org, minDate, maxDate, OrgName);
                                //Inserts Conflicts and gets back new mapped data
                                await InsertIntoPbjsnap(outerUnitOfWork, outerUploadService, minDate, maxDate, org, staticCount, OrgName);
                                //gets Unmapped data
                                await GetInvalidData(outerUnitOfWork, org, staticCount, OrgName);

                                await AddKronosTrail(outerUnitOfWork, "NA", $"Completed BackGround Service to pass Kronos Punch data to Timesheet for org : {OrgName}", Convert.ToInt32(KronosTrailStatus.Success), staticCount, 0);

                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "An Error Occurred in BackGround Service Kronos Punch data : {errorMsg}", ex.Message);

                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An Error Occurred in BackGround Service Kronos Punch data  DoWork: {errorMsg}", ex.Message);
                }
            }
        }

        private async Task GetInvalidData(IUnitOfWork unitOfWork, int orgId, int count, string orgName, int records = 0)
        {
            //GetHashCode Invalid data
            try
            {
                //get New Records and Insert conflicts
                var unprocessed = await unitOfWork.executeSpRepo.ExecWithStoreProcedure("exec sp_loadunprocesseddatainto_KronosPunchExportUnprocessData");
                if (unprocessed != null && unprocessed.ToList().Count > 0)
                {
                    if (unprocessed.FirstOrDefault().result != 1)
                    {
                        Log.Error("error", "An Error Occured in Kronos Background Service :{ErrorMsg}", "Error collecting Unprocessed ");
                        await AddKronosTrail(unitOfWork, "Kronos Invalid Data", $"Failed to Get Invalid Kronos Data for org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);
                    }
                    else
                    {
                        Log.Information("InvalidData Processing{c}", "completed");
                        await AddKronosTrail(unitOfWork, "Kronos Invalid Data", $"Successfully Completed Getting Invalid Kronos Data for org : {orgName}", Convert.ToInt32(KronosTrailStatus.Success), count, records);
                    }
                }
                else
                {
                    Log.Information("InvalidData Processing{c}", "No New Data");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Kronos Background Service :{ErrorMsg}", ex.Message);
                await AddKronosTrail(unitOfWork, "Kronos Invalid Data", $"Failed to Get Invalid Kronos Data for org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, 0);

            }
        }

        private async Task<List<KronosPunchExportModel>> GetAllKronosData(IUnitOfWork unitOfWork, List<KronosSftpCredentials> kronosSftpCreds)
        {
            List<KronosPunchExportModel> kronosPunchExportList = new List<KronosPunchExportModel>();
            List<Stream> encryptedKronosPunchExportFileStreamlist = new List<Stream>();
            string getOrgName = string.Empty;
            if (kronosSftpCreds != null && kronosSftpCreds.Count > 0)
            {
                foreach (var item in kronosSftpCreds)
                {
                    getOrgName = unitOfWork.OrganizationRepo.GetAll().FirstOrDefault(x => x.OrganizationID == item.OrganizationId).OrganizationName;
                    try
                    {
                        KronosSftpFileEndpoints kronosSftpFileEndpoints = unitOfWork.kronosFileEndPointRepo.GetAll().Where(x => x.OrganizationId == item.OrganizationId).FirstOrDefault();
                        Stream encryptedKronosPunchExportFileStream = _sftpServices.DownloadKronosPunchExportFile(item, kronosSftpFileEndpoints);
                        if (encryptedKronosPunchExportFileStream != null && encryptedKronosPunchExportFileStream.Length > 0)
                        {
                            int orgId = item.OrganizationId;
                            KronosBlobStorageCreds blobCred = unitOfWork.kronosBlobCredsRepo.GetAll().FirstOrDefault(c => c.OrganizationId == orgId);
                            Stream dencryptedKronosPunchExportFileStream = await _decryptionService.DecryptPgpFileStream(encryptedKronosPunchExportFileStream, blobCred);
                            DataTable punchExportDataTable = _csvReaderService.CsvStreamToDataTable(dencryptedKronosPunchExportFileStream);//DataTable punchExportDataTable = csvReader.LocalFileCsvReader(); 
                            List<KronosPunchExportModel> xport = _kronosPunchExportService.ManipulateKronosPunchExportData(punchExportDataTable, orgId);
                            kronosPunchExportList.AddRange(xport);
                            Log.Information("Got punch data for org {info Msg}", $":{getOrgName} and count :{xport.Count}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddKronosTrail(unitOfWork, "NA", "Failed to Establish connection with Kronos Server", Convert.ToInt32(KronosTrailStatus.Failed), 0, 0);
                        Log.Error(ex, "An Error Occured in GetAllKronosData for org {Error Msg}", $":{getOrgName} {ex.Message}");
                    }
                }
            }
            return kronosPunchExportList;
        }
        private async Task DeletePunch(IUnitOfWork unitOfWork, DateTime min, DateTime max, int orgId)
        {
            try
            {
                List<KronosPunchExportModel> removables = unitOfWork.KronosPunchRepo.GetAll().Where(x => x.OrganizationId == orgId && (x.ADJUSTEDAPPLYDATE >= min && x.ADJUSTEDAPPLYDATE <= max)).ToList();
                unitOfWork.KronosPunchRepo.RemoveRange(removables);
                unitOfWork.SaveChanges();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occurred in DeletePunch Kronos Punch data : {errorMsg}", ex.Message);
            }
        }
        private async Task GetNewFacilities(int orgId, int count, IUnitOfWork unitOfWork, string OrgName)
        {
            try
            {
                var kronosFacilityIds = unitOfWork.KronosPunchRepo.GetAll().Where(x => x.OrganizationId == orgId).Select(c => c.facilityId).Distinct().ToList();
                var fIds = unitOfWork.FacilityRepo.GetAll().Where(c => c.OrganizationId == orgId).Select(c => c.FacilityID).ToList();
                var matchingIds = kronosFacilityIds.Intersect(fIds).ToList();
                var nonMatchingIds = kronosFacilityIds.Except(fIds).ToList();
                var exisistingKronosfacility = unitOfWork.kronosfacilitRepo.GetAll().Where(c => nonMatchingIds.Contains(c.FacilityId)).Select(c => c.FacilityId).ToList();
                var newKronosfacility = nonMatchingIds.Except(exisistingKronosfacility).ToList();

                if (newKronosfacility.Count > 0)
                {
                    var kfacilityList = new List<KronosFacilities>();
                    foreach (var id in newKronosfacility)
                    {
                        KronosFacilities kfacility = new KronosFacilities();
                        kfacility.FacilityId = id;
                        kfacility.Refreshmentdate = DateTime.UtcNow;
                        kfacility.OrganizationId = orgId;
                        if (!string.IsNullOrEmpty(id))
                        {
                            kfacilityList.Add(kfacility);
                        }
                    }
                    if (kfacilityList != null && kfacilityList.Count > 0)
                    {
                        try
                        {
                            unitOfWork.kronosfacilitRepo.AddRange(kfacilityList);
                            unitOfWork.SaveChanges();
                            await AddKronosTrail(unitOfWork, "Kronos Facilities", $"Received New Facilities Kronos Punch data for org{OrgName}", Convert.ToInt32(KronosTrailStatus.Success), count, kfacilityList.Count);

                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An Error Occurred in GetNewFacilities for Org :{errorMsg}", $"{orgId} :{ex.Message}");
                            await AddKronosTrail(unitOfWork, "Facilities", $"Failed to receive New Facilities Kronos Punch data for org{OrgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, kfacilityList.Count);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("error", "An Error Occured while adding Facilities to kronos Facilities:{ErrorMsg}", $"Failed to add facilities for Org:{OrgName} msg:{ex.Message}");
            }
        }
        private async Task LoadKronosTimeSheet(IUnitOfWork unitOfWork, int count, int orgId, DateTime minDate, DateTime maxDate, string orgName, int records = 0)
        {
            try
            {
                orgName = unitOfWork.OrganizationRepo.GetAll().Where(c => c.OrganizationID == orgId).FirstOrDefault().OrganizationName;
                var existingFacilities = unitOfWork.KronosPunchRepo.GetAll().Where(x => x.OrganizationId == orgId).Select(x => x.facilityId).Distinct().ToList();
                var facilities = unitOfWork.FacilityRepo.GetAll().Where(x => x.OrganizationId == orgId).Select(c => c.FacilityID).ToList();
                var processFacilities = existingFacilities.Intersect(facilities).ToList();
                await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"Mapping Kronos Punch data to Timesheet for org{orgName}", Convert.ToInt32(KronosTrailStatus.InProgress), count, processFacilities.Count);

                foreach (var list in processFacilities)
                {
                    var fidSp = list;
                    var facilityName = unitOfWork.FacilityRepo.GetAll().Where(c => c.FacilityID == fidSp).FirstOrDefault().FacilityName;
                    // await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"Mapping Kronos Punch data to Timesheet for facility: {facilityName} of org{orgName}", Convert.ToInt32(KronosTrailStatus.InProgress), count, records);
                    try
                    {
                        var parameters = new object[]{
                                                          new SqlParameter("@facility_id",fidSp),
                                                          new SqlParameter("@start_date", minDate),
                                                          new SqlParameter("@end_date", maxDate),
                                                          new SqlParameter("@OrgId",orgId)
                                                      };
                        IEnumerable<ExecutionSpResult> LoadSpResult = await unitOfWork.executeSpRepo.ExecWithStoreProcedure("exec sp_loaddatainto_kronosTimesheetMapped @facility_id, @start_date, @end_date,@OrgId", parameters);
                        ExecutionSpResult LoadResult = LoadSpResult.FirstOrDefault();
                        if (LoadResult != null && LoadResult.result != Convert.ToInt32(OperationResult.Success))
                        {
                            Log.Error("error", "An Error Occured in Kronos Background Service  Load Sp:{ErrorMsg}", $"Failed to load Mapping for Org:{fidSp}");
                            await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"failed Mapped Kronos Punch data to Timesheet for facility: {facilityName} of org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);
                            continue;
                        }
                        else
                        {
                            await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $" Successfully Mapped Kronos Punch data to Timesheet for facility: {facilityName} of org : {orgName}", Convert.ToInt32(KronosTrailStatus.Success), count, records);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, " An error occurred in LoadKronosTimeSheet for Faility {err} ", $"{list} {ex.Message}");
                        await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"failed Mapped Kronos Punch data to Timesheet for facility: {facilityName} of org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, processFacilities.Count);
                    }
                }
                await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"Completed Mapping Kronos Punch data to Timesheet for org{orgName}", Convert.ToInt32(KronosTrailStatus.Success), count, processFacilities.Count);

            }
            catch (Exception ex)
            {
                Log.Error("error", "An Error Occured in Kronos Background Service  Load Sp:{ErrorMsg}", $"Failed to load Mapping for Org:{orgId}");
                await AddKronosTrail(unitOfWork, "Kronos TimeSheet", $"failed Mapped Kronos Punch data to Timesheet for  org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);

            }
        }
        private async Task InsertIntoPbjsnap(IUnitOfWork unitOfWork, IUpload uploadService, DateTime minDate, DateTime maxDate, int orgId, int count, string orgName, int records = 0)
        {
         
            try
            {
                var existingFacilities = unitOfWork.KronosPunchRepo.GetAll().Where(x => x.OrganizationId == orgId).Select(x => x.facilityId).Distinct().ToList();
                var facilities = unitOfWork.FacilityRepo.GetAll().Where(x => x.OrganizationId == orgId).Select(c => c.FacilityID).ToList();
                var existInBoth = existingFacilities.Intersect(facilities).ToList();
                var proecessable = unitOfWork.FacilityRepo.GetAll().Where(c => existInBoth.Contains(c.FacilityID)).Select(x => x.Id).ToList();

                foreach (var facility in proecessable)
                {
                    var fName = unitOfWork.FacilityRepo.GetById(facility).FacilityName;
                    var dataTable = new DataTable();
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.FullNameWithLastNamefirst), typeof(string));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.EmployeeId), typeof(string));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.FacilityId), typeof(int));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.WorkDay), typeof(string));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.PayTypeCode), typeof(int));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.JobTitleCode), typeof(int));
                    dataTable.Columns.Add(EnumHelper.GetEnumDescription(KronosTimeSheetColums.Hours), typeof(decimal));
                    try
                    {
                        var myparams = new object[]
                         {
                            new SqlParameter("@start_date", minDate),
                            new SqlParameter("@end_date", maxDate),
                            new SqlParameter("@OrgId",orgId),
                            new SqlParameter("@FacilityId",facility)
                         };
                        await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"Collecting newly Mapped Kronos Punch data to Timesheet for org : {orgName} and Facility :{fName}", Convert.ToInt32(KronosTrailStatus.InProgress), count, records);
                        //Call sp
                        IEnumerable<KronosSpResult> result = await unitOfWork.kronosSpResultsRepo.ExecWithStoreProcedure("exec sp_kronosandtimecomparationdata @start_date, @end_date,@OrgId,@FacilityId", myparams);
                  
                        await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"Successfully Collected newly Mapped Kronos Punch data to Timesheet for org : {orgName} and Facility :{fName} ", Convert.ToInt32(KronosTrailStatus.Success), count, result.ToList().Count);
                        //process SpResult
                        if (result != null && result.ToList().Count > 0)
                        {
                            foreach (var data in result)
                            {
                                //Feed datatable with results
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
                            //send to validate
                           await SentToValidate(uploadService, unitOfWork, dataTable, orgName, fName, count, result.ToList().Count);
                        }
                        else
                        {
                            await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"No New Records in Kronos Punch data to Timesheet for org : {orgName} and Facility :{fName}", Convert.ToInt32(KronosTrailStatus.Success), count, records);
                            Log.Information("error", "No New Records ", $"{orgId}");
                        }
                        Log.Information("AutoSync{c}", $"completed for Org :{orgId}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error Occurred in InsertIntoPbjsnap for Org: {err}", $"{orgId} and error is{ex.Message}");
                        await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"Failed Collecting newly Mapped Kronos Punch data to Timesheet for org : {orgName} and Facility :{fName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);
                    }

                }
          
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error Occurred in InsertIntoPbjsnap for Org: {err}", $"{orgId} and error is{ex.Message}");
                await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"Failed Collecting newly Mapped Kronos Punch data to Timesheet for org : {orgName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);

            }
        }
        private async Task SentToValidate(IUpload uploadService,IUnitOfWork unitOfWork, DataTable dataTable,string? orgName, string? fName, int count = 0,int records= 0)
        {
            try
            {
                var final = await uploadService.KronosLogic(dataTable, count);                
                Log.Information("AutoSync{c}", $"completed for facility :{fName}");
            }
            catch (Exception ex)
            {
                await AddKronosTrail(unitOfWork, "Kronos Conflicts", $"Failed to Collect newly Mapped Kronos Punch data to Timesheet for org : {orgName} and Facility :{fName}", Convert.ToInt32(KronosTrailStatus.Failed), count, records);
                Log.Error(ex, "An error Occurred in InsertIntoPbjsnap for Org: {err}", $"{orgName} and error is{ex.Message}");
            }
        }

        private async Task AddKronosTrail(IUnitOfWork unitOfWork, string table, string description, int status, int count, int recordsCount)
        {
            KronosDataTrack kronosDataTrack = new KronosDataTrack();
            Guid guid = Guid.NewGuid();
            kronosDataTrack.Id = guid.ToString();
            kronosDataTrack.ToTable = table;
            kronosDataTrack.Description = description;
            kronosDataTrack.Status = status;
            kronosDataTrack.Source = "Autoyunc";
            kronosDataTrack.CreatedDate = DateTime.UtcNow;
            kronosDataTrack.AutoSyncCount = count;
            kronosDataTrack.RecordsCount = recordsCount;
            unitOfWork.kronosDataTrackRepo.Add(kronosDataTrack);
            unitOfWork.SaveChanges();
        }


    }
}
