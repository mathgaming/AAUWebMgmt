using ITSWebMgmt.Controllers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Caches;
using System.Management;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ITSWebMgmt.Helpers
{
    public class OneDriveHelper
    {
        const string COMPUTER_USES_1DRIVE_FLAG = "GPO_Computer_UseOnedriveStorage";
        const string USER_USES_1DRIVE_FLAG = "GPO_User_DenyFolderRedirection";
        public static string doesUserUseOneDrive(UserModel user) {
            bool userOnedrive = doesUserHaveDeniedFolderRedirect(user);
            bool computerOnedreive = doesOneComputerUseOneDrive(user);

            if (userOnedrive)
            {
                if (computerOnedreive)
                {
                    return "True";
                }
                else
                {
                    return "True (computer does not)";
                }
            }

            return "False";
        }
        private static bool doesOneComputerUseOneDrive(UserModel user)
        {
            string upn = user.UserPrincipalName;
            string[] upnsplit = upn.Split('@');
            string domain = upnsplit[1].Split('.')[0];

            string userName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

            ManagementObjectCollection connectedComputers = user.getUserMachineRelationshipFromUserName(userName);
            foreach (ManagementObject comp in connectedComputers)
            {
                if (computerUsesOneDrive(comp))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool doesUserHaveDeniedFolderRedirect(UserModel user)
        {
            List<string> memberGroups = user.ADcache.getGroupsTransitive("memberOf");
            return memberGroups.Exists(x => x.Contains(USER_USES_1DRIVE_FLAG));
        }

        public static bool computerUsesOneDrive(ManagementObject comp)
        {
            string computerName = comp.Properties["ResourceName"].Value.ToString();
            ADcache cache = new ComputerADcache(computerName);
            return ComputerUsesOneDrive(cache);
        }

        public static bool ComputerUsesOneDrive(ADcache cache)
        {
            return cache.getGroupsTransitive("memberOf").Exists(x => x.Contains(COMPUTER_USES_1DRIVE_FLAG));
        }
    }
}
