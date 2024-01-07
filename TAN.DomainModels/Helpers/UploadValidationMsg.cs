using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
    [ExcludeFromCodeCoverage]
    public class UploadValidationMsg
    {
        public static string? FirstNameNull => "First Name is empty";
        public static string? JobTitleNull => "Job Title is empty";
        public static string? LastNameNull => "Last Name is empty";
        public static string? PayCodeNull => "Pay Code is empty";
        public static string? InvalidDate => "Invalid date";
        public static string? WorkdayIsGreaterThenCurrDay => "Work day is greater then current day";
        public static string? Hours24 => "Total hours must be less than 24 hours and more than 0 hours.";
        public static string? EmpIdNull => "Employee Id is empty";
        public static string? EmpIdLength => "Employee Id must be smaller then 30 characters";
        public static string? ClockPunchIn => "In data is missing";
        public static string? ClockPunchOut => "Out data is missing";
        public static string? WorkdayExist => "The Work day already exists";
        public static string? IsWorkdayValidateWithFilter => "Workday Mapping is incorrect";
        public static string? InvalidJobTitle => "invalid Job Title";
        public static string? InvalidPayCode => "invalid Pay Code";
        public static string? InvalidEmpId => "invalid Employee Id";
        public static string? DifferentQuarterYear => "Work day is not in selected quarter or year";
        public static string? HireTerminationDate => "The workday record does not come between your hire and termination dates.";
        public static string? EmpIdHasSpace => "Employee Id should not has space";
        public static string? FullNameNull => "Full Name is null or empty";
        public static string? NameNull => "Name field is null or empty";

    }
}
