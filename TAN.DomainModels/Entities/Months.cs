using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    public class Months
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quarter {  get; set; }
    }
}
