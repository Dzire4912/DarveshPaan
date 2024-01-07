using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class EmployeeRepository : Repository<Employee>, IEmployee
    {
        private readonly IHttpContextAccessor _httpContext;
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public EmployeeRepository(DbContext db, IHttpContextAccessor httpContext)
        {
            this.db = db;
            _httpContext = httpContext;
        }
        public IEnumerable<EmployeeData> GetActiveEmployeesData(int PageSize, int skip, EmployeeListRequest employeeList, out int TotalCount, string search, string sortOrder)
        {
            var lowercaseSearchValue = search?.ToLower();
            TotalCount = 0;
            try
            {
                GenerateDatesByYearQuarterMonth generate = new GenerateDatesByYearQuarterMonth(Convert.ToInt32(employeeList.ReportQuarter), Convert.ToInt32(employeeList.Month), Convert.ToInt32(employeeList.Year));

                var employeeData = (from e in Context.Employee
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    where e.IsActive && (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000" ||
                                    e.TerminationDate > DateTime.Now || (e.TerminationDate >= generate.StartDate && e.TerminationDate <= generate.EndDate))
                                    orderby e.EmployeeId
                                    select new EmployeeData
                                    {
                                        Id = e.Id,
                                        EmployeeId = e.EmployeeId.ToString(),
                                        FirstName = (e.FirstName == null) ? "" : e.FirstName.ToString(),
                                        LastName = (e.LastName == null) ? "" : e.LastName.ToString(),
                                        HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" :
                                                        e.HireDate.ToString("MM-dd-yyyy"),
                                        TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        FacilityId = e.FacilityId.ToString(),
                                        FacilityName = f.FacilityName,
                                        CreateDate = (e.CreateDate != null && e.CreateDate != DateTime.MinValue) ? e.CreateDate.Value.ToString("MM-dd-yyyy") : "",
                                        EmployeeFullName = ((e.FirstName == null) ? "" : e.FirstName.ToString()) + " " + ((e.LastName == null) ? "" : e.LastName.ToString()),
                                        IsActive = e.IsActive
                                    }).AsEnumerable();


                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    employeeData = ApplySearch(employeeData, lowercaseSearchValue);
                }

                if (!string.IsNullOrEmpty(employeeList.FacilityId))
                {
                    employeeData = employeeData.Where(empData => empData.FacilityId == employeeList.FacilityId).ToList();
                }

                TotalCount = employeeData.Count();
                employeeData = employeeData.Skip(skip).Take(PageSize).ToList();

                if (!string.IsNullOrEmpty(sortOrder))
                {

                    switch (sortOrder)
                    {
                        case "employeeid_asc":
                            employeeData = employeeData.OrderBy(x => x.EmployeeId).ToList();
                            break;
                        case "employeeid_desc":
                            employeeData = employeeData.OrderByDescending(x => x.EmployeeId).ToList();
                            break;
                        case "employeefullname_asc":
                            employeeData = employeeData.OrderBy(x => x.FirstName + " " + x.LastName).ToList();
                            break;
                        case "employeefullname_desc":
                            employeeData = employeeData.OrderByDescending(x => x.FirstName).ToList();
                            break;
                        case "hiredate_asc":
                            employeeData = employeeData.OrderBy(x => x.HireDate).ToList();
                            break;
                        case "hiredate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.HireDate).ToList();
                            break;
                        case "terminationdate_asc":
                            employeeData = employeeData.OrderBy(x => x.TerminationDate).ToList();
                            break;
                        case "terminationdate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.TerminationDate).ToList();
                            break;
                        case "facilityname_asc":
                            employeeData = employeeData.OrderBy(x => x.FacilityName).ToList();
                            break;
                        case "facilityname_desc":
                            employeeData = employeeData.OrderByDescending(x => x.FacilityName).ToList();
                            break;
                        case "paytype_asc":
                            employeeData = employeeData.OrderBy(x => x.PayType).ToList();
                            break;
                        case "paytype_desc":
                            employeeData = employeeData.OrderByDescending(x => x.PayType).ToList();
                            break;
                        case "jobtitle_asc":
                            employeeData = employeeData.OrderBy(x => x.JobTitle).ToList();
                            break;
                        case "jobtitle_desc":
                            employeeData = employeeData.OrderByDescending(x => x.JobTitle).ToList();
                            break;
                        case "createdate_asc":
                            employeeData = employeeData.OrderBy(x => x.CreateDate).ToList();
                            break;
                        case "createdate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.CreateDate).ToList();
                            break;
                    }
                }
                return employeeData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                TotalCount = 0;
                return Enumerable.Empty<EmployeeData>();
            }
        }

        public IEnumerable<EmployeeData> GetAllEmployeesData(int PageSize, int skip, string FacilityId, out int TotalCount, string search, string sortOrder)
        {
            var lowercaseSearchValue = search?.ToLower();
            TotalCount = 0;
            try
            {
                var employeeData = (from e in Context.Employee
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    orderby e.EmployeeId
                                    select new EmployeeData
                                    {
                                        Id = e.Id,
                                        EmployeeId = e.EmployeeId.ToString(),
                                        FirstName = (e.FirstName == null) ? "" : e.FirstName.ToString(),
                                        LastName = (e.LastName == null) ? "" : e.LastName.ToString(),
                                        HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" :
                                                        e.HireDate.ToString("MM-dd-yyyy"),
                                        TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        FacilityId = e.FacilityId.ToString(),
                                        FacilityName = f.FacilityName,
                                        CreateDate = (e.CreateDate != null && e.CreateDate != DateTime.MinValue) ? e.CreateDate.Value.ToString("MM-dd-yyyy") : "",
                                        EmployeeFullName = ((e.FirstName == null) ? "" : e.FirstName.ToString()) + " " + ((e.LastName == null) ? "" : e.LastName.ToString()),
                                        IsActive = e.IsActive
                                    }).AsEnumerable();


                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    employeeData = ApplySearch(employeeData, lowercaseSearchValue);
                }

                if (!string.IsNullOrEmpty(FacilityId))
                {
                    employeeData = employeeData.Where(empData => empData.FacilityId == FacilityId).ToList();
                }

                TotalCount = employeeData.Count();
                employeeData = employeeData.Skip(skip).Take(PageSize).ToList();

                if (!string.IsNullOrEmpty(sortOrder))
                {

                    switch (sortOrder)
                    {
                        case "employeeid_asc":
                            employeeData = employeeData.OrderBy(x => x.EmployeeId).ToList();
                            break;
                        case "employeeid_desc":
                            employeeData = employeeData.OrderByDescending(x => x.EmployeeId).ToList();
                            break;
                        case "employeefullname_asc":
                            employeeData = employeeData.OrderBy(x => x.FirstName + " " + x.LastName).ToList();
                            break;
                        case "employeefullname_desc":
                            employeeData = employeeData.OrderByDescending(x => x.FirstName).ToList();
                            break;
                        case "hiredate_asc":
                            employeeData = employeeData.OrderBy(x => x.HireDate).ToList();
                            break;
                        case "hiredate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.HireDate).ToList();
                            break;
                        case "terminationdate_asc":
                            employeeData = employeeData.OrderBy(x => x.TerminationDate).ToList();
                            break;
                        case "terminationdate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.TerminationDate).ToList();
                            break;
                        case "facilityname_asc":
                            employeeData = employeeData.OrderBy(x => x.FacilityName).ToList();
                            break;
                        case "facilityname_desc":
                            employeeData = employeeData.OrderByDescending(x => x.FacilityName).ToList();
                            break;
                        case "paytype_asc":
                            employeeData = employeeData.OrderBy(x => x.PayType).ToList();
                            break;
                        case "paytype_desc":
                            employeeData = employeeData.OrderByDescending(x => x.PayType).ToList();
                            break;
                        case "jobtitle_asc":
                            employeeData = employeeData.OrderBy(x => x.JobTitle).ToList();
                            break;
                        case "jobtitle_desc":
                            employeeData = employeeData.OrderByDescending(x => x.JobTitle).ToList();
                            break;
                        case "createdate_asc":
                            employeeData = employeeData.OrderBy(x => x.CreateDate).ToList();
                            break;
                        case "createdate_desc":
                            employeeData = employeeData.OrderByDescending(x => x.CreateDate).ToList();
                            break;
                    }
                }
                return employeeData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                TotalCount = 0;
                return Enumerable.Empty<EmployeeData>();
            }
        }

        private IEnumerable<EmployeeData> ApplySearch(IEnumerable<EmployeeData> query, string lowercaseSearchValue)
        {

            query = query.Where(emp =>
            emp.EmployeeId.ToString().Contains(lowercaseSearchValue) ||
            emp.FirstName.ToLower().Contains(lowercaseSearchValue) ||
            emp.LastName.ToLower().Contains(lowercaseSearchValue) ||
            emp.PayType.ToLower().Contains(lowercaseSearchValue) ||
            emp.JobTitle.ToLower().Contains(lowercaseSearchValue) ||
            emp.HireDate.Contains(lowercaseSearchValue) ||
            emp.TerminationDate.Contains(lowercaseSearchValue) ||
            emp.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
            emp.CreateDate.Contains(lowercaseSearchValue));

            return query;
        }

        private IEnumerable<CustomEmployeeData> ApplySearchOnCustomEmployeeData(IEnumerable<CustomEmployeeData> query, string lowercaseSearchValue)
        {
            query = query.Where(emp =>
            emp.EmployeeId.ToString().Contains(lowercaseSearchValue) ||
            emp.FirstName.ToLower().Contains(lowercaseSearchValue) ||
            emp.LastName.ToLower().Contains(lowercaseSearchValue) ||
            emp.PayType.ToLower().Contains(lowercaseSearchValue) ||
            emp.JobTitle.ToLower().Contains(lowercaseSearchValue) ||
            emp.HireDate.Contains(lowercaseSearchValue) ||
            emp.TerminationDate.Contains(lowercaseSearchValue) ||
            emp.FacilityName.ToLower().Contains(lowercaseSearchValue) ||
            emp.CreateDate.Contains(lowercaseSearchValue));

            return query;
        }

        public async Task<bool> AddEmployee(Employee employee)
        {
            try
            {
                if (employee != null)
                {
                    if (employee.Id == 0)
                    {
                        Context.Employee.Add(employee);
                        Context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        Context.Employee.Update(employee);
                        Context.SaveChanges();
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

        public async Task<List<Employee>> GetEmployeeDetails(List<string> EmployeeIds, int FacilityId)
        {
            List<Employee> EmpList = new List<Employee>();
            try
            {
                EmpList = await Context.Employee
                    .Where(item => EmployeeIds.Contains(item.EmployeeId) && item.IsActive && item.FacilityId == FacilityId).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeDetails :{ErrorMsg}", ex.Message);
            }
            return EmpList;
        }

        public async Task<List<SearchResponse>> GetEmployeeIdAndName(string SearchValue, int FacilityId)
        {
            List<SearchResponse> empList = new List<SearchResponse>();
            try
            {
                empList = await Context.Employee.Where(x => x.FacilityId == FacilityId &&
                (x.EmployeeId.Contains(SearchValue.ToLower()) ||
                x.FirstName.Contains(SearchValue.ToLower()) ||
                x.LastName.Contains(SearchValue.ToLower()))).Select(s => new SearchResponse
                {
                    Id = s.Id,
                    Text = s.EmployeeId + " - " + s.FirstName + ", " + s.LastName,
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeDetails :{ErrorMsg}", ex.Message);
            }
            return empList;
        }

        public async Task<List<Itemlist>> GetEmployeeLabourData(int OrgId)
        {
            List<EmployeeVM> employeeData = new List<EmployeeVM>();
            var items = new List<Itemlist>();
            try
            {
                if (OrgId > 0)
                {
                    employeeData = (from e in Context.Employee
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.OrganizationId == OrgId && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = e.LastName.ToString() != null ? e.LastName.ToString() : "" + ", " + e.FirstName.ToString() != null ? e.FirstName.ToString() : "",
                                        HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" :
                                                    e.HireDate.ToString("MM-dd-yyyy"),
                                        TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description
                                    }).ToList();
                    items = employeeData.GroupBy(c => c.LabourTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }
                else
                {
                    employeeData = (from e in Context.Employee
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        LabourTitle = lc.Description
                                        /* Name = e.LastName.ToString() != null ? e.LastName.ToString() : "" + ", " + e.FirstName.ToString() != null ? e.FirstName.ToString() : "",
                                         HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" :
                                                     e.HireDate.ToString("MM-dd-yyyy"),
                                         TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                         PayType = pc.PayTypeDescription,
                                         JobTitle = jc.Title*/

                                    }).ToList();
                    items = employeeData.GroupBy(c => c.LabourTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);

            }
            return items;
        }
        public async Task<List<Itemlist>> GetEmployeeLabourTable(int OrgId, int facilityId, int fiscal, int year, int month)
        {
            List<EmployeeVM> employeeData = new List<EmployeeVM>();
            var items = new List<Itemlist>();
            try
            {
                if (OrgId > 0)
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.OrganizationId == OrgId && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = "NA",
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = e.Month.ToString()
                                    }).ToList();
                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }
                    if (fiscal != 0)
                    {
                        employeeData = employeeData.Where(x => x.FicalQuarter == fiscal.ToString()).ToList();
                    }
                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }
                    if (month != 0)
                    {
                        employeeData = employeeData.Where(x => x.Month == month.ToString()).ToList();
                    }
                    items = employeeData.GroupBy(c => c.LabourTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }
                else
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = "NA",
                                        HireDate = DateTime.Now.ToString(),
                                        TerminationDate = DateTime.UtcNow.ToString(),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = e.Month.ToString(),
                                        FacilityName = f.FacilityName
                                    }).ToList();
                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }
                    if (fiscal != 0)
                    {
                        employeeData = employeeData.Where(x => x.FicalQuarter == fiscal.ToString()).ToList();
                    }
                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }
                    if (month != 0)
                    {
                        employeeData = employeeData.Where(x => x.Month == month.ToString()).ToList();
                    }


                    items = employeeData.GroupBy(c => c.LabourTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);

            }
            return items;
        }

        public async Task<List<Itemlist>> GetTopJobTitleTable(int OrgId, int facilityId, int fiscal, int year, int month)
        {
            List<EmployeeVM> employeeData = new List<EmployeeVM>();
            var items = new List<Itemlist>();
            try
            {
                if (OrgId > 0)
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.OrganizationId == OrgId && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = (e.LastName != null && e.FirstName != null) ? (e.LastName.ToString() + ", " + e.FirstName.ToString()) : null,
                                        HireDate = DateTime.Now.ToString(),
                                        TerminationDate = DateTime.UtcNow.ToString(),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = e.Month.ToString()
                                    }).ToList();
                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }
                    if (fiscal != 0)
                    {
                        employeeData = employeeData.Where(x => x.FicalQuarter == fiscal.ToString()).ToList();
                    }
                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }
                    if (month != 0)
                    {
                        employeeData = employeeData.Where(x => x.Month == month.ToString()).ToList();
                    }
                    //Get Count and Jobtitles out of employeeData 
                    items = employeeData.GroupBy(c => c.JobTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }
                else
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    where e.IsActive && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = (e.LastName != null && e.FirstName != null) ? (e.LastName.ToString() + ", " + e.FirstName.ToString()) : null,
                                        HireDate = DateTime.Now.ToString(),
                                        TerminationDate = DateTime.UtcNow.ToString(),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = e.Month.ToString()

                                    }).ToList();

                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }
                    if (fiscal != 0)
                    {
                        employeeData = employeeData.Where(x => x.FicalQuarter == fiscal.ToString()).ToList();
                    }
                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }
                    if (month != 0)
                    {
                        employeeData = employeeData.Where(x => x.Month == month.ToString()).ToList();
                    }

                    //Get Count and Jobtitles out of employeeData 
                    items = employeeData.GroupBy(c => c.JobTitle)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                }


            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);

            }
            return items;
        }

        public async Task<List<Itemlist>> GetStaffingData(int OrgId, int facilityId, int year)
        {
            List<EmployeeVM> employeeData = new List<EmployeeVM>();
            var items = new List<Itemlist>();
            try
            {
                if (OrgId > 0)
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    join mo in Context.Months on e.Workday.Month equals mo.Id
                                    where e.IsActive && f.OrganizationId == OrgId && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = (e.LastName != null && e.FirstName != null) ? (e.LastName.ToString() + ", " + e.FirstName.ToString()) : "",
                                        HireDate = DateTime.Now.ToString(),
                                        TerminationDate = DateTime.UtcNow.ToString(),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FacilityName = f.FacilityName,
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = mo.Name.ToString(),
                                        MonthValue = e.Workday.Month
                                    }).ToList();
                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }

                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }

                    //Get Count and Jobtitles out of employeeData 
                    items = employeeData.GroupBy(c => c.Month)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                    var monthSelection = Context.Months.Select(x => x.Name).ToList();
                    foreach (var month in monthSelection)
                    {
                        // Check if the month is not present in the existing items list
                        if (!items.Any(item => item.Text == month))
                        {
                            // Add a new Itemlist with the missing month and count 0
                            items.Add(new Itemlist { Text = month, Value = 0 });
                        }
                    }
                    var months = Context.Months.ToList();
                    var orderedItems = (from e in months
                                        join f in items on e.Name equals f.Text
                                        orderby e.Id
                                        select new Itemlist { Text = f.Text, Value = f.Value }).ToList();
                    items = orderedItems;
                }
                else
                {
                    employeeData = (from e in Context.Timesheet
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    join lc in Context.LaborCodes on jc.LabourCode equals lc.Id
                                    join mo in Context.Months on e.Month equals mo.Id
                                    where e.IsActive && f.IsActive
                                    select new EmployeeVM
                                    {
                                        EmployeeId = e.EmployeeId,
                                        Name = "NA",
                                        HireDate = DateTime.Now.ToString(),
                                        TerminationDate = DateTime.UtcNow.ToString(),
                                        PayType = pc.PayTypeDescription,
                                        JobTitle = jc.Title,
                                        LabourTitle = lc.Description,
                                        FacilityId = f.Id.ToString(),
                                        FacilityName = f.FacilityName,
                                        FicalQuarter = e.ReportQuarter.ToString(),
                                        year = e.Year.ToString(),
                                        Month = mo.Name.ToString()

                                    }).ToList();

                    if (facilityId != 0)
                    {
                        employeeData = employeeData.Where(x => x.FacilityId == facilityId.ToString()).ToList();
                    }

                    if (year != 0)
                    {
                        employeeData = employeeData.Where(x => x.year == year.ToString()).ToList();
                    }


                    //Get Count and Jobtitles out of employeeData 
                    items = employeeData.GroupBy(c => c.Month)
                  .Select(group => new Itemlist { Text = group.Key, Value = group.Count() }).ToList();
                    var monthSelection = Context.Months.Select(x => x.Name).ToList();
                    foreach (var month in monthSelection)
                    {
                        // Check if the month is not present in the existing items list
                        if (!items.Any(item => item.Text == month))
                        {
                            // Add a new Itemlist with the missing month and count 0
                            items.Add(new Itemlist { Text = month, Value = 0 });
                        }
                    }
                    var months = Context.Months.ToList();
                    var orderedItems = (from e in months
                                        join f in items on e.Name equals f.Text
                                        orderby e.Id
                                        select new Itemlist { Text = f.Text, Value = f.Value }).ToList();
                    items = orderedItems;
                }


            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);

            }
            return items;
        }
        public async Task<int> GetOrganizationEmpCount(int OrgId)
        {
            int count = 0;
            var empcount = from e in Context.Employee
                           join f in Context.Facilities on e.FacilityId equals f.Id
                           where f.OrganizationId == OrgId && f.IsActive && e.IsActive
                           select e.EmployeeId;
            count = empcount.Count();

            return count;

        }
        public async Task<int> GetPendingStaffingdata(int reportingQuarter, int OrgId)
        {
            List<Itemlist> emp = new List<Itemlist>();

            if (OrgId > 0)
            {
                emp = (from e in Context.Timesheet
                       join f in Context.Facilities on e.FacilityId equals f.Id
                       join o in Context.Organizations on f.OrganizationId equals o.OrganizationID
                       where e.ReportQuarter == reportingQuarter && e.Year == DateTime.Now.Year && f.IsActive && e.IsActive && o.OrganizationID == OrgId && o.IsActive

                       select new Itemlist
                       {
                           Text = f.FacilityID,
                           Value = Convert.ToInt32(e.Id)

                       }).ToList();

                var empcount = emp.GroupBy(c => c.Text).Select(g => new Itemlist { Text = g.Key, Value = g.Count() }).Count();
                var facility = from f in Context.Facilities.Where(c => c.IsActive && c.OrganizationId == OrgId) select f.FacilityID;
                var facilityCount = facility.Count();
                var ratio = (double)empcount / facilityCount;
                var percentage = Convert.ToInt32(ratio * 100);
                return percentage;
            }
            else
            {
                emp = (from e in Context.Timesheet
                       join f in Context.Facilities on e.FacilityId equals f.Id
                       join o in Context.Organizations on f.OrganizationId equals o.OrganizationID

                       where e.ReportQuarter == reportingQuarter && e.Year == DateTime.Now.Year && f.IsActive && e.IsActive && o.IsActive

                       select new Itemlist
                       {
                           Text = f.FacilityID,
                           Value = Convert.ToInt32(e.Id)

                       }).ToList();

                var empcount = emp.GroupBy(c => c.Text).Select(g => new Itemlist { Text = g.Key, Value = g.Count() }).Count();
                var facility = from f in Context.Facilities.Where(x => x.IsActive) select f.FacilityID;
                var facilityCount = facility.Count();
                var ratio = (double)empcount / facilityCount;
                var percentage = Convert.ToInt32(ratio * 100);
                return percentage;
            }

        }




        public async Task<Employee> CheckDuplicateEmployeeData(string empId, int facilityId)
        {
            Employee employee = new Employee();
            try
            {
                employee = await Context.Employee.Where(x => x.EmployeeId == empId && x.FacilityId == facilityId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured GetEmployeeDetails :{ErrorMsg}", ex.Message);
            }
            return employee;
        }

        public bool UpdateEmployeeStatus(int employeeId, bool status)
        {
            try
            {
                if (employeeId > 0)
                {
                    var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var data = Context.Employee.Where(x => x.Id == employeeId).FirstOrDefault();
                    if (data != null)
                    {
                        data.UpdateDate = DateTime.Now;
                        data.IsActive = status;
                        data.UpdateBy = userId;
                        Context.Employee.Update(data);
                        Context.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured UpdateEmployeeStatus :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<List<User>> filterorgUsers(List<User> myusers)
        {
            var users = new List<User>();

            foreach (var user in myusers)
            {
                if (user.UserType != Convert.ToInt32(UserTypes.Thinkanew))
                {
                    var x = (from uof in Context.UserOrganizationFacilities
                             join f in Context.Facilities on uof.FacilityID equals f.Id
                             join o in Context.Organizations on uof.OrganizationID equals o.OrganizationID
                             where uof.UserId == user.Id && o.IsActive
                             select uof).FirstOrDefault();
                    if (x != null)
                    {
                        users.Add(user);
                    }
                }
                else
                {
                    users.Add(user);
                }

            }

            // To execute the query and get the results



            return users;
        }

        public async Task<List<User>> listUsers(string cUser, string appId, int OrgId)
        {
            List<User> users = new List<User>();
            if (OrgId > 0)
            {
                users = (from a in Context.Users
                         join uof in Context.UserOrganizationFacilities on a.Id equals uof.UserId into userOrgFacilitiesGroup
                         from uof in userOrgFacilitiesGroup.DefaultIfEmpty()
                         join o in Context.Organizations on uof.OrganizationID equals o.OrganizationID into organizationsGroup
                         from o in organizationsGroup.DefaultIfEmpty()
                         join f in Context.Facilities on uof.FacilityID equals f.Id into facilitiesGroup
                         from f in facilitiesGroup.DefaultIfEmpty()
                         join aur in Context.AspNetUserRoles on a.Id equals aur.UserId into userRolesGroup
                         from aur in userRolesGroup.DefaultIfEmpty()
                         join r in Context.Roles on aur.RoleId equals r.Id into rolesGroup
                         from r in rolesGroup.DefaultIfEmpty()
                         join app in Context.Application on a.DefaultAppId equals app.ApplicationId
                         where a.Id != cUser && o.OrganizationID == OrgId && (o.IsActive || o.OrganizationID == null) &&
                         r.Name != "SuperAdmin" &&
                               (r.ApplicationId == appId || r.RoleType == RoleTypes.TANApplicationUser.ToString() || r.Id == null) && (r.Name == null || r.Name != "SuperAdmin")
                         select new User
                         {
                             Id = a.Id,
                             UserName = a.UserName,
                             ActiveStatus = a.IsActive,
                             UserType = a.UserType,
                             Role = r.Name != null ? r.Name : "N/A",
                             UserEmail = a.Email,
                             FirstName = a.FirstName != null ? a.FirstName : "",
                             LastName = a.LastName != null ? a.LastName : "",
                             OrgId = o.OrganizationID != null ? o.OrganizationID : 0,
                             RoleId = r.Id,
                             DefaultAppName = app.Name
                         }).AsNoTracking().ToList();
            }
            else
            {
                users = (from a in Context.Users
                         join uof in Context.UserOrganizationFacilities on a.Id equals uof.UserId into userOrgFacilitiesGroup
                         from uof in userOrgFacilitiesGroup.DefaultIfEmpty()
                         join o in Context.Organizations on uof.OrganizationID equals o.OrganizationID into organizationsGroup
                         from o in organizationsGroup.DefaultIfEmpty()
                         join f in Context.Facilities on uof.FacilityID equals f.Id into facilitiesGroup
                         from f in facilitiesGroup.DefaultIfEmpty()
                         join aur in Context.AspNetUserRoles on a.Id equals aur.UserId into userRolesGroup
                         from aur in userRolesGroup.DefaultIfEmpty()
                         join r in Context.Roles on aur.RoleId equals r.Id into rolesGroup
                         from r in rolesGroup.DefaultIfEmpty()
                         join app in Context.Application on a.DefaultAppId equals app.ApplicationId
                         where a.Id != cUser &&
                               r.Name != "SuperAdmin" && (o.IsActive || o.OrganizationID == null) &&
                               (r.ApplicationId == appId || r.RoleType == RoleTypes.TANApplicationUser.ToString() || r.Id == null) && (r.Name == null || r.Name != "SuperAdmin")
                         select new User
                         {
                             Id = a.Id,
                             UserName = a.UserName,
                             ActiveStatus = a.IsActive,
                             UserType = a.UserType,
                             Role = r.Name != null ? r.Name : "N/A",
                             UserEmail = a.Email,
                             FirstName = a.FirstName != null ? a.FirstName : "",
                             LastName = a.LastName != null ? a.LastName : "",
                             OrgId = o.OrganizationID != null ? o.OrganizationID : 0,
                             FacilityId = f.Id != null ? f.Id : 0,
                             RoleId = r.Id,
                             DefaultAppName = app.Name
                         }).AsNoTracking().Distinct().ToList();
            }

            // To execute the query and get the results



            return users;
        }


        public async Task<int> activeOrgClients()
        {
            int c = 0;
            var distinctUserIds = (from user in Context.Users
                                   join uof in Context.UserOrganizationFacilities on user.Id equals uof.UserId into uofGroup
                                   from uof in uofGroup.DefaultIfEmpty()
                                   join org in Context.Organizations on uof.OrganizationID equals org.OrganizationID into orgGroup
                                   from org in orgGroup.DefaultIfEmpty()
                                   where org.IsActive && user.IsActive && user.UserType == 2
                                   select user.Id).Distinct();
            c = distinctUserIds.Count();


            return c;
        }

        //tsVM 
        // List<timesheet> and List of Different data
        public async Task<List<Timesheet>> MapKronosTimeSheet(List<string> str, DateTime? min, DateTime? max)
        {
            List<Timesheet> ts = new List<Timesheet>();
            try
            {

                var query = (from k in Context.KronosPunchExport
                             join p in Context.KronosPaytypeMappings on k.PayCode equals p.kronosPaytype
                             join j in Context.Facilities on k.facilityId equals j.FacilityID
                             join m in Context.Months on k.ADJUSTEDAPPLYDATE.Value.Month equals m.Id
                             join jc in Context.KronosToPbjMappings
                                         on new { Column1 = k.JOBCODE, Column2 = k.HrJob }
                                     equals new { Column1 = jc.Code.ToString(), Column2 = jc.Job }
                             where str.Contains(k.facilityId) && k.ADJUSTEDAPPLYDATE >= min && k.ADJUSTEDAPPLYDATE <= max

                             select new Timesheet
                             {
                                 EmployeeId = k.EmployeeID,
                                 FirstName = k.EmployeeName,
                                 PayTypeCode = Convert.ToInt32(p.Paytype),
                                 JobTitleCode = jc.PBJJobCode,
                                 Month = m.Id,
                                 ReportQuarter = m.Quarter,
                                 Workday = k.ADJUSTEDAPPLYDATE.Value,
                                 THours = k.TIMEINHOURS.Value,
                                 FacilityId = j.Id,
                                 UploadType = 3,
                                 Year = k.ADJUSTEDAPPLYDATE.Value.Year,
                                 CreateDate = DateTime.Now,
                                 Createby = "AutoSync",

                             }).AsNoTracking().ToList();
                if (query != null)
                {
                    ts = query.ToList();
                }
            }
            catch (Exception ex)
            {
                //log
            }
            return ts;
        }

        public async Task<Employee> GetEmployeeById(GetEmployeeDetailsRequest request)
        {
            Employee employee = null;
            try
            {
                if (request != null)
                {
                    employee = await Context.Employee.FirstOrDefaultAsync(x => x.IsActive &&
                x.FacilityId == Convert.ToInt32(request.FacilityId) &&
                x.EmployeeId == request.EmployeeId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured UpdateEmployeeStatus :{ErrorMsg}", ex.Message);
            }
            return employee;
        }

        public IEnumerable<CustomEmployeeData> GetSearchedEmployeesData(EmployeeListRequest request)
        {
            var lowercaseSearchValue = request.SearchValue?.ToLower();
            try
            {
                GenerateDatesByYearQuarterMonth generate = new GenerateDatesByYearQuarterMonth(Convert.ToInt32(request.ReportQuarter), Convert.ToInt32(request.Month), Convert.ToInt32(request.Year));
                var employeeData = (from e in Context.Employee
                                    join f in Context.Facilities on e.FacilityId equals f.Id
                                    join pc in Context.PayTypeCodes on e.PayTypeCode equals pc.PayTypeCode
                                    join jc in Context.JobCodes on e.JobTitleCode equals jc.Id
                                    where e.IsActive && f.Id == Convert.ToInt32(request.FacilityId) && (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000" ||
                                e.TerminationDate > DateTime.Now || (e.TerminationDate >= generate.StartDate && e.TerminationDate <= generate.EndDate))
                                    orderby e.EmployeeId
                                    select new CustomEmployeeData
                                    {
                                        Id = e.Id,
                                        EmployeeId = e.EmployeeId.ToString(),
                                        FirstName = (e.FirstName == null) ? "" : e.FirstName.ToString(),
                                        LastName = (e.LastName == null) ? "" : e.LastName.ToString(),
                                        HireDate = (e.HireDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" :
                                                        e.HireDate.ToString("MM-dd-yyyy"),
                                        TerminationDate = (e.TerminationDate.ToString() == "0001-01-01 00:00:00.0000000") ? "" : e.TerminationDate.ToString("MM-dd-yyyy"),
                                        PayType = pc.PayTypeDescription,
                                        PayTypeCode = pc.PayTypeCode,
                                        JobTitle = jc.Title,
                                        JobTitleCode = jc.Id,
                                        FacilityId = e.FacilityId.ToString(),
                                        FacilityName = f.FacilityName,
                                        CreateDate = (e.CreateDate != null && e.CreateDate != DateTime.MinValue) ? e.CreateDate.Value.ToString("MM-dd-yyyy") : "",
                                        EmployeeFullName = ((e.FirstName == null) ? "" : e.FirstName.ToString()) + " " + ((e.LastName == null) ? "" : e.LastName.ToString()),
                                        IsActive = e.IsActive
                                    }).AsEnumerable();


                if (!string.IsNullOrEmpty(lowercaseSearchValue))
                {
                    employeeData = ApplySearchOnCustomEmployeeData(employeeData, lowercaseSearchValue);
                }

                return employeeData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured :{ErrorMsg}", ex.Message);
                return Enumerable.Empty<CustomEmployeeData>();
            }
        }
        public async Task<EmployeeById> GetEmployeeData(string employeeId, int facilityId)
        {
            EmployeeById empData = new EmployeeById();
            try
            {
                empData = (from e in Context.Employee
                           join p in Context.PayTypeCodes on e.PayTypeCode equals p.PayTypeCode
                           join j in Context.JobCodes on e.JobTitleCode equals j.Id
                           join f in Context.Facilities on e.FacilityId equals f.Id
                           join o in Context.Organizations on f.OrganizationId equals o.OrganizationID
                           where e.EmployeeId == employeeId && e.FacilityId == facilityId && e.IsActive
                           select new EmployeeById
                           {
                               Id=e.EmployeeId,
                               FullName= ((e.FirstName == null) ? "" : e.FirstName.ToString()) + " " + ((e.LastName == null) ? "" : e.LastName.ToString()),
                               PayTypeName = p.PayTypeDescription,
                               JobTitle = j.Title,
                               FacilityName = f.FacilityName,
                               OrganizationName = o.OrganizationName
                           }).FirstOrDefault();

                return empData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in getemployeedetails in repository :{ErrorMsg}", ex.Message);
            }
            return empData;
        }
    }
}
