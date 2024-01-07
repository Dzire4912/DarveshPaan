using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class EmployeeVM
    {
        public int Id { get; set; }
        public string? EmployeeId { get; set; }
        public string? Name { get; set; }

        public string? PayType { get; set; }
        public string? JobTitle { get; set; }
        public string? HireDate { get; set; }
        public string? TerminationDate { get; set; }
        public string? FacilityId { get; set; }
        public string? LabourTitle { get; set; }
        public string? FicalQuarter { get; set; }
        public string? year { get; set; }
        public string? Month { get; set; }
        public int MonthValue { get; set; }
        public string? FacilityName { get; set;}


    }

}
