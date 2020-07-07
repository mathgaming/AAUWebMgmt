using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;

namespace ITSWebMgmt.Controllers
{
    public class GroupController : WebMgmtController
    {
        public IActionResult Index(string grouppath, bool forceviewgroup = false)
        {
            GroupModel = new GroupModel(grouppath);
            new Logger(_context).Log(LogEntryType.GroupLookup, HttpContext.User.Identity.Name, grouppath, true);

            if (forceviewgroup == false && IsFileShare(GroupModel.DistinguishedName))
            {
                InitFileshareTables();
                
                GroupModel.IsFileShare = true;

                return View("FileShare", GroupModel);
            }
            return View(GroupModel);
        }

        public GroupModel GroupModel;

        public GroupController(LogEntryContext context) : base(context) {}

        public bool IsGroup()
        {
            ///XXX we expect a group check its a group
            return GroupModel.ADCache.DE.SchemaEntry.Name.Equals("group", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsFileShare(string value)
        {
            var split = value.Split(',');
            var oupath = split.Where(s => s.StartsWith("OU=")).ToArray();
            int count = oupath.Count();

            return ((count == 3 && oupath[count - 1].Equals("OU=Groups") && oupath[count - 2].Equals("OU=Resource Access")));
        }

        public void InitFileshareTables()
        {
            bool first = true;
            //TODO Things to show in basic info: Type fileshare/department and Domain plan/its/adm

            List<string> accessNames = new List<string> { "Full", "Modify", "Read", "Edit", "Contribute" };
            foreach (string accessName in accessNames)
            {
                string temp = Regex.Replace(GroupModel.ADPath, @"_[a-zA-Z]*,OU", $"_{accessName},OU");
                ADCache groupCache = null;
                try
                {
                    groupCache = new GroupADCache(temp);
                }
                catch (Exception)
                {
                }

                if (groupCache != null)
                {
                    var member = new PartialGroupModel(groupCache, "member", "Fileshares", accessName);
                    var memberOf = new PartialGroupModel(groupCache, "memberOf", "Fileshares", accessName);

                    if (first)
                    {
                        GroupModel.GroupTable = member.GroupTable;
                        GroupModel.GroupAllTable = member.GroupAllTable;
                        GroupModel.GroupOfTable = memberOf.GroupTable;
                        GroupModel.GroupOfAllTable = memberOf.GroupAllTable;
                        first = false;
                    }
                    else
                    {
                        GroupModel.GroupTable.Rows.AddRange(member.GroupTable.Rows);
                        GroupModel.GroupAllTable.Rows.AddRange(member.GroupAllTable.Rows);
                        GroupModel.GroupOfTable.Rows.AddRange(memberOf.GroupTable.Rows);
                        GroupModel.GroupOfAllTable.Rows.AddRange(memberOf.GroupAllTable.Rows);
                        GroupModel.GroupTable.LinkRows.AddRange(member.GroupTable.LinkRows);
                        GroupModel.GroupAllTable.LinkRows.AddRange(member.GroupAllTable.LinkRows);
                        GroupModel.GroupOfTable.LinkRows.AddRange(memberOf.GroupTable.LinkRows);
                        GroupModel.GroupOfAllTable.LinkRows.AddRange(memberOf.GroupAllTable.LinkRows);
                    }
                }
            }
        }
    }
}