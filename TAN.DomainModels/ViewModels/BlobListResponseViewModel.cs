using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class BlobListResponseViewModel
    {
        public IEnumerable<BlobInfoViewModel> Files { get; set; }
        public int TotalCount { get; set; }
    }
}
