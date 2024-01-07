using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions; 

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class CensusRepository : Repository<Census>, ICensus
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public CensusRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }
        public List<CensusData> GetCencusList(int PageSize, int PageNo, out int TotalCount, CensusViewRequest cencusViewRequest, string sortOrder)
        {
            List<CensusData> response = new List<CensusData>();
            TotalCount = 0;
            try
            {               
               var data =  (from c in Context.Census
                                  join f in Context.Facilities on c.FacilityId equals f.Id
                                  where f.IsActive && c.IsActive && f.Id == Convert.ToInt32(cencusViewRequest.FacilityID) && 
                                  c.Year == cencusViewRequest.Year && c.ReportQuarter == cencusViewRequest.ReportQuarter 
                            select new CensusData { 
                                      CensusId = c.CensusId.ToString(), Year = c.Year, 
                                      Other = c.Other, Medicad = c.Medicad, 
                                      Medicare = c.Medicare, FacilityName = f.FacilityName,
                                      Month = c.Month.ToString()
                                  }).ToList();
                if (data.Count > 0)
                {
                    TotalCount = data.Count;
                    data = data.Skip(PageNo).Take(PageSize).ToList();
                    foreach (var item in data)
                    {
                        CensusData cencusData = new CensusData();
                        cencusData.FacilityName = item.FacilityName.ToString();
                        cencusData.CensusId = item.CensusId.ToString();
                        cencusData.Year = item.Year;
                        cencusData.Other = item.Other;
                        cencusData.Medicare = item.Medicare;
                        cencusData.Medicad = item.Medicad;
                        cencusData.Month = GetMonth(Convert.ToInt32(item.Month));
                        response.Add(cencusData);
                    }

                    if (!string.IsNullOrEmpty(sortOrder))
                    {
                        switch (sortOrder)
                        {
                            case "facilityname_asc":
                                response = response.OrderBy(x => x.FacilityName).ToList();
                                break;
                            case "facilityname_desc":
                                response = response.OrderByDescending(x => x.FacilityName).ToList();
                                break;
                            case "year_asc":
                                response = response.OrderBy(x => x.Year).ToList();
                                break;
                            case "year_desc":
                                response = response.OrderByDescending(x => x.Year).ToList();
                                break;
                            case "month_asc":
                                response = response.OrderBy(x => x.Month).ToList();
                                break;
                            case "month_desc":
                                response = response.OrderByDescending(x => x.Month).ToList();
                                break;
                            case "medicare_asc":
                                response = response.OrderBy(x => x.Medicare).ToList();
                                break;
                            case "medicare_desc":
                                response = response.OrderByDescending(x => x.Medicare).ToList();
                                break;
                            case "medicad_asc":
                                response = response.OrderBy(x => x.Medicad).ToList();
                                break;
                            case "medicad_desc":
                                response = response.OrderByDescending(x => x.Medicad).ToList();
                                break;
                            case "other_asc":
                                response = response.OrderBy(x => x.Other).ToList();
                                break;
                            case "other_desc":
                                response = response.OrderByDescending(x => x.Other).ToList();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return response;
        }
        public async Task<bool> AddCencus(Census census)
        {
            try
            {
                if (census != null)
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (census.CensusId == 0)
                    {
                        census.CreateBy = userId;
                        census.IsActive = true;
                        census.CreateDate = DateTime.Now;
                        await Context.Census.AddAsync(census);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        census.UpdateDate = DateTime.Now;
                        census.UpdateBy = userId;
                        census.IsActive = true;
                        Context.Census.Update(census);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> DeleteCencus(int CensusId)
        {
            try
            {
                if (CensusId != 0)
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var data = Context.Census.Where(x => x.CensusId == CensusId && x.IsActive).FirstOrDefault();
                    if(data != null)
                    {
                        data.UpdateDate = DateTime.Now;
                        data.IsActive = false;
                        data.UpdateBy = userId;
                        Context.Census.Update(data);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public string GetMonth(int Month)
        {
            string MonthName = "";
            switch (Month)
            {
                case 1:
                    MonthName = "January";
                    break;
                case 2:
                    MonthName = "February";
                    break;
                case 3:
                    MonthName = "March";
                    break;
                case 4:
                    MonthName = "April";
                    break;
                case 5:
                    MonthName = "May";
                    break;
                case 6:
                    MonthName = "Jun";
                    break;
                case 7:
                    MonthName = "July";
                    break;
                case 8:
                    MonthName = "August";
                    break;
                case 9:
                    MonthName = "September";
                    break;
                case 10:
                    MonthName = "October";
                    break;
                case 11:
                    MonthName = "November";
                    break;
                case 12:
                    MonthName = "December";
                    break;
            }
            return MonthName;
        }

        public DataTable GetCencusByFacilityAndQuarter(int FacilityId,int ReportQuarter,int Year)
        {
            DataTable dataTable = new DataTable();
            try
            { 
                var data =  (from c in Context.Census join f in Context.Facilities on c.FacilityId equals f.Id where c.FacilityId == FacilityId && c.ReportQuarter == ReportQuarter && c.IsActive && f.IsActive && c.Year == Year
                             select new  {
                    FacilityName = f.FacilityName,
                    Month = c.Month,
                    Medicad = c.Medicad,
                    Medicare = c.Medicare,
                    Other = c.Other,
                    Year = c.Year
                }).ToList();
                
                dataTable.Columns.Clear();
                dataTable.Columns.Add(new DataColumn("FacilityName"));
                dataTable.Columns.Add(new DataColumn("Year"));
                dataTable.Columns.Add(new DataColumn("Month"));
                dataTable.Columns.Add(new DataColumn("Medicad"));
                dataTable.Columns.Add(new DataColumn("Medicare"));
                dataTable.Columns.Add(new DataColumn("Other"));
                foreach (var item in data)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["FacilityName"] = item.FacilityName;
                    dataRow["Year"] = item.Year;
                    dataRow["Month"] = GetMonth(item.Month);
                    dataRow["Medicad"] = item.Medicad;
                    dataRow["Medicare"] = item.Medicare;
                    dataRow["Other"] = item.Other;
                    dataTable.Rows.Add(dataRow); 
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetCencusexportByFacilityAndQuarter :{ErrorMsg}", ex.Message);
            }
            return dataTable;
        }
         
    }
}
