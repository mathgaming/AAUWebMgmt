using ITSWebMgmt.Helpers;
using System;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Connectors.Active_Directory
{
    public class ADHelpers
    {
        public static void AddMemberToGroup(string userADpath, string groupADPath)
        {
            try
            {
                DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry(groupADPath);
                dirEntry.Properties["member"].Add(userADpath);
                dirEntry.CommitChanges();
                dirEntry.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public static void RemoteMemberFromGroup(string userADpath, string groupADPath)
        {
            DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry(groupADPath);
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
            string domain = string.Join(".", dnSplit.Where(x => x.ToLower().StartsWith("dc=")).Select(x => x.ToLower().Replace("dc=", "")));

            return $"{cn}@{domain}";

        }

    }
}
