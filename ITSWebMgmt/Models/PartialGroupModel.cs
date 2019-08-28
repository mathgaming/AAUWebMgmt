using ITSWebMgmt.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class PartialGroupModel : WebMgmtModel<ADcache>
    {
        public List<string> GroupList { get; set; }
        public List<string> GroupListAll { get; set; }
        public string Data { get; set; }
        public string GroupSegment { get; set; }
        public string GroupsAllSegment { get; set; }
        public string AttributeName { get; set; }
        public string Title { get; set; }

        public List<string> getGroups(string name) => ADcache.getGroups(name);
        public List<string> getGroupsTransitive(string name) => ADcache.getGroupsTransitive(name);

        public PartialGroupModel(ADcache aDcache, string attributeName, string title = "Groups")
        {
            ADcache = aDcache;
            AttributeName = attributeName;
            Title = title;
        }
    }
}
