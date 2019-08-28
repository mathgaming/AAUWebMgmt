using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class TabModel
    {
        public string TabId { get; set; }
        public string TabName { get; set; }
        public bool ShowLoader { get; set; } = false;

        public TabModel(string tabId, string tabName)
        {
            TabId = tabId;
            TabName = tabName;
        }

        public TabModel(string tabId, string tabName, bool showLoader) : this(tabId, tabName)
        {
            ShowLoader = showLoader;
        }
    }
}
