using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public class Exchange
    {
        public static PartialGroupModel Init(PartialGroupModel Model)
        {
            string transitiv = "";

            var members = Model.getGroupsTransitive(Model.AttributeName);
            if (members.Count == 0)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
                members = Model.getGroups(Model.AttributeName);
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Select Exchange groups and convert to list of ExchangeMailboxGroup
            var exchangeMailboxGroupList = members.Where<string>(group => (group.StartsWith("CN=MBX_"))).Select(x => new ExchangeMailboxGroup(x));

            foreach (ExchangeMailboxGroup e in exchangeMailboxGroupList)
            {
                var type = e.Type;
                var domain = e.Domain;
                var nameFormated = string.Format("<a href=\"/Group?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + e.RawValue), e.Name);
                var access = e.Access;
                helper.AddRow(new string[] { type, domain, nameFormated, access });
            }

            Model.Data = transitiv + helper.GetTable();

            return Model;
        }
    }
}
