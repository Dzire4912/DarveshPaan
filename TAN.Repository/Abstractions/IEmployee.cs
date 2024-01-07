using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using static TAN.Repository.Implementations.EmployeeRepository;

namespace TAN.Repository.Abstractions
{
    public interface IEmployee : IRepository<Employee>
    {
        IEnumerable<EmployeeData> GetActiveEmployeesData(int pageSize, int pageNo, EmployeeListRequest employeeList, out int totalCount, string search, string sortOrder);
        Task<bool> AddEmployee(Employee employee);
        Task<List<Employee>> GetEmployeeDetails(List<string> EmployeeIds, int FacilityId);
        Task<List<SearchResponse>> GetEmployeeIdAndName(string SearchValue, int FacilityId);
        Task<Employee> CheckDuplicateEmployeeData(string empId, int facilityId);
        Task<List<Itemlist>> GetEmployeeLabourData(int OrgId);
        Task<List<Itemlist>> GetEmployeeLabourTable(int OrgId, int facilityId, int fiscal, int year, int month);
        Task<List<Itemlist>> GetTopJobTitleTable(int OrgId, int facilityId, int fiscal, int year, int month);
        Task<List<Itemlist>> GetStaffingData(int OrgId, int facilityId, int year);
        Task<int> GetOrganizationEmpCount(int OrgId);
        Task<int> GetPendingStaffingdata(int reportingQuarter, int OrgId);
        IEnumerable<EmployeeData> GetAllEmployeesData(int PageSize, int skip, string FacilityId, out int TotalCount, string search, string sortOrder);
        bool UpdateEmployeeStatus(int employeeId, bool status);
        Task<List<User>> filterorgUsers(List<User> myusers);
        Task<int> activeOrgClients();
        Task<List<User>> listUsers(string cUser, string appId, int OrgId);
        Task<Employee> GetEmployeeById(GetEmployeeDetailsRequest request);
        IEnumerable<CustomEmployeeData> GetSearchedEmployeesData(EmployeeListRequest request);
        Task<EmployeeById> GetEmployeeData(string employeeId, int facilityId);
        Task<List<Timesheet>> MapKronosTimeSheet(List<string> str,DateTime? min,DateTime? max);
    }
}
