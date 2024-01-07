using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TAN.DomainModels.Models
{
    [XmlRoot("nursingHomeData")]
    public class NursingHomeData
    {
        [XmlElement("header")]
        public Header Header { get; set; } = new Header();

        [XmlArray("employees")]
        [XmlArrayItem("employee")]
        public List<EmployeeIdData> Employees { get; set; } = new List<EmployeeIdData>();

        [XmlArray("staffingHours")]
        [XmlArrayItem("staffHours")]
        public List<StaffHours> StaffingHours { get; set; } = new List<StaffHours>();
    }

    public class Header
    {
        [XmlElement("facilityId")]
        public string FacilityId { get; set; }

        [XmlElement("stateCode")]
        public string StateCode { get; set; }

        [XmlElement("reportQuarter")]
        public int ReportQuarter { get; set; }

        [XmlElement("federalFiscalYear")]
        public int FederalFiscalYear { get; set; }

        [XmlElement("softwareVendorName")]
        public string SoftwareVendorName { get; set; }

        [XmlElement("softwareVendorEmail")]
        public string SoftwareVendorEmail { get; set; }

        [XmlElement("softwareProductName")]
        public string SoftwareProductName { get; set; }

        [XmlElement("softwareProductVersion")]
        public string SoftwareProductVersion { get; set; }
    }

    public class EmployeeIdData
    {
        [XmlElement("employeeId")]
        public string? EmployeeId { get; set; }
    }

    public class StaffHours
    {
        [XmlElement("employeeId")]
        public string? EmployeeId { get; set; }

        [XmlElement("workDays")]
        public WorkDays WorkDays { get; set; }
    }

    public class WorkDays
    {
        [XmlElement("workDay")]
        public List<WorkDay> WorkDayList { get; set; }
    }

    public class WorkDay
    {
        [XmlElement("date")]
        public DateTime Date { get; set; }

        [XmlElement("hourEntries")]
        public HourEntries HourEntries { get; set; }
    }

    public class HourEntries
    {
        [XmlElement("hourEntry")]
        public List<HourEntry>  HourEntry { get; set; }
    }

    public class HourEntry
    {
        [XmlElement("hours")]
        public double Hours { get; set; }

        [XmlElement("jobTitleCode")]
        public int JobTitleCode { get; set; }

        [XmlElement("payTypeCode")]
        public int PayTypeCode { get; set; }
    }
}
