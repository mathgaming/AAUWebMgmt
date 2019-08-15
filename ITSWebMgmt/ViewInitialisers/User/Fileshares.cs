﻿using ITSWebMgmt.Controllers;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class Fileshares
    {
        public static PartialGroupModel Init(PartialGroupModel model)
        {
            string transitiv = "";
            var members = model.getGroupsTransitive(model.AttributeName);

            if (members.Count == 0)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
                members = model.getGroups(model.AttributeName);
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Filter fileshare groups and convert to Fileshare Objects
            var fileshareList = members.Where((string value) =>
            {
                return GroupController.isFileShare(value);
            }).Select(x => new FileshareModel(x));

            foreach (FileshareModel f in fileshareList)
            {
                var nameWithLink = string.Format("<a href=\"/Group?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + f.Fileshareraw), f.Name);
                helper.AddRow(new string[] { f.Type, f.Domain, nameWithLink, f.Access });
            }

            model.Data = transitiv + helper.GetTable();

            return model;
        }
    }
}