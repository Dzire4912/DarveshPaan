using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.ViewModels
{
    public class TileViewModel
    {
        public long TileValue { get; set; }
        public string TileSubText { get; set; }
        public string TileText { get; set; } = string.Empty;
        public string TileID { get; set; } = string.Empty;
        public bool TileArrow { get; set; }
        public string TileArrowText { get; set; }
        public long TileNumber { get; set; }
        public string TileToolTipText { get; set; }
        public string TileFunctionTrigger { get; set; }
    }
}
