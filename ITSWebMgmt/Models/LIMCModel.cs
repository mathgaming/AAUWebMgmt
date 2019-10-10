using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class LIMCModel
    {
        public string RawCSVInput { get; set; }
        public string MailOutput { get; set; }
        public LIMCModel(){ }
        public LIMCModel(string inputCSV)
        {
            RawCSVInput = inputCSV;
        }
    }
}
