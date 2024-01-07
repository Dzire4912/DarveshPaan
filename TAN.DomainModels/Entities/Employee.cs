using System.ComponentModel.DataAnnotations;

namespace TAN.DomainModels.Entities
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string? EmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int JobTitleCode { get; set; }
        public int PayTypeCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
