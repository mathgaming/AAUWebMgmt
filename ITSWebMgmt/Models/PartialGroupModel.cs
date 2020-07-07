using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSWebMgmt.Models
{
    public class PartialGroupModel : WebMgmtModel<ADCache>
    {
        public List<string> GroupList { get; set; }
        public List<string> GroupAllList { get; set; }
        public TableModel GroupTable { get; set; }
        public TableModel GroupAllTable { get; set; }
        public TableModel FilteredTable { get; set; }
        public string AttributeName { get; set; }
        public string Title { get; set; }
        public string AccessName { get; set; }

        public List<string> GetGroups(string name) => ADCache.GetGroups(name);
        public List<string> GetGroupsTransitive(string name) => ADCache.GetGroupsTransitive(name);

        public PartialGroupModel(ADCache cache, string attributeName, string title = "Groups", string accessName = null)
        {
            ADCache = cache;
            AttributeName = attributeName;
            Title = title;
            AccessName = accessName;

            Init();
        }

        private void Init()
        {
            GroupList = SortGroupMembers(ADCache.GetGroups(AttributeName));
            GroupAllList = SortGroupMembers(ADCache.GetGroupsTransitive(AttributeName));

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
            if (accessName == null) //Is not fileshare
            {
                groupTable.LinkColumns = new int[] { 1 };
            }
            else
            {
                groupTable.LinkColumns = new int[] { 0 };
            }

            foreach (string ADPath in groups)
            {
                var split = ADPath.Split(',');
                var name = split[0].Replace("CN=", "");
                var domain = split.Where(s => s.StartsWith("DC=")).ToArray()[0].Replace("DC=", "");
                var link = "";
                var linkText = name;
                var type = "unknown";
                if (!ADPath.Contains("OU"))
                {
                    type = "unknown";
                }
                else if (ADPath.Contains("OU=Groups"))
                {
                    link = GetGroupLink(ADPath);
                    type = "Group";
                }
                else if (ADPath.Contains("OU=Admin Groups"))
                {
                    link = GetGroupLink(ADPath);
                    type = "Admin group";
                }
                else if (ADPath.Contains("OU=People"))
                {
                    link = GetPersonLink(domain, name);
                    type = "Person";
                }
                else if (ADPath.Contains("OU=Admin Identities"))
                {
                    link = GetPersonLink(domain, name);
                    type = "Admin identity";
                }
                else if (ADPath.Contains("OU=Admin"))
                {
                    type = "Admin (Server?)";
                }
                else if (ADPath.Contains("OU=Server"))
                {
                    type = "Server";
                }
                else if (ADPath.Contains("OU=Client"))
                {
                    link = GetComputerLink(ADPath);
                    type = "Client";
                }
                else if (ADPath.Contains("OU=Microsoft Exchange Security Groups"))
                {
                    link = GetGroupLink(ADPath);
                    type = "Microsoft Exchange Security Groups";
                }
                else
                {
                    type = "Special, find it in ADPath";
                }

                if (accessName == null) //Is not fileshare
                {
                    groupTable.Rows.Add(new string[] { domain, linkText });
                    groupTable.LinkRows.Add(new string[] { link });
                }
                else
                {
                    groupTable.Rows.Add(new string[] { linkText, type, accessName });
                    groupTable.LinkRows.Add(new string[] { link });
                }
            }
        }

        private static string GetComputerLink(string ADPath) => string.Format("/Computer?computername={0}", ADHelper.ComputerNameFromADPath(ADPath));

        private static string GetGroupLink(string ADPath) =>  string.Format("/Group?grouppath={0}", HttpUtility.UrlEncode("LDAP://" + ADPath));

        private static string GetPersonLink(string domain, string name) => string.Format("/User?username={0}%40{1}.aau.dk", name, domain);

        private List<string> SortGroupMembers(List<string> groupMembers)
        {
            static bool StartsWith(string[] prefix, string value) => prefix.Any(value.StartsWith);
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
