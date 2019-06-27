using ITSWebMgmt.Caches;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Models
{
    public class GroupModel : WebMgmtModel<GroupADcache>
    {
        public string Description { get => ADcache.getProperty("description"); }
        public string Info { get => ADcache.getProperty("info"); }
        public string Name { get => ADcache.getProperty("name"); }
        public string ADManagedBy { get => ADcache.getProperty("managedBy"); }
        public string GroupType { get => ADcache.getProperty("groupType").ToString(); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName").ToString(); }
        public string Title;
        public string Domain;
        public string ManagedBy;
        public string SecurityGroup;
        public string GroupScope;
        public string GroupSegment;
        public string GroupsAllSegment;
        public string GroupOfSegment;
        public string GroupsOfAllSegment;
        public string Raw;
        public bool IsFileShare = false;

        public GroupModel()
        {
        }
    }
}