using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class RawSCCMModel
    {
        public ManagementObjectCollection Mo { get; set; }
        public string ErrorMessage { get; set; }
        public RawSCCMModel(ManagementObjectCollection mo, string errorMessage)
        {
            Mo = mo;
            ErrorMessage = errorMessage;
        }
    }
}
