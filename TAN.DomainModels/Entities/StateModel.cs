using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class StateModel
    {
        [Key]
        public int StateID { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int DisplayOrder { get; set; }

    }
}
