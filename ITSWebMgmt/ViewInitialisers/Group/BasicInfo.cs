using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.ViewInitialisers.Group
{
    public class BasicInfo
    {
        public static GroupModel Init(GroupModel model)
        {
            var sb = new StringBuilder();

            var dom = model.Path.Split(',').Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");
            model.Domain = dom;

            string managedByString = "none";
            if (model.ADManagedBy != "")
            {
                var manager = model.ADManagedBy;

                var ldapSplit = manager.Split(',');
                var name = ldapSplit[0].Replace("CN=", "");
                var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                managedByString = string.Format("<a href=\"/Home/Redirector?adpath={0}\">{1}</a>", HttpUtility.HtmlEncode("LDAP://" + manager), domain + "\\" + name);
            }
            model.ManagedBy = managedByString;

            //IsDistributionGroup?
            //ManamgedBy

            var isDistgrp = false;
            string groupType = ""; //domain Local, Global, Universal

            var gt = model.GroupType;
            switch (gt)
            {
                case "2":
                    isDistgrp = true;
                    groupType = "Global";
                    break;
                case "4":
                    groupType = "Domain local";
                    isDistgrp = true;
                    break;
                case "8":
                    groupType = "Universal";
                    isDistgrp = true;
                    break;
                case "-2147483646":
                    groupType = "Global";
                    break;
                case "-2147483644":
                    groupType = "Domain local";
                    break;
                case "-2147483640":
                    groupType = "Universal";
                    break;
            }

            model.SecurityGroup = (!isDistgrp).ToString();
            model.GroupScope = groupType;

            //TODO: IsRequrceGroup (is exchange, fileshare or other resource type group?)

            return model;
        }
    }
}
