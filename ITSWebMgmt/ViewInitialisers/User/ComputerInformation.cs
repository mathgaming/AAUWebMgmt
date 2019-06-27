using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class ComputerInformation
    {
        public static UserModel Init(UserModel model)
        {
            try
            {
                string upn = model.UserPrincipalName;
                string[] upnsplit = upn.Split('@');
                string domain = upnsplit[1].Split('.')[0];

                string userName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

                var helper = new HTMLTableHelper(new string[] { "Computername", "AAU Fjernsupport" });

                foreach (ManagementObject o in model.getUserMachineRelationshipFromUserName(userName))
                {
                    var machinename = o.Properties["ResourceName"].Value.ToString();
                    var name = "<a href=\"/Computer?computername=" + machinename + "\">" + machinename + "</a><br />";
                    var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + machinename + "\">Start</a>";
                    helper.AddRow(new string[] { name, fjernsupport });
                }
                model.ComputerInformation = "<h4>Links til computerinfo kan være til maskiner i et forkert domæne, da info omkring computer domæne ikke er tilgængelig i denne søgning</h4>" + helper.GetTable();
            }
            catch (System.UnauthorizedAccessException e)
            {
                model.ComputerInformation = "Service user does not have SCCM access.";
            }

            return model;
        }
    }
}
