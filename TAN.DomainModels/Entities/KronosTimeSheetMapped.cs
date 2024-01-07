using System.ComponentModel.DataAnnotations;

namespace TAN.DomainModels.Entities
{
    public class KronosTimeSheetMapped
    {
        [Key]
        public int rowId { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public int PayTypeCode { get; set; }
        public int JobTitleCode { get; set; }
        public int Month { get; set; }
        public int ReportQuarter { get; set; }
        public DateTime Workday { get; set; }
        public decimal THours { get; set; }
        public int FacilityId { get; set; }
        public int UploadType { get; set; }
        public int Year { get; set; }
        public DateTime CreateDate { get; set; }
        public string Createby { get; set; }
        public int OrganizationId { get; set; } = 0;
    }
}
