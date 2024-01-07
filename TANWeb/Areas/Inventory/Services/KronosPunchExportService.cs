using System.Data;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TANWeb.Areas.Inventory.Models;
using static TAN.DomainModels.Helpers.Permissions;

namespace TANWeb.Areas.Inventory.Services
{
    [ExcludeFromCodeCoverage]
    public class KronosPunchExportService
    {
        public  List<KronosPunchExportModel> ManipulateKronosPunchExportData(DataTable dataTable, int orgId=0)
         {
            List<DataRow> dataList = dataTable.AsEnumerable().ToList();
            //add Condition in future for org wise mapping
            List<KronosPunchExportModel> kronosList = new List<KronosPunchExportModel>();

            foreach (var item in dataList)
            {
                try
                {
                    if (item.IsNull("Last Name") != true && item.IsNull("First Name") != true && item.IsNull("EmployeeID") != true)
                    {
                        KronosPunchExportModel kronosModel = new KronosPunchExportModel
                        {
                            HrJob = Convert.ToString(item["HrJob"]),
                            facilityId = Convert.ToString(item["FacilityCode"]),
                            EmployeeID = Convert.ToString(item["EmployeeID"]),
                            EmployeeName = Convert.ToString($"{item["Last Name"]}") + "," + Convert.ToString($"{item["First Name"]}"),
                            NATIVEKRONOSID = Convert.ToString(item["ShortName"]),
                            DEPARTMENTNUMBER = Convert.ToString(item["DepartmentCode"]),
                            DepartmentDescription = Convert.ToString(item["Account"]),
                            JOBCODE = Convert.ToString(item["Code"]), 
                            JobDescription = null, //Convert.ToString(item["Job Description"]),
                            ADJUSTEDAPPLYDATE = item.IsNull("LaborDate") ? null : (DateTime?)Convert.ToDateTime(item["LaborDate"]),
                            PayCode = Convert.ToString(item["PayCode"]),
                            TIMEINHOURS = item.IsNull("TotalHrs") ? null : (decimal?)Convert.ToDecimal(item["TotalHrs"]),
                            OrganizationId= orgId
                        };
                        kronosList.Add(kronosModel); 
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }
            return kronosList;
        }
    }
}
