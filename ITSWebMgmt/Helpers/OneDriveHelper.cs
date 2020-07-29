using ITSWebMgmt.Models;
using ITSWebMgmt.Caches;
using System.Management;
using System.Collections.Generic;

namespace ITSWebMgmt.Helpers
{
    public class OneDriveHelper
    {
        const string COMPUTER_USES_1DRIVE_FLAG = "GPO_Computer_UseOnedriveStorage";
        const string USER_USES_1DRIVE_FLAG = "GPO_User_DenyFolderRedirection";
        public static string DoesUserUseOneDrive(UserModel user) {
            bool userOnedrive = DoesUserHaveDeniedFolderRedirect(user);
            bool computerOnedreive = DoesOneComputerUseOneDrive(user);

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
        private static bool DoesOneComputerUseOneDrive(UserModel user)
        {
            string upn = user.UserPrincipalName;
            if (upn != "")
            {
                string[] upnsplit = upn.Split('@');
                string domain = upnsplit[1].Split('.')[0];

                string userName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

                ManagementObjectCollection connectedComputers = user.GetUserMachineRelationshipFromUserName(userName);
                foreach (ManagementObject comp in connectedComputers)
                {
                    if (ComputerUsesOneDrive(comp))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        public static bool DoesUserHaveDeniedFolderRedirect(UserModel user)
        {
            List<string> memberGroups = user.ADCache.GetGroupsTransitive("memberOf");
            return memberGroups.Exists(x => x.Contains(USER_USES_1DRIVE_FLAG));
        }

        public static bool ComputerUsesOneDrive(ManagementObject comp)
        {
            string computerName = comp.Properties["ResourceName"].Value.ToString();
            ADCache cache = new ComputerADCache(computerName);
            if (cache.Path == "LDAP://") //AD-OESSTEST can not be found in AD
            {
                return false;
            }
            return ComputerUsesOneDrive(cache);
        }

        public static bool ComputerUsesOneDrive(ADCache cache)
        {
            return cache.GetGroupsTransitive("memberOf").Exists(x => x.Contains(COMPUTER_USES_1DRIVE_FLAG));
        }
    }
}
