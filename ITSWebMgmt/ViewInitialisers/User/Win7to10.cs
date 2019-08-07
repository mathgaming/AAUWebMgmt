
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class Win7to10
    {
        public static UserModel Init(UserModel model)
        {
            bool haveWindows7 = false;
            var helper = new HTMLTableHelper(new string[] { "Computername", "Windows 7 to 10 upgrade" });

            foreach (ComputerModel m in model.getManagedComputers())
            {
                m.setConfig();
                string upgradeButton = "";
                if (m.ConfigPC.Equals("AAU7 PC") || m.ConfigPC.Equals("Administrativ7 PC"))
                {
                    var name = "<a href=\"/Computer?computername=" + m.ComputerName + "\">" + m.ComputerName + "</a><br/>";
                    upgradeButton = "<input type=\"button\" value=\"Create Win7 to 10 SR\" onclick=\"submitform('" + m.ComputerName + "');\"/>";
                    helper.AddRow(new string[] { name, upgradeButton });
                    haveWindows7 = true;
                }
            }

            if (haveWindows7)
            {
                var scsm = new SCSMConnector();
                _ = scsm.getUUID(model.UserPrincipalName, model.DisplayName).Result;
                model.SCSMUserID = scsm.userID;

                model.Windows7to10 = helper.GetTable();
            }
            else
            {
                model.Windows7to10 = "User do not have any Windows 7 PCs";
            }

            return model;
        }
    }
}
