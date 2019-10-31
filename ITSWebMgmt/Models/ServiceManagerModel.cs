using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ServiceManagerModel
    {
        public string userID;
        public List<Case> userCases;

        public ServiceManagerModel(Dictionary<string, object> userJson)
        {

        }
    }
    
    public class Case
    {

    }
}
