using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class Census
    {
        [Key]
        public int CensusId { get; set; }
        public int FacilityId { get; set;}
        public int ReportQuarter { get; set;}
        public int Month { get; set;}
        public int Year { get; set;}
        public int Medicare { get; set;}
        public int Medicad { get; set;}
        public int Other { get; set;}
        public DateTime CreateDate { get; set;}
        public string? CreateBy { get; set;}
        public DateTime UpdateDate { get; set;}
        public string? UpdateBy { get; set;}
        public bool IsActive { get; set;}
    }
}
