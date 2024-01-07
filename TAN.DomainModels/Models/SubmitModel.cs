using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.DomainModels.Models
{
    public class SubmitModel
    {
        public List<FacilityModel> FacilityList { get; set; }
    }

    public class GenerateXml
    {
        public int FacilityId { get; set; }
        public int Year { get; set; }
        public int ReportQuarter { get; set; }
        public int Month { get; set; }
        public string? FileName { get; set; }
    }
}
