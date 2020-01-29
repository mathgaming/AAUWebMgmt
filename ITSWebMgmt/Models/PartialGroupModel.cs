using ITSWebMgmt.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.Models
{
    public class PartialGroupModel : WebMgmtModel<ADcache>
    {
        public List<string> GroupList { get; set; }
        public List<string> GroupAllList { get; set; }
        public TableModel GroupTable { get; set; }
        public TableModel GroupAllTable { get; set; }
        public TableModel FilteredTable { get; set; }
        public string AttributeName { get; set; }
        public string Title { get; set; }
        public string AccessName { get; set; }

        public List<string> getGroups(string name) => ADcache.getGroups(name);
        public List<string> getGroupsTransitive(string name) => ADcache.getGroupsTransitive(name);

        public PartialGroupModel(ADcache aDcache, string attributeName, string title = "Groups", string accessName = null)
        {
            ADcache = aDcache;
            AttributeName = attributeName;
            Title = title;
            AccessName = accessName;

            Init();
        }

        private void Init()
        {
            GroupList = SortGroupMembers(ADcache.getGroups(AttributeName));
            GroupAllList = SortGroupMembers(ADcache.getGroupsTransitive(AttributeName));

            if (AccessName != null)
            {
                GroupTable = new TableModel(new string[] { "Name", "Type", "Access" }, new List<string[]>());
                GroupAllTable = new TableModel(new string[] { "Name", "Type", "Access" }, new List<string[]>());

                AddRowsTotable(GroupTable, GroupList, AccessName);
                AddRowsTotable(GroupAllTable, GroupAllList, AccessName);
            }
            else
            {
                GroupTable = new TableModel(new string[] { "Domain", "Name" }, new List<string[]>());
                GroupAllTable = new TableModel(new string[] { "Domain", "Name" }, new List<string[]>());

                AddRowsTotable(GroupTable, GroupList);
                AddRowsTotable(GroupAllTable, GroupAllList);
            }
        }

        public void AddRowsTotable(TableModel groupTable, List<string> groups, string accessName = null)
        {
            foreach (string adpath in groups)
            {
                var split = adpath.Split(',');
                var name = split[0].Replace("CN=", "");
                var domain = split.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
                var link = name;
                var type = "unknown";
                if (!adpath.Contains("OU"))
                {
                    type = "unknown";
                }
                else if (adpath.Contains("OU=Groups"))
                {
                    link = getGroupLink(adpath, name);
                    type = "Group";
                }
                else if (adpath.Contains("OU=Admin Groups"))
                {
                    link = getGroupLink(adpath, name);
                    type = "Admin group";
                }
                else if (adpath.Contains("OU=People"))
                {
                    link = getPersonLink(domain, name);
                    type = "Person";
                }
                else if (adpath.Contains("OU=Admin Identities"))
                {
                    link = getPersonLink(domain, name);
                    type = "Admin identity";
                }
                else if (adpath.Contains("OU=Admin"))
                {
                    type = "Admin (Server?)";
                }
                else if (adpath.Contains("OU=Server"))
                {
                    type = "Server";
                }
                else if (adpath.Contains("OU=Microsoft Exchange Security Groups"))
                {
                    link = getGroupLink(adpath, name);
                    type = "Microsoft Exchange Security Groups";
                }
                else
                {
                    type = "Special, find it in adpath";
                    Console.WriteLine();
                }

                if (accessName == null) //Is not fileshare
                {
                    groupTable.Rows.Add(new string[] { domain, link });
                }
                else
                {
                    groupTable.Rows.Add(new string[] { link, type, accessName });
                }
            }
        }

        // TODO: do not write HTML in backend
        private static string getGroupLink(string adpath, string name)
        {
            return string.Format("<a href=\"/Group?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + adpath), name);
        }

        private static string getPersonLink(string domain, string name)
        {
            return string.Format("<a href=\"/User?username={0}%40{1}.aau.dk\">{0}</a><br/>", name, domain);
        }

        private List<string> SortGroupMembers(List<string> groupMembers)
        {
            bool StartsWith(string[] prefix, string value) => prefix.Any(value.StartsWith);
            string[] prefixMBX_ACL = { "CN=MBX_", "CN=ACL_" };
            bool startsWithMBXorACL(string value) => StartsWith(prefixMBX_ACL, value);

            //Sort MBX and ACL Last
            groupMembers.Sort((a, b) =>
            {
                if (startsWithMBXorACL(a) && startsWithMBXorACL(b))
                {
                    return a.CompareTo(b);
                }
                else if (startsWithMBXorACL(a))
                {
                    return 1;
                }
                else if (startsWithMBXorACL(b))
                {
                    return -1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            });

            return groupMembers;
        }
    }
}
