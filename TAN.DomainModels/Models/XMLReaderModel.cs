using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TAN.DomainModels.Models
{
    [ExcludeFromCodeCoverage]
    public class XMLReaderModel
    {
        public class StaffHours
        {
            public string employeeId;
            public WorkDays workDays;

            public StaffHours()
            {
                workDays = new WorkDays();
            }
        }
        public class WorkDays
        {
            [XmlElement(elementName: "workDay")]
            public List<WorkDay> workDay;

            public WorkDays()
            {
                workDay = new List<WorkDay>();
            }
        }

        public class StaffingHours
        {
            [XmlAttribute(attributeName: "processType")]
            public string processType;
            [XmlElement(elementName: "staffHours")]
            public List<StaffHours> staffingHours { get; set; }
            public StaffingHours()
            {
                staffingHours = new List<StaffHours>();
            }
        }

        [XmlRoot(elementName: "nursingHomeData")]
        public class NursingHomeData
        {
            [XmlElement(elementName: "staffingHours")]
            public StaffingHours StaffingHours;
             
            public string ToXml()
            {

                StringWriter xml = new StringWriter();

                var serializer = new XmlSerializer(typeof(NursingHomeData));
                serializer.Serialize(xml, this);
                return xml.ToString();
            }
        }
        public class WorkDay
        {
            private string myDate;
            [XmlElement(elementName: "date", Order = 1)]
            public string date
            {
                get
                {
                    return myDate;
                }
                set
                {
                    myDate = value;
                }
            }
            [XmlElement(elementName: "hourEntries", Order = 2)]
            public HourEntries hourEntries;

            public WorkDay()
            {
                hourEntries = new HourEntries();
            }
              
        }

        public class HourEntry
        {
            public decimal hours { get; set; }
            public int jobTitleCode { get; set; }
            public int payTypeCode { get; set; }

        }

        public class HourEntries
        {
            [XmlElement(elementName: "hourEntry")]
            public List<HourEntry> hourEntry;

            public HourEntries()
            {
                hourEntry = new List<HourEntry>();
            }
        }
    }
}
