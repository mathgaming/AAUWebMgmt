using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class WorkItemRedirectModel
    {
        public string Data { get; set; }
        public string Url { get; set; }

        public WorkItemRedirectModel(string url, string data)
        {
            Url = url;
            Data = data;
        }
    }
}
