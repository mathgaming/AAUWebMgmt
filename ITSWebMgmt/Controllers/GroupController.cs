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
        //https://localhost:44322/group/index?grouppath=LDAP:%2f%2fCN%3dcm12_config_AAU10%2cOU%3dConfigMgr%2cOU%3dGroups%2cDC%3dsrv%2cDC%3daau%2cDC%3ddk
        public IActionResult Index(string grouppath, bool forceviewgroup = false)
        {
            GroupModel = new GroupModel(grouppath);
            new Logger(_context).Log(LogEntryType.GroupLookup, HttpContext.User.Identity.Name, grouppath, true);

            if (forceviewgroup == false && isFileShare(GroupModel.DistinguishedName))
            {
                InitFileshareTables();
                
                GroupModel.IsFileShare = true;

                return View("FileShare", GroupModel);
            }
            return View(GroupModel);
        }

        public GroupModel GroupModel;

        public GroupController(LogEntryContext context) : base(context) {}

        public bool isGroup()
        {
            ///XXX we expect a group check its a group
            return GroupModel.ADcache.DE.SchemaEntry.Name.Equals("group", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool isFileShare(string value)
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
                string temp = Regex.Replace(GroupModel.adpath, @"_[a-zA-Z]*,OU", $"_{accessName},OU");
                ADcache groupCache = null;
                try
                {
                    groupCache = new GroupADcache(temp);
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
                    }
                }
            }
        }
    }
}