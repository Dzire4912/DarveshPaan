using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class StorageRequestViewModel
    {
            public string? FileName { get; set; }
            public string? FileURL { get; set; }
            public bool IsOneDrive { get; set; }
    }
}
