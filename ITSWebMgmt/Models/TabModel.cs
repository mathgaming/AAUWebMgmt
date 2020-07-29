namespace ITSWebMgmt.Models
{
    public class TabModel
    {
        public string TabId { get; set; } // The id used for java scripts
        public string TabName { get; set; } // The text that is shown on the tab
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
