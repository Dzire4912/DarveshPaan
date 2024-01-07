using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class ChartData
    {
        public string[] Labels { get; set; }
        public int[] Data { get; set; }
    }
}
