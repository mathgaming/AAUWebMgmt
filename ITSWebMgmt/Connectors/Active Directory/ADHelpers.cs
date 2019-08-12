using ITSWebMgmt.Helpers;
using System;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Connectors.Active_Directory
{
    public class ADHelpers
    {
        public static void AddADUserToGroup(string userADpath, string groupADPath)
        {
            DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry("LDAP://" + groupADPath);
            dirEntry.Properties["member"].Add(userADpath);
            dirEntry.CommitChanges();
            dirEntry.Close();
        }

        public static void RemoteADUserFromGroup(string userADpath, string groupADPath)
        {
            DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry("LDAP://" + groupADPath);
            dirEntry.Properties["member"].Remove(userADpath);
            dirEntry.CommitChanges();
            dirEntry.Close();
        }

        public static string DistinguishedNameToUPN(string dn)
        {
            //format CN=kyrke,OU=test,OU=Staff,OU=People,DC=its,DC=aau,DC=dk 
            //to kyrke@its.aau.dk

            string[] dnSplit = dn.Split(',');
            string cn = dnSplit[0].ToLower().Replace("cn=", "");
            string domain = String.Join(".", dnSplit.Where(x => x.ToLower().StartsWith("dc=")).Select(x => x.ToLower().Replace("dc=", "")));

            return $"{cn}@{domain}";

        }

    }
}
