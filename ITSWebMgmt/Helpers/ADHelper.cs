using ITSWebMgmt.Helpers;
using System;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Helpers
{
    public class ADHelper
    {
        public static bool DisableUser(string adpath)
        {
            return setUserDisableState(adpath, false);
        }

        public static bool EnableUser(string adpath)
        {
            return setUserDisableState(adpath, true);
        }

        private static bool setUserDisableState(string adpath, bool enable)
        {
            try
            {
                DirectoryEntry user = DirectoryEntryCreator.CreateNewDirectoryEntry(adpath);
                int val = (int)user.Properties["userAccountControl"].Value;
                if (enable)
                {
                    user.Properties["userAccountControl"].Value = val & ~0x2;
                }
                else
                {
                    user.Properties["userAccountControl"].Value = val | 0x2;
                }
                
                //ADS_UF_ACCOUNTDISABLE;

                user.CommitChanges();
                user.Close();

                return true;
            }
            catch (DirectoryServicesCOMException)
            {
                return false;
            }
        }

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

        private static string getADPathFromUsername(string username)
        {
            if (username.Contains("\\"))
            {
                //Form is domain\useranme
                var tmp = username.Split('\\');
                if (!tmp[0].Equals("AAU", StringComparison.CurrentCultureIgnoreCase))
                {
                    username = string.Format("{0}@{1}.aau.dk", tmp[1], tmp[0]);
                }
                else //IS AAU domain 
                {
                    username = string.Format("{0}@{1}.dk", tmp[1], tmp[0]);
                }
            }

            var adpath = globalSearch(username);
            if (adpath == null)
            {
                //Show user not found
                return null;
            }
            else
            {
                //We got ADPATH lets build the GUI
                return adpath;
            }
        }

        public static string GetADPath(string username)
        {
            int val;
            if (username.Length == 4 && int.TryParse(username, out val))
            {
                return doPhoneSearch(username);
            }
            else
            {
                return getADPathFromUsername(username);
            }
        }

        private static string globalSearch(string seachString)
        {
            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry("GC://aau.dk");
            string filter;
            if (seachString.Length == 6)
            {
                filter = string.Format("(|(proxyaddresses=SMTP:{0})(aauAAUID={0}))", seachString);
            }
            else
            {
                filter = string.Format("(|(proxyaddresses=SMTP:{0})(userPrincipalName={0}))", seachString);
            }

            DirectorySearcher search = new DirectorySearcher(de, filter);
            SearchResult r = search.FindOne();

            if (r != null)
            {
                //return r.Properties["userPrincipalName"][0].ToString(); //XXX handle if result is 0 (null exception)
                string adpath = r.Properties["ADsPath"][0].ToString();
                return adpath.Replace("aau.dk/", "").Replace("GC:", "LDAP:");
            }
            else
            {
                return null;
            }
        }

        //Searhces on a phone numer (internal or external), and returns a upn (later ADsPath) to a use or null if not found
        private static string doPhoneSearch(string numberIn)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string number = numberIn;
            //If number is a shot internal number, expand it :)
            if (number.Length == 4)
            {
                // format is 3452
                number = string.Format("+459940{0}", number);

            }
            else if (number.Length == 6)
            {
                //format is +453452 
                number = string.Format("+459940{0}", number.Replace("+45", ""));

            }
            else if (number.Length == 8)
            {
                //format is 99403442
                number = string.Format("+45{0}", number);

            } // else format is ok

            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry("GC://aau.dk");
            //string filter = string.Format("(&(objectCategory=person)(telephoneNumber={0}))", number);
            string filter = string.Format("(&(objectCategory=person)(objectClass=user)(telephoneNumber={0}))", number);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            search.PropertiesToLoad.Add("userPrincipalName"); //Load something to speed up the object get?
            SearchResult r = search.FindOne();

            watch.Stop();
            System.Diagnostics.Debug.WriteLine("phonesearch took: " + watch.ElapsedMilliseconds);

            if (r != null)
            {
                //return r.Properties["ADsPath"][0].ToString(); //XXX handle if result is 0 (null exception)
                return r.Properties["ADsPath"][0].ToString().Replace("GC:", "LDAP:").Replace("aau.dk/", "");
            }
            else
            {
                return null;
            }
        }
    }
}
