using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TAN.DomainModels.Models
{
    public class GetQuarter
    {
        public List<QuarterDetails> GetQuarterById()
        {
            List<QuarterDetails> list = new List<QuarterDetails>
        {
            new QuarterDetails{ Id = 1, QuaterTitle = "October 1 - December 31" },
            new QuarterDetails{ Id = 2, QuaterTitle = "January 1 - March 31" },
            new QuarterDetails{ Id = 3, QuaterTitle = "April 1 - June 30" },
            new QuarterDetails{ Id = 4, QuaterTitle = "July 1 - September 30" },
        };

            return list;
        }
    }
    public class QuarterDetails
    {
        public int Id { get; set; }
        public string? QuaterTitle { get; set; }
    }
}
