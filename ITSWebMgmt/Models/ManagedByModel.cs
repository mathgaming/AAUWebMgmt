using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ManagedByModel
    {
        public string adpath { get; set; }
        public string ManagedByADPath { get; set; }
        public string ManagedByDomainAndName { get; set; }
        public ManagedByModel(string adpath, string managedByPath, string managedByName)
        {
            this.adpath = adpath;
            ManagedByADPath = managedByPath;
            ManagedByDomainAndName = managedByName;
        }
    }
}
