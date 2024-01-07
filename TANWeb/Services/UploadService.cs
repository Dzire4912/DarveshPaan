using ExcelDataReader;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using OfficeOpenXml;
using Serilog;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;
using TANWeb.Helpers;
using TANWeb.Interface;
using static TAN.DomainModels.Models.UploadModel;

namespace TANWeb.Services
{
    [ExcludeFromCodeCoverage]
    public class UploadService : IUpload
    {
        public readonly IUnitOfWork _unitOfWork;
        public static DataTable? TimesheetTableData { get; set; }
        public static List<Timesheet> timesheets { get; set; } = null;
        public static List<Timesheet> diffQuarterTimesheet { get; set; } = null;
        public static List<FileErrorRecords> FileErrorRecordsList { get; set; } = null;
        List<TimesheetEmployeeDataResponse> TimesheetEmployeeList = null;
        List<PayTypeCodes> PayTypeCode = new List<PayTypeCodes>();
        List<JobCodes> JobCode = new List<JobCodes>();
        List<Employee> SetEmployeeList = new List<Employee>();
        GenerateDatesByYearQuarterMonth byQuarter = null;
        GenerateDateByMultipleQuarterAndYear generatePrevQuarterDate = null; //
        string UserId = "";
        Utils utils = new Utils();
        private readonly IHttpContextAccessor _httpContext;
        public UploadService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContext)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContext;
        }

        public async Task<DataTable> CheckValidColumn(string[] Column)
        {
            DataTable dataTable = null;
            try
            {
                string ColumnNames = "";
                StringBuilder myStringBuilder = new StringBuilder("");

                if (Column.Length > 0)
                {
                    foreach (var item in Column)
                    {
                        myStringBuilder.Append("('" + item + "')");
                        ColumnNames += "('" + item + "')";
                    }
                    SqlParameter[] sqlParameter = new SqlParameter[1];
                    sqlParameter[0] = new SqlParameter("@ColumnNames", myStringBuilder.ToString());

                    //dataTable = sql.Get_DataTable_SP("", sqlParameter);
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in CheckValidColumn :{ErrorMsg}", ex.Message);
                throw;
            }
            return dataTable;
        }

        public async Task<DataTable> CheckFileExtension(IFormFile file)
        {
            DataTable dataTable = null;
            try
            {
                if (file != null)
                {
                    string extension = Path.GetExtension(file.FileName);
                    switch (extension)
                    {
                        case ".xlsx":
                            dataTable = await ReadExcel(file);
                            break;
                        case ".xls":
                            dataTable = await ReadExcel(file);
                            break;
                        case ".csv":
                            dataTable = ReadCSV(file);
                            break;
                        default: break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in CheckFileExtension :{ErrorMsg}", ex.Message);
                throw;
            }

            return dataTable;
        }

        public async Task<DataTable> ReadExcel(IFormFile file)
        {
            DataTable? dt = new DataTable();
            string ext = Path.GetExtension(file.FileName);
            var stream = file.OpenReadStream();

            IExcelDataReader excelReader;
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                if (ext.ToUpper() == ".XLS")
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                DataSet dataSet = excelReader.AsDataSet(conf);
                if (dataSet != null)
                {
                    dt = dataSet?.Tables[0];
                }

            }
            return dt;
        }

        public static DataTable ReadCSV(IFormFile file)
        {
            DataTable? dt = new DataTable();
            string ext = Path.GetExtension(file.FileName);
            var stream = file.OpenReadStream();
            IExcelDataReader _excelReader;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var ReaderConf = new ExcelReaderConfiguration()
            {
                FallbackEncoding = Encoding.UTF8,
                AutodetectSeparators = new char[] { ',', ';', '\t', '|', '#' },
                LeaveOpen = false,
            };

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };

            if (ext.ToUpper() == ".CSV")
            {
                _excelReader = ExcelReaderFactory.CreateCsvReader(stream, ReaderConf);
                DataSet dataSet = _excelReader.AsDataSet(conf);
                if (dataSet != null)
                {
                    dt = dataSet?.Tables[0];
                }
            }
            return dt;
        }

        public async Task<DataTable> PreviewData(DataTable data, string FacilityId)
        {
            DataTable previewDataTable = new DataTable();
            try
            {
                List<UserMappingFieldData>? userMappingFieldDatas = await _unitOfWork.UserMappingFieldData.
                    GetUserMappingFieldDataByFacility(Convert.ToInt32(FacilityId));

                if (IUpload.uploadFileDetails.DtCmsList == null)
                {
                    IUpload.uploadFileDetails.DtCmsList = await GetAllCMSFieldData();
                }

                int count = 0;
                while (count < 2)
                {
                    previewDataTable.Columns.Add();
                    count++;
                }
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    DataRow newRow = previewDataTable.NewRow();
                    newRow[0] = data.Columns[i].ColumnName.ToString();

                    if ((IUpload.uploadFileDetails.DtCmsList != null) && (userMappingFieldDatas != null && userMappingFieldDatas.Count > 0))
                    {
                        var key = userMappingFieldDatas.Find(x => x.ColumnName == data.Columns[i].ColumnName.ToString());
                        if (key != null)
                            newRow[1] = key.CMSMappingId;
                        else newRow[1] = "";

                    }

                    /*for (int j = 0; j < data.Rows.Count; j++)
                    {
                        newRow[j + 2] = data.Rows[j][i].ToString();
                        if (j > 1)
                            break;
                    }*/
                    previewDataTable.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in PreviewData :{ErrorMsg}", ex.Message);
                throw;
            }
            return previewDataTable;
        }

        public async Task<bool> CheckFileName(string Filename, string facilityId)
        {
            try
            {
                if (Filename == null)
                    return false;

                var filedetails = await _unitOfWork.UploadFileDetailsRepo.GetUploadFileDetailsByFileName(Filename, facilityId);

                if (filedetails.FileName == Filename)
                    return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in CheckFileName :{ErrorMsg}", ex.Message);
                throw;
            }
            return false;
        }

        public async Task<bool> AddUpdateMappingField(MappingRequestData mappingRequestData)
        {
            try
            {
                await _unitOfWork.UserMappingFieldData.GetUserMappingFieldDataFindAddUpdate(mappingRequestData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in AddUpdateMappingField :{ErrorMsg}", ex.Message);
                throw;

            }
            return true;
        }

        public async Task<DataTable> UpdateColumnName(MappingRequestData requestData, DataTable uploadTable)
        {
            List<string> RemoveColNames = new List<string>();
            int _inOut = 1;
            int clockPunch = 0;
            try
            {
                if (uploadTable != null)
                {
                    for (int i = 0; i < uploadTable.Columns.Count; i++)
                    {
                        if ((uploadTable.Columns[i].ColumnName.ToString() == requestData.MappingDatas?[i].ColumnName) &&
                            (requestData.MappingDatas[i].CmsMappingID != 0))
                        {
                            if (requestData.MappingDatas[i].CmsFieldName == "Clock Punch")
                            {
                                if (clockPunch == 0)
                                {
                                    uploadTable.Columns[i].ColumnName = "ClockPunch_IN" + _inOut;
                                    clockPunch++;
                                }
                                else
                                {
                                    uploadTable.Columns[i].ColumnName = "ClockPunch_OUT" + _inOut;
                                    clockPunch = 0;
                                    _inOut++;
                                }
                            }
                            else
                            {
                                uploadTable.Columns[i].ColumnName = requestData.MappingDatas?[i].CmsFieldName;
                            }
                        }
                        else
                        {
                            RemoveColNames.Add(uploadTable.Columns[i].ColumnName);
                        }
                    }
                    foreach (var item in RemoveColNames)
                    {
                        uploadTable.Columns.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in UpdateColumnName :{ErrorMsg}", ex.Message);
            }
            finally
            {
                RemoveColNames.Clear();
            }
            return uploadTable;
        }

        public async Task<bool> BusinessLogic(RequestModel requestData, DataTable FileTable)
        {
            try
            {
                timesheets = new List<Timesheet>();
                diffQuarterTimesheet = new List<Timesheet>();
                IUpload.uploadFileDetails.DiffQuarterTimesheet = new List<Timesheet>();
                IUpload.uploadFileDetails.IsAllRecordInserted = false;
                FileErrorRecordsList = new List<FileErrorRecords>();
                SetEmployeeList = IUpload.uploadFileDetails.ValidEmployeeList;
                UserId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                PayTypeCode = IUpload.uploadFileDetails.PayTypeCode;
                TimesheetTableData = FileTable;
                JobCode = IUpload.uploadFileDetails.JobCode;

                GetQuarterByMonth quarterByMonth = new GetQuarterByMonth(DateTime.Now.Month);
                int StartQuarter = quarterByMonth.Quarter;
                int StartYear = DateTime.Now.Year;
                if (quarterByMonth.Quarter == 1)
                {
                    StartQuarter = 4;
                }
                else
                {
                    StartQuarter -= 1;
                    if (StartQuarter == 1)
                        StartYear -= 1;
                }
                generatePrevQuarterDate = new GenerateDateByMultipleQuarterAndYear(StartQuarter, quarterByMonth.Quarter, StartYear, DateTime.Now.Year);

                byQuarter = new GenerateDatesByYearQuarterMonth(quarterByMonth.Quarter, 0, DateTime.Now.Year);
                await GetListEmployeeWorkday(TimesheetTableData, generatePrevQuarterDate.StartDate, generatePrevQuarterDate.EndDate);
                bool hasDeductuion = _unitOfWork.FacilityRepo.GetAll().FirstOrDefault(c => c.Id == requestData.FacilityId).HasDeduction;
                int regularTime = 0;
                int overTime = 0;
                if (hasDeductuion)
                {
                    regularTime = _unitOfWork.BreakDeductionRepo.GetAll().FirstOrDefault(x => x.FacilityId == requestData.FacilityId).RegularTime;
                    overTime = _unitOfWork.BreakDeductionRepo.GetAll().FirstOrDefault(x => x.FacilityId == requestData.FacilityId).OverTime;
                }

                foreach (DataRow row in TimesheetTableData.Rows)
                {
                    await GetRowData(row, requestData, hasDeductuion, regularTime, overTime);
                }
                if (timesheets.Count > 0)
                {
                    await _unitOfWork.TimesheetRepo.InsertTimesheet(timesheets);
                }
                if (FileErrorRecordsList.Count > 0)
                {
                    await _unitOfWork.FileErrorRecordsRepo.InsertErrorValidationRecordsAddRange(FileErrorRecordsList);
                    IUpload.uploadFileDetails.IsAllRecordInserted = true;
                }
                if (diffQuarterTimesheet != null)
                {
                    if (diffQuarterTimesheet.Count > 0)
                    {
                        IUpload.uploadFileDetails.DiffQuarterTimesheet = diffQuarterTimesheet;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in BusinessLogic :{ErrorMsg}", ex.Message);
                throw;
            }
            return true;
        }

        public async Task<bool> GetRowData(DataRow dataRow, RequestModel requestData, bool ReduceTime = true, int? regularTime = 0, int? overTime = 0)
        {
            try
            {
                ValidatDataResponse validatData = await ValidateData(dataRow, requestData, ReduceTime, regularTime, overTime);
                if (validatData.Success == false)
                {
                    ErrorInsert(JsonConvert.SerializeObject(ConvertDataRowToList(dataRow)), validatData.ErrorMessage, requestData);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetRowData :{ErrorMsg}", ex.Message);
            }
            return true;
        }

        public async Task<ValidatDataResponse> ValidateData(DataRow dataRow, RequestModel requestData, bool? reduceTime = true, int? regularTime = 0, int? overTime = 0)
        {
            Timesheet timesheet = new Timesheet();
            ValidatDataResponse dataResponse = new ValidatDataResponse();
            try
            {
                bool isPrevQuarter = false;
                timesheet.FileDetailId = requestData.FileNameId;
                timesheet.FacilityId = requestData.FacilityId;
                timesheet.UploadType = Convert.ToInt32(requestData.UploadType);
                timesheet.AgencyId = Convert.ToInt32(requestData.AgencyId);
                timesheet.Createby = UserId;

                if (dataRow.Table.Columns.Contains("Job Title Code"))
                {
                    if (String.IsNullOrEmpty(dataRow["Job Title Code"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.JobTitleNull.ToString();
                        return dataResponse;
                    }
                    if (!JobCode.Where(x => x.Id == Convert.ToInt32(dataRow["Job Title Code"])).Any())
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.InvalidJobTitle.ToString();
                        return dataResponse;
                    }
                    timesheet.JobTitleCode = Convert.ToInt32(dataRow["Job Title Code"].ToString());
                }

                if (dataRow.Table.Columns.Contains("Pay Type Code"))
                {
                    if (String.IsNullOrEmpty(dataRow["Pay Type Code"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.PayCodeNull.ToString();
                        return dataResponse;
                    }
                    if (!PayTypeCode.Where(x => x.PayTypeCode == Convert.ToInt32(dataRow["Pay Type Code"])).Any())
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.InvalidPayCode.ToString();
                        return dataResponse;
                    }
                    timesheet.PayTypeCode = Convert.ToInt32(dataRow["Pay Type Code"].ToString());
                }

                if (dataRow.Table.Columns.Contains("Full Name"))
                {
                    if (String.IsNullOrEmpty(dataRow["Full Name"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.FullNameNull.ToString();
                        return dataResponse;
                    }

                    var (firstName, lastName) = ExtractNames(dataRow["Full Name"].ToString().Trim());
                    timesheet.FirstName = firstName;
                    timesheet.LastName = lastName;

                }

                if (dataRow.Table.Columns.Contains("First Name"))
                {
                    if (String.IsNullOrEmpty(dataRow["First Name"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.FirstNameNull.ToString();
                        return dataResponse;
                    }
                    timesheet.FirstName = dataRow["First Name"].ToString().Trim();
                }

                if (dataRow.Table.Columns.Contains("Last Name"))
                {
                    if (String.IsNullOrEmpty(dataRow["Last Name"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.LastNameNull.ToString();
                        return dataResponse;
                    }

                    timesheet.LastName = dataRow["Last Name"].ToString().Trim();
                }

                if (dataRow.Table.Columns.Contains("Full Name with First Name first"))
                {
                    if (String.IsNullOrEmpty(dataRow["Full Name with First Name first"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.NameNull.ToString();
                        return dataResponse;
                    }

                    var (firstName, lastName) = ExtractNames(dataRow["Full Name with First Name first"].ToString().Trim());
                    timesheet.FirstName = firstName;
                    timesheet.LastName = lastName;

                }

                if (dataRow.Table.Columns.Contains("Full Name with Last Name first"))
                {
                    if (String.IsNullOrEmpty(dataRow["Full Name with Last Name first"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.NameNull.ToString();
                        return dataResponse;
                    }

                    var (lastName, firstName) = ExtractNames(dataRow["Full Name with Last Name first"].ToString().Trim());
                    timesheet.FirstName = firstName;
                    timesheet.LastName = lastName;
                }

                if (dataRow.Table.Columns.Contains("Employee Id"))
                {
                    if (String.IsNullOrEmpty(dataRow["Employee Id"].ToString()))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.EmpIdNull.ToString();
                        return dataResponse;
                    }
                    if (dataRow["Employee Id"].ToString()?.Length > 30)
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.EmpIdLength.ToString();
                        return dataResponse;
                    }
                    if (dataRow["Employee Id"].ToString().Contains(" "))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.EmpIdHasSpace.ToString();
                        return dataResponse;
                    }
                    if (SetEmployeeList.Count == 0)
                    {
                        Employee employee = new Employee()
                        {
                            EmployeeId = dataRow["Employee Id"].ToString(),
                            FacilityId = timesheet.FacilityId,
                            JobTitleCode = timesheet.JobTitleCode,
                            PayTypeCode = timesheet.PayTypeCode,
                            FirstName = timesheet.FirstName,
                            LastName = timesheet.LastName,
                            IsActive = true,
                            CreateBy = UserId,
                            CreateDate = DateTime.Now
                        };

                        await _unitOfWork.EmployeeRepo.AddEmployee(employee);
                        SetEmployeeList.Add(employee);
                        employee = null;
                    }
                    if (!SetEmployeeList.Where(x => x.EmployeeId == dataRow["Employee Id"].ToString()).Any())
                    {
                        Employee employee = new Employee()
                        {
                            EmployeeId = dataRow["Employee Id"].ToString(),
                            FacilityId = timesheet.FacilityId,
                            JobTitleCode = timesheet.JobTitleCode,
                            PayTypeCode = timesheet.PayTypeCode,
                            FirstName = timesheet.FirstName,
                            LastName = timesheet.LastName,
                            CreateBy = UserId,
                            IsActive = true,
                            CreateDate = DateTime.Now
                        };

                        await _unitOfWork.EmployeeRepo.AddEmployee(employee);
                        SetEmployeeList.Add(employee);
                        employee = null;
                    }
                    timesheet.EmployeeId = dataRow["Employee Id"].ToString();
                }

                if (dataRow.Table.Columns.Contains("Work Day"))
                {
                    DateTime datetime1;
                    if (!IsValidDateTime(dataRow["Work Day"].ToString(), out datetime1))
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.InvalidDate.ToString();
                        return dataResponse;
                    }
                    DateTime workdate = datetime1;
                    if (workdate > DateTime.Now)
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.WorkdayIsGreaterThenCurrDay.ToString();
                        return dataResponse;
                    }
                    if (!IsDateWithinRange(workdate, byQuarter.StartDate, byQuarter.EndDate))
                    {
                        if (!IsDateWithinRange(workdate, generatePrevQuarterDate.StartDate, generatePrevQuarterDate.EndDate))
                        {
                            dataResponse.Success = false;
                            dataResponse.ErrorMessage = UploadValidationMsg.DifferentQuarterYear.ToString();
                            return dataResponse;
                        }
                        isPrevQuarter = true;
                    }

                    var HireTerminationDate = SetEmployeeList.Where(x => x.EmployeeId == dataRow["Employee Id"].ToString()).Select(s => new
                    {
                        s.HireDate,
                        s.TerminationDate
                    }).FirstOrDefault();
                    if (HireTerminationDate != null)
                    {
                        if (HireTerminationDate.HireDate != DateTime.MinValue
                        && HireTerminationDate.TerminationDate != DateTime.MinValue)
                        {
                            if (!IsDateWithinRange(workdate, HireTerminationDate.HireDate, HireTerminationDate.TerminationDate))
                            {
                                dataResponse.Success = false;
                                dataResponse.ErrorMessage = UploadValidationMsg.HireTerminationDate.ToString();
                                return dataResponse;
                            }
                        }

                    }

                    timesheet.Workday = workdate;
                    timesheet.Month = workdate.Month;
                    timesheet.Year = workdate.Year;
                    timesheet.ReportQuarter = GetQuarter(workdate);
                }

                if (dataRow.Table.Columns.Contains("Hours"))
                {
                    decimal hours = Convert.ToDecimal(dataRow["Hours"].ToString());

                    if (hours <= 0)
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.Hours24.ToString();
                        return dataResponse;
                    }
                    if (hours >= 24)
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.Hours24.ToString();
                        return dataResponse;
                    }

                    else if (hours >= 16)
                    {
                        if (reduceTime != null && reduceTime == true)
                        {

                            if (overTime != null && overTime > 0)
                            {
                                var oTime = (Decimal)overTime / 60;
                                oTime = Math.Round(oTime, 2);
                                hours -= oTime;
                            }
                            else
                            {
                                hours -= 1;
                            }

                        }
                    }
                    else if (hours >= 8 && hours < 16)
                    {
                        if (reduceTime != null && reduceTime == true)
                        {

                            if (regularTime > 0)
                            {
                                var rTime = (Decimal)regularTime / 60;
                                rTime = Math.Round(rTime, 2);
                                hours -= rTime;
                            }
                            else
                            {
                                hours -= Convert.ToDecimal(0.5);
                            }

                        }
                    }
                    timesheet.THours = hours;
                }

                string[] arrayNames = (from DataColumn x
    in dataRow.Table.Columns.Cast<DataColumn>().Where(x => x.ColumnName.Contains("ClockPunch"))
                                       select x.ColumnName).ToArray();

                if (arrayNames.Length > 0)
                {
                    string _in = "", _out = "";
                    int inOut = 1;
                    int flag = 0;
                    timesheet.THours = 0;
                    bool isValid = false;
                    foreach (var item in arrayNames)
                    {
                        bool r = item.Contains("IN");
                        if (r)
                        {
                            if (inOut == 1 && String.IsNullOrEmpty(dataRow[item].ToString()))
                            {
                                dataResponse.Success = false;
                                dataResponse.ErrorMessage = UploadValidationMsg.ClockPunchIn.ToString();
                                return dataResponse;
                            }
                            if (dataRow.Table.Columns.Contains(item))
                            {
                                if (String.IsNullOrEmpty(dataRow[item].ToString()) && inOut > 1)
                                {
                                    break;
                                }
                                if (dataRow[item].ToString().ToLower().Contains("p"))
                                {
                                    flag = 1;
                                }
                                DateTime workDay;
                                bool validDateTime = IsValidDateTime(dataRow[item].ToString(), out workDay);
                                if (validDateTime)
                                {
                                    isValid = true;
                                    timesheet.Workday = workDay.Date;
                                    timesheet.Month = timesheet.Workday.Month;
                                    timesheet.Year = timesheet.Workday.Year;
                                    timesheet.ReportQuarter = GetQuarter(timesheet.Workday);
                                }
                                _in = "";
                                _in += (dataRow[item].ToString().ToLower().Contains("m")) ? dataRow[item].ToString().ToLower() : dataRow[item].ToString().ToLower() + "m";
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(dataRow[item].ToString()))
                            {
                                dataResponse.Success = false;
                                dataResponse.ErrorMessage = UploadValidationMsg.ClockPunchOut.ToString();
                                return dataResponse;
                            }

                            if (dataRow.Table.Columns.Contains(item) && dataRow[item].ToString().ToLower().Contains("a") && flag == 1)
                            {
                                flag = 0;
                                _out = "";
                                _out += (dataRow[item].ToString().ToLower().Contains("m")) ? dataRow[item].ToString().ToLower() :
                                    dataRow[item].ToString().ToLower() + "m";
                                Timesheet ts = new Timesheet()
                                {
                                    Workday = timesheet.Workday.AddDays(1),
                                    EmployeeId = timesheet.EmployeeId,
                                    AgencyId = timesheet.AgencyId,
                                    FirstName = timesheet.FirstName,
                                    LastName = timesheet.LastName,
                                    THours = timesheet.THours,
                                    JobTitleCode = timesheet.JobTitleCode,
                                    PayTypeCode = timesheet.PayTypeCode,
                                    Status = (int)UploadWorkdayStatus.UpdateTHoursToNextDay,
                                    FileDetailId = requestData.FileNameId,
                                    FacilityId = requestData.FacilityId,
                                    Month = requestData.Month,
                                    UploadType = Convert.ToInt32(requestData.UploadType),
                                    ReportQuarter = requestData.Quarter,
                                    Year = requestData.year,
                                    IsActive = true,
                                    Createby = UserId
                                };
                                TimeSpan _times;
                                TimeSpan _times2;
                                if (isValid)
                                {

                                    DateTime inPunch = Convert.ToDateTime(_in);
                                    DateTime outPunch = Convert.ToDateTime(_out);

                                    DateTime dateTime = new DateTime(timesheet.Workday.Year, timesheet.Workday.Month, timesheet.Workday.Day, 23, 59, 59, 59);
                                    string formattedDateTime = dateTime.ToString("MM/dd/yyyy hh:mm:ss tt");

                                    DateTime outDateTime = new DateTime(outPunch.Year, outPunch.Month, outPunch.Day, 00, 00, 01);
                                    string outFormattedDateTime = outDateTime.ToString("MM/dd/yyyy hh:mm:ss tt");


                                    _times = Convert.ToDateTime(formattedDateTime) - Convert.ToDateTime(inPunch.ToString("MM-dd-yyyy hh:mm:ss tt"));

                                    _times2 = Convert.ToDateTime(outPunch) - Convert.ToDateTime(outFormattedDateTime);
                                }
                                else
                                {
                                    _times = Convert.ToDateTime("11:59:59 pm") - Convert.ToDateTime(_in);
                                    _times2 = Convert.ToDateTime(_out) - Convert.ToDateTime("00:00:01 am");
                                }

                                timesheet.THours += (decimal)_times.TotalHours;
                                ts.THours = (decimal)_times2.TotalHours;
                                if (!IsDateWithinRange(ts.Workday, byQuarter.StartDate, byQuarter.EndDate))
                                {
                                    if (!IsDateWithinRange(ts.Workday, generatePrevQuarterDate.StartDate, generatePrevQuarterDate.EndDate))
                                    {
                                        dataResponse.Success = false;
                                        dataResponse.ErrorMessage = UploadValidationMsg.DifferentQuarterYear.ToString();
                                        return dataResponse;
                                    }
                                    isPrevQuarter = true;
                                    diffQuarterTimesheet.Add(ts);
                                }
                                else
                                {
                                    timesheets.Add(ts);
                                }
                                inOut++;
                            }
                            else
                            {
                                _out = "";
                                _out += (dataRow[item].ToString().ToLower().Contains("m")) ? dataRow[item].ToString().ToLower() : dataRow[item].ToString().ToLower() + "m";
                                TimeSpan dt = Convert.ToDateTime(_out) - Convert.ToDateTime(_in);
                                timesheet.THours += (decimal)dt.TotalHours;
                                inOut++;
                            }
                        }
                    }
                    if (timesheet.THours > (int)TotalHours.Zero && timesheet.THours <= (int)TotalHours.Fullday)
                    {
                        if (timesheet.THours >= (int)TotalHours.Sixteen)
                        {
                            if (reduceTime != null && reduceTime == true)
                            {
                                if (overTime > 0)
                                {
                                    var rTime = (decimal)overTime / 60;
                                    rTime = Math.Round(rTime, 2);
                                    timesheet.THours -= rTime;
                                }
                                else
                                {
                                    timesheet.THours -= Convert.ToDecimal(1);
                                }
                            }

                        }
                        else if (timesheet.THours >= (int)TotalHours.Eight && timesheet.THours < (int)TotalHours.Sixteen)
                        {
                            if (reduceTime != null && reduceTime == true)
                            {
                                if (regularTime > 0)
                                {
                                    var rTime = (decimal)regularTime / 60;
                                    rTime = Math.Round(rTime, 2);
                                    timesheet.THours -= rTime;
                                }
                                else
                                {
                                    timesheet.THours -= Convert.ToDecimal(0.5);
                                }
                            }

                        }
                    }
                    else
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.Hours24.ToString();
                        return dataResponse;
                    }

                }

                if (TimesheetEmployeeList != null)
                {
                    decimal TotalHours = 0;
                    TotalHours = TimesheetEmployeeList.Where(x => x.Workday.Date == timesheet.Workday.Date && x.EmployeeId == timesheet.EmployeeId)
                        .Sum(s => s.THours);

                    if ((TotalHours + timesheet.THours) > 24)
                    {
                        dataResponse.Success = false;
                        dataResponse.ErrorMessage = UploadValidationMsg.Hours24.ToString();
                        return dataResponse;
                    }
                    TimesheetEmployeeList.Add(new TimesheetEmployeeDataResponse()
                    {
                        EmployeeId = timesheet.EmployeeId,
                        Workday = timesheet.Workday,
                        THours = timesheet.THours,
                        FacilityId = timesheet.FacilityId
                    });
                }

                if (isPrevQuarter)
                {
                    diffQuarterTimesheet.Add(timesheet);
                }
                else
                {
                    timesheets.Add(timesheet);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in ValidateData :{ErrorMsg}", ex.Message);
                dataResponse.Success = false;
                dataResponse.ErrorMessage = ex.ToString();
                timesheet = null;
                return dataResponse;
            }
            timesheet = null;
            dataResponse.Success = true;
            return dataResponse;
        }

        public async Task<Int32> AddFilenName(string filename, string FacilityID, int RowCount, RequestModel requestModel)
        {
            try
            {
                var uploadFile = await _unitOfWork.UploadFileDetailsRepo.AddFileDetails(filename, FacilityID, RowCount, requestModel);
                if (uploadFile != null)
                {
                    return uploadFile.Id;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in AddFilenName :{ErrorMsg}", ex.Message);
                return 0;
            }
            return 0;
        }

        public bool IsValidDateTime(string dateTime, out DateTime parsedDateTime)
        {
            parsedDateTime = DateTime.Now;
            try
            {
                string[] formats = { "MM/dd/yyyy h:mm:ss tt", "M/d/yyyy h:mm:ss tt", "M/dd/yyyy h:mm:ss tt", "MM/d/yyyy h:mm:ss tt", "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM-dd-yyyy" };
                return DateTime.TryParseExact(dateTime, formats, new CultureInfo("en-US"),
                                               DateTimeStyles.None, out parsedDateTime);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ErrorInsert(string row, string errmsg, RequestModel requestData)
        {
            FileErrorRecords fileErrRec = new FileErrorRecords();
            try
            {
                fileErrRec.FileDetailId = requestData.FileNameId;
                fileErrRec.FacilityId = requestData.FacilityId;
                fileErrRec.Month = requestData.Month;
                fileErrRec.ReportQuarter = requestData.Quarter;
                fileErrRec.Year = requestData.year;
                fileErrRec.ErrorMessage = errmsg;
                fileErrRec.FileRowData = row;
                fileErrRec.Status = 0;
                fileErrRec.Createdby = UserId;
                FileErrorRecordsList.Add(fileErrRec);
                fileErrRec = null;

            }
            catch (Exception ex)
            {
                fileErrRec = null;
                Log.Error(ex, "An Error Occured in ErrorInsert :{ErrorMsg}", ex.Message);
            }
        }

        public static List<Dictionary<string, object>> ConvertDataRowToList(DataRow row)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            foreach (DataColumn column in row.Table.Columns)
            {
                Dictionary<string, object> keyValue = new Dictionary<string, object>();
                keyValue[column.ColumnName] = row[column.ColumnName];
                result.Add(keyValue);
            }

            return result;
        }

        public async Task<bool> GetListEmployeeWorkday(DataTable dataTable, DateTime startDate, DateTime endDate)
        {
            try
            {
                DataView view = new DataView(dataTable);
                List<string> EmpId = utils.ConvertDataTableToList(view.ToTable(true, "Employee Id"));

                TimesheetEmployeeDataRequest timeReq = new TimesheetEmployeeDataRequest()
                {
                    FacilityId = IUpload.uploadFileDetails.FacilityId,
                    Month = IUpload.uploadFileDetails.Month,
                    ReportQuarter = IUpload.uploadFileDetails.Quarter,
                    Year = IUpload.uploadFileDetails.Year,
                    EmployeeId = EmpId,
                    StartDate = startDate,
                    EndDate = endDate
                };
                TimesheetEmployeeList = await _unitOfWork.TimesheetRepo.GetWorkdayDetails(timeReq);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetListEmployeeWorkday :{ErrorMsg}", ex.Message);
            }
            return true;
        }

        public async Task<DataTable> ReadXml(IFormFile file)
        {
            DataTable? xmlDataTable = new DataTable();
            try
            {
                var stream = file.OpenReadStream();
                XMLReaderModel.NursingHomeData data = new XMLReaderModel.NursingHomeData();
                XmlSerializer reader = new XmlSerializer(data.GetType());
                data = (XMLReaderModel.NursingHomeData)reader.Deserialize(stream);
                if (data != null)
                {
                    return await ReadFromXml(data);
                }
                else
                {
                    return xmlDataTable;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in ReadXml error :{ErrorMsg}", ex.Message);
                throw;
            }
        }

        public async Task<DataTable> ReadFromXml(TAN.DomainModels.Models.XMLReaderModel.NursingHomeData data)
        {
            DataTable dt = new DataTable();
            try
            {
                dt.Columns.Add("Employee Id");
                dt.Columns.Add("Hire Date");
                dt.Columns.Add("Termination Date");
                dt.Columns.Add("Work Day");
                dt.Columns.Add("Hours");
                dt.Columns.Add("Pay Type Code");
                dt.Columns.Add("Job Title Code");

                foreach (XMLReaderModel.StaffHours hours in data.StaffingHours.staffingHours)
                {
                    foreach (XMLReaderModel.WorkDay day in hours.workDays.workDay)
                    {
                        foreach (XMLReaderModel.HourEntry entry in day.hourEntries.hourEntry)
                        {
                            DataRow row = dt.NewRow();
                            row["Employee Id"] = hours.employeeId;
                            row["Work Day"] = day.date;
                            row["Hours"] = entry.hours;
                            row["Job Title Code"] = entry.jobTitleCode;
                            row["Pay Type Code"] = entry.payTypeCode;
                            dt.Rows.Add(row);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Read from xml :{ErrorMsg}", ex.Message);
            }
            return dt;
        }

        public async Task<bool> HasDuplicateColumns(MappingRequestData requestData)
        {
            try
            {
                bool hasDuplicates = requestData.MappingDatas.Where(s => s.CmsFieldName != "Clock Punch" && s.CmsFieldName != "Ignore Field" && s.CmsFieldName != "").GroupBy(x => x.CmsMappingID).Any(g => g.Count() > 1);
                if (hasDuplicates)
                    return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in HasDuplicateColumns :{ErrorMsg}", ex.Message);
                throw;
            }
            return false;
        }

        public async Task<List<string>> HasPresentRequiredCmsColumn(List<MappingData> MappingDatas)
        {
            List<string> MissingField = new List<string>();
            try
            {
                List<string> desiredStrings = new List<string>()
        {
            "Employee Id",
            "Pay Type Code",
            "Job Title Code",
            "Work Day"
        };
                if (MappingDatas.Count > 0)
                {
                    foreach (string desiredString in desiredStrings)
                    {
                        if (!MappingDatas.Exists(pair => pair.CmsFieldName.Contains(desiredString)))
                        {
                            MissingField.Add(desiredString);
                        }
                    }

                    bool containsHoursOrIn = MappingDatas.Any(s => s.CmsFieldName.Contains("Hours")
                    || s.CmsFieldName.Contains("Clock Punch"));
                    if (!containsHoursOrIn)
                    {
                        MissingField.Add("Total Hours or Clock Punch ");
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in HasPresentRequiredCmsColumn :{ErrorMsg}", ex.Message);
                throw;
            }
            return MissingField;
        }

        public async Task<List<CMSMappingFieldData>> GetAllCMSFieldData()
        {
            List<CMSMappingFieldData>? CmsfieldDatas = null;
            try
            {
                CmsfieldDatas = await _unitOfWork.CmsMapFieldDataRepo.GetAllCMSMappingFieldData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetAllCMSFieldData :{ErrorMsg}", ex.Message);
                throw;
            }
            return CmsfieldDatas;
        }

        public async Task<List<Employee>> CheckEmployee(DataTable dataTable, int FacilityId)
        {
            List<Employee> EmpList = new List<Employee>();
            try
            {
                DataView view = new DataView(dataTable);
                List<string> EmpIds = utils.ConvertDataTableToList(view.ToTable(true, "Employee Id"));
                EmpList = await _unitOfWork.EmployeeRepo.GetEmployeeDetails(EmpIds, FacilityId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in upload service - CheckEmployee :{ErrorMsg}", ex.Message);
            }
            return EmpList;
        }

        public async Task<ImportFileResponse> ImportEmployee(DataTable dataTable, int facilityId)
        {
            ImportFileResponse importFileResponse = new ImportFileResponse();
            try
            {
                if (dataTable != null)
                {
                    string user_Id = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                    List<PayTypeCodes> payTypeCodes = await _unitOfWork.PayTypeCodesRepo.GetAllPayTypeCodes();
                    List<JobCodes> jobCodes = await _unitOfWork.JobCodesRepo.GetAllJobCodes();

                    int row = 0;
                    bool flag = true;
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        Employee employee = new Employee();
                        row++;
                        if (dr["Employee Id"].ToString().Trim() != "")
                        {
                            var EmployeeData = await _unitOfWork.EmployeeRepo.CheckDuplicateEmployeeData(dr["Employee Id"].ToString(), facilityId);
                            if (EmployeeData != null)
                            {
                                if (EmployeeData.Id != 0)
                                {
                                    importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Employee Id is already exist"));
                                    flag = false;
                                    continue;
                                }
                            }
                            else
                            {
                                employee.EmployeeId = dr["Employee Id"].ToString();
                            }
                        }
                        else
                        {
                            importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Employee Id is missing"));
                            flag = false;
                            continue;
                        }
                        if (dr["First Name"].ToString().Trim() != "")
                        {
                            employee.FirstName = dr["First Name"].ToString();
                        }
                        if (dr["Last Name"].ToString().Trim() != "")
                        {
                            employee.LastName = dr["Last Name"].ToString();
                        }

                        DateTime hireDate;
                        if (dr["Hire Date"].ToString().Trim() != "")
                        {
                            DateTime HireDate;
                            if (!IsValidDateTime(dr["Hire Date"].ToString(), out HireDate))
                            {
                                importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Invalid date format for Hire date"));
                                flag = false;
                                continue;
                            }
                            else
                            {
                                employee.HireDate = HireDate;
                            }
                        }
                        if (dr["Termination Date"].ToString().Trim() != "")
                        {
                            DateTime TerminationDate;
                            if (!IsValidDateTime(dr["Termination Date"].ToString(), out TerminationDate))
                            {
                                importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Invalid date format for Termination date"));
                                flag = false;
                                continue;
                            }
                            else
                            {
                                if (dr["Hire Date"].ToString().Trim() == "")
                                {
                                    importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Hire date is missing."));
                                    flag = false;
                                    continue;
                                }
                                if (employee.HireDate > TerminationDate)
                                {
                                    importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Hire date is greater then termination date."));
                                    flag = false;
                                    continue;
                                }
                                employee.TerminationDate = TerminationDate;
                            }
                        }

                        if (dr["Pay Type"].ToString().Trim() != "")
                        {
                            var PayTypeCheck = payTypeCodes.Where(x => x.PayTypeCode == Convert.ToInt32(dr["Pay Type"].ToString())).Any();
                            if (PayTypeCheck)
                            {
                                employee.PayTypeCode = Convert.ToInt32(dr["Pay Type"].ToString());
                            }
                            else
                            {
                                importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Invalid Pay type."));
                                flag = false;
                                continue;
                            }
                        }
                        else
                        {
                            importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Pay type is missing"));
                            flag = false;
                            continue;
                        }
                        if (dr["Job Title"].ToString().Trim() != "")
                        {
                            var JobTitle = jobCodes.Where(x => x.Id == Convert.ToInt32(dr["Job Title"].ToString())).Any();
                            if (JobTitle)
                            {
                                employee.JobTitleCode = Convert.ToInt32(dr["Job Title"].ToString());
                            }
                            else
                            {
                                importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Invalid Job title"));
                                flag = false;
                                continue;
                            }
                        }
                        else
                        {
                            importFileResponse.keyValuePairs.Add(new KeyValuePair<string, string>(row.ToString(), "Job title is missing"));
                            flag = false;
                            continue;
                        }
                        employee.IsActive = true;
                        employee.CreateDate = DateTime.Now;
                        employee.CreateBy = user_Id;
                        employee.FacilityId = facilityId;
                        await _unitOfWork.EmployeeRepo.AddEmployee(employee);
                    }
                    if (flag)
                    {
                        importFileResponse.StatusCode = 200;
                        importFileResponse.Message = "Successfully imported.";
                    }
                    else
                    {
                        importFileResponse.StatusCode = 202;
                        importFileResponse.Message = "Please find few of the data has validation errors!";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in upload service - ImportEmployee :{ErrorMsg}", ex.Message);
            }
            return importFileResponse;
        }

        public async Task<ValidatDataResponse> ReValidatedWorkingHours(ErrorData errorData)
        {
            ValidatDataResponse dataResponse = new ValidatDataResponse();
            try
            {
                if (errorData != null)
                {
                    timesheets = new List<Timesheet>();
                    diffQuarterTimesheet = new List<Timesheet>();
                    string _userid = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    PayTypeCode = await _unitOfWork.PayTypeCodesRepo.GetAllPayTypeCodes();
                    JobCode = await _unitOfWork.JobCodesRepo.GetAllJobCodes();
                    if (errorData.keyValueList?.Count > 0)
                    {
                        TimesheetTableData = ConvertListKeyValuePairToDataTable(errorData.keyValueList);
                        if (TimesheetTableData != null && TimesheetTableData.Rows.Count > 0)
                        {
                            SetEmployeeList = await CheckEmployee(TimesheetTableData, errorData.FacilityId);
                            IUpload.uploadFileDetails.FacilityId = errorData.FacilityId;
                            IUpload.uploadFileDetails.Year = errorData.Year;
                            
                            GetQuarterByMonth quarterByMonth = new GetQuarterByMonth(DateTime.Now.Month);
                            int StartQuarter = quarterByMonth.Quarter;
                            int StartYear = DateTime.Now.Year;
                            if (quarterByMonth.Quarter == 1)
                            {
                                StartQuarter = 4;
                            }
                            else
                            {
                                StartQuarter -= 1;
                                if (StartQuarter == 1)
                                    StartYear -= 1;
                            }
                            generatePrevQuarterDate = new GenerateDateByMultipleQuarterAndYear(StartQuarter, quarterByMonth.Quarter, StartYear, DateTime.Now.Year);

                            byQuarter = new GenerateDatesByYearQuarterMonth(errorData.ReportQuarter, errorData.Month, errorData.Year);
                            await GetListEmployeeWorkday(TimesheetTableData, generatePrevQuarterDate.StartDate, generatePrevQuarterDate.EndDate);
                            RequestModel requestModel = new RequestModel();

                            bool hasDeductuion = _unitOfWork.FacilityRepo.GetAll().FirstOrDefault(c => c.Id == errorData.FacilityId).HasDeduction;
                            int regularTime = 0;
                            int overTime = 0;
                            if (hasDeductuion)
                            {
                                regularTime = _unitOfWork.BreakDeductionRepo.GetAll().FirstOrDefault(x => x.FacilityId == errorData.FacilityId).RegularTime;
                                overTime = _unitOfWork.BreakDeductionRepo.GetAll().FirstOrDefault(x => x.FacilityId == errorData.FacilityId).OverTime;
                            }

                            dataResponse = await ValidateData(TimesheetTableData.Rows[0], requestModel, hasDeductuion, regularTime, overTime);
                            if (dataResponse.Success == true)
                            {
                                if (timesheets.Count > 0)
                                {
                                    timesheets[0].Createby = _userid;
                                    timesheets[0].CreateDate = DateTime.Now;
                                    timesheets[0].FacilityId = errorData.FacilityId;
                                    timesheets[0].UploadType = errorData.UploadType;
                                    /*timesheets[0].ReportQuarter = errorData.ReportQuarter;
                                    timesheets[0].Year = errorData.Year;
                                    timesheets[0].Month = errorData.Month;*/
                                    timesheets[0].FileDetailId = errorData.FileDetailId;

                                    await _unitOfWork.TimesheetRepo.InsertTimesheet(timesheets);
                                    dataResponse.ErrorMessage = "Record is inserted to the Timesheet";
                                }
                            }
                        }
                    }
                    else
                    {
                        dataResponse.ErrorMessage = "Failed to validate the record";
                        dataResponse.Success = false;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in upload service - ReValidatedWorkingHours :{ErrorMsg}", ex.Message);
            }
            return dataResponse;
        }

        public static DataTable ConvertListKeyValuePairToDataTable(List<KeyValuePair<string, string>> keyValuePairs)
        {
            DataTable dataTable = new DataTable();
            foreach (var kvp in keyValuePairs)
            {
                dataTable.Columns.Add(kvp.Key, typeof(string));
            }
            DataRow row = dataTable.NewRow();
            foreach (var kvp in keyValuePairs)
            {
                row[kvp.Key] = kvp.Value;
            }
            dataTable.Rows.Add(row);

            return dataTable;
        }

        public bool IsDateWithinRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck <= endDate;
        }

        public async Task<DataTable> CheckOneDriveFileExtension(Stream file, string fileName)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (file != null)
                {
                    string extension = Path.GetExtension(fileName);
                    switch (extension)
                    {
                        case ".xlsx":
                            dataTable = OneDriveExcelToDataTable(file);
                            break;
                        case ".xls":
                            dataTable = OneDriveExcelToDataTable(file);
                            break;
                        case ".csv":
                            dataTable = OneDriveCsvToDataTable(file);
                            break;
                        default: break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in CheckOneDriveFileExtension :{ErrorMsg}", ex.Message);
                throw;
            }
            return dataTable;
        }

        static DataTable OneDriveExcelToDataTable(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var dataTable = new DataTable();

                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    dataTable.Columns.Add(firstRowCell.Text);
                }

                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                    var newRow = dataTable.Rows.Add();
                    foreach (var cell in row)
                    {
                        newRow[cell.Start.Column - 1] = cell.Text;
                    }
                }
                return dataTable;
            }
        }

        static DataTable OneDriveCsvToDataTable(Stream csvStream)
        {
            DataTable dataTable = new DataTable();

            using (TextFieldParser csvParser = new TextFieldParser(new StreamReader(csvStream)))
            {
                csvParser.TextFieldType = FieldType.Delimited;
                csvParser.SetDelimiters(",");

                // Read the header row and create columns
                string[] headers = csvParser.ReadFields();
                foreach (string header in headers)
                {
                    dataTable.Columns.Add(header);
                }
                // Read data rows and populate the DataTable
                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    dataTable.Rows.Add(fields);
                }
            }
            return dataTable;
        }

        public int GetQuarter(DateTime dateTime)
        {
            int Month = dateTime.Month;
            int Quarter = 0;
            switch (Month)
            {
                case 1:
                case 2:
                case 3:
                    Quarter = 2;
                    break;
                case 4:
                case 5:
                case 6:
                    Quarter = 3;
                    break;
                case 7:
                case 8:
                case 9:
                    Quarter = 4;
                    break;
                case 10:
                case 11:
                case 12:
                    Quarter = 1;
                    break;
                default:
                    Quarter = 0;
                    break;
            }
            return Quarter;
        }

        static string[] SplitName(string name)
        {
            return name.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static (string firstName, string lastName) ExtractNames(string fullName)
        {
            string cleanedName = Regex.Replace(fullName, @"[^\w\s'-]", "");
            string[] words = cleanedName.Split();

            string firstName = string.Join(" ", words[..^1]);
            string lastName = words[^1];

            return (firstName, lastName);
        }

        public async Task<bool> KronosLogic(DataTable FileTable, int? count = 0) // 25 facilities
        {
            try
            {
                PayTypeCode = _unitOfWork.PayTypeCodesRepo.GetAll().ToList();//get all obj paycodes
                JobCode = _unitOfWork.JobCodesRepo.GetAll().ToList();//get all obj jobcodes
                timesheets = new List<Timesheet>();
                FileErrorRecordsList = new List<FileErrorRecords>();
                diffQuarterTimesheet = new List<Timesheet>();

                GetQuarterByMonth quarterByMonth = new GetQuarterByMonth(DateTime.Now.Month);
                int StartQuarter = quarterByMonth.Quarter;
                int StartYear = DateTime.Now.Year;
                if (quarterByMonth.Quarter == 1)
                {
                    StartQuarter = 4;
                }
                else
                {
                    StartQuarter -= 1;
                    if (StartQuarter == 1)
                        StartYear -= 1;
                }
                generatePrevQuarterDate = new GenerateDateByMultipleQuarterAndYear(StartQuarter, quarterByMonth.Quarter, StartYear, DateTime.Now.Year);
                byQuarter = new GenerateDatesByYearQuarterMonth(quarterByMonth.Quarter, 0, DateTime.Now.Year);
                var facilityIds = FileTable.AsEnumerable().Select(row => Convert.ToInt32(row["FacilityID"])).Distinct().ToList();
                int validattioCount = 0;
                foreach (int facilityId in facilityIds)
                {
                    try
                    {
                        var facilityData = FileTable.AsEnumerable().Where(row => Convert.ToInt32(row["FacilityID"]) == facilityId).CopyToDataTable();
                        SetEmployeeList = await CheckEmployee(facilityData, facilityId);
                        UserId = "kronos";
                        TimesheetTableData = facilityData;

                        RequestModel requestData = new RequestModel();
                        requestData.FacilityId = facilityId;
                        string fName = _unitOfWork.FacilityRepo.GetById(facilityId).FacilityName;
                        #region Validation Inprogress Trail

                        await AddKronosTrail("TimeSheet", $"Validating Records from Kronos for Facility:{fName}", Convert.ToInt32(KronosTrailStatus.InProgress), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, facilityData.Rows.Count);
            
                        #endregion
                        #region Validation
                        foreach (DataRow row in TimesheetTableData.Rows)
                        {
                            try
                            {
                                await GetRowData(row, requestData, false);
                                validattioCount++;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "An Error Occured in BusinessLogic while Validating :{ErrorMsg}", ex.Message);
                                continue;
                            }
                        }
                        #endregion
                        #region Validation Complete

                        await  AddKronosTrail("TimeSheet", $"Validated Records from Kronos for Facility :{fName}", Convert.ToInt32(KronosTrailStatus.Success), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, facilityData.Rows.Count);
                      
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured in Kronos Validation :{ErrorMsg}",$"for Facility :{facilityId}, { ex.Message}");
                        continue;
                    }
                }
                await AddKronosTrail("TimeSheet", $"Sending  {(timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0)} records to PBJSnap TimeSheet & {FileErrorRecordsList.Count} Sent to File Error Records", Convert.ToInt32(KronosTrailStatus.InProgress), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, (timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0) + (FileErrorRecordsList != null ? FileErrorRecordsList.Count : 0));
             
                //out of the loop 
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    try
                    {
                        if (timesheets != null && timesheets.Count > 0)
                        {
                            await _unitOfWork.TimesheetRepo.InsertTimesheet(timesheets);
                        }
                        if (FileErrorRecordsList != null && FileErrorRecordsList.Count > 0)
                        {
                            await _unitOfWork.FileErrorRecordsRepo.InsertErrorValidationRecordsAddRange(FileErrorRecordsList);
                        }
                        if (diffQuarterTimesheet != null && diffQuarterTimesheet.Count > 0)
                        {
                            await _unitOfWork.TimesheetRepo.InsertTimesheet(diffQuarterTimesheet);
                        }
                        transaction.Commit();
                    }
                    catch(Exception ex)
                    {
                        transaction.Rollback();
                        //log
                        Log.Error(ex, "An Error Occured in  Kronos Business Logic :{ErrorMsg}", ex.Message);
                        await AddKronosTrail("TimeSheet", $"Failed to {(timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0)} records to PBJSnap TimeSheet & {FileErrorRecordsList.Count} Sent to File Error Records", Convert.ToInt32(KronosTrailStatus.Failed), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, (timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0) + (FileErrorRecordsList != null ? FileErrorRecordsList.Count : 0));

                    }
              
                }
                if ((diffQuarterTimesheet.Count > 0 || timesheets.Count > 0))
                {
                    await AddKronosTrail("TimeSheet", $"Sent {(timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0)} records to PBJSnap TimeSheet & {FileErrorRecordsList.Count} Sent to File Error Records", Convert.ToInt32(KronosTrailStatus.Success), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, (timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0) + (FileErrorRecordsList != null ? FileErrorRecordsList.Count : 0));
 
                }


            }
            catch (Exception ex)
            {
                await AddKronosTrail("TimeSheet", $"Failed to {(timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0)} records to PBJSnap TimeSheet & {FileErrorRecordsList.Count} Sent to File Error Records", Convert.ToInt32(KronosTrailStatus.Failed), Convert.ToInt32(count) != null ? Convert.ToInt32(count) : 0, (timesheets != null ? timesheets.Count : 0) + (diffQuarterTimesheet != null ? diffQuarterTimesheet.Count : 0) + (FileErrorRecordsList != null ? FileErrorRecordsList.Count : 0));
                Log.Error(ex, "An Error Occured in  Kronos Business Logic :{ErrorMsg}", ex.Message);            
                return false;
            }
            return true;
        }
        public async Task AddKronosTrail( string table, string description, int status, int? count=0, int recordsCount=0)
        {
            KronosDataTrack kronosDataTrack = new KronosDataTrack();
            Guid guid = Guid.NewGuid();
            kronosDataTrack.Id = guid.ToString();
            kronosDataTrack.ToTable = table;
            kronosDataTrack.Description = description;
            kronosDataTrack.Status = status;
            if (count != null && count > 0)
            {
                kronosDataTrack.Source = "Autoyunc";
            }
            else
            {
                kronosDataTrack.Source = "User Action";
            }          
            kronosDataTrack.CreatedDate = DateTime.UtcNow;
            kronosDataTrack.AutoSyncCount = (int)count;
            kronosDataTrack.RecordsCount = recordsCount;
            _unitOfWork.kronosDataTrackRepo.Add(kronosDataTrack);
            _unitOfWork.SaveChanges();
        }


    }
}
