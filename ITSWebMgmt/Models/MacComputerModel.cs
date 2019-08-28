using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class MacComputerModel
    {
        public ComputerModel BaseModel { get; set; }
        public MacComputerModel(ComputerModel baseModel)
        {
            BaseModel = baseModel;
        }
    }
}
