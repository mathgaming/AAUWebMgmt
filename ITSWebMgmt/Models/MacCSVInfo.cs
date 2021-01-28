using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class MacCSVInfo
    {
        public string Name { get; set; } // 5
        public string Specs { get; set; } // 6
        public string ComputerType { get; set; } // 7
        public string InvoiceNumber { get; set; } // 12
        [Key]
        public string SerialNumber { get; set; } // 27
        public string OESSAssetNumber { get; set; } // 65
        public string AAUNumber { get; set; } // 66
    }
}
