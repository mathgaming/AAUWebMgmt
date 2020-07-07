using System;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Helpers
{
    public class ADHelper
    {
        public static bool DisableUser(string ADPath)
        {
            return SetUserDisableState(ADPath, false);
        }

        public static bool EnableUser(string ADPath)
        {
            return SetUserDisableState(ADPath, true);
        }

        private static bool SetUserDisableState(string ADPath, bool enable)
        {
            try
            {
                DirectoryEntry user = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);
                int val = (int)user.Properties["userAccountControl"].Value;
                if (enable)
                {
                    user.Properties["userAccountControl"].Value = val & ~0x2;
                }
                else
                {
                    user.Properties["userAccountControl"].Value = val | 0x2;
                }

                user.CommitChanges();
                user.Close();

                return true;
            }
            catch (DirectoryServicesCOMException)
            {
                return false;
            }
        }

        public static void AddMemberToGroup(string userADPath, string groupADPath)
        {
            try
            {
                DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry(groupADPath);
                dirEntry.Properties["member"].Add(userADPath);
                dirEntry.CommitChanges();
                dirEntry.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public static void RemoteMemberFromGroup(string userADPath, string groupADPath)
        {
            DirectoryEntry dirEntry = DirectoryEntryCreator.CreateNewDirectoryEntry(groupADPath);
            dirEntry.Properties["member"].Remove(userADPath);
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

        public static string ComputerNameFromADPath(string ADPath) => ADPath.Split(',')[0].ToLower().Replace("cn=", "");

        private static string GetADPathFromUsername(string username)
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

            var ADPath = GlobalSearch(username);
            if (ADPath == null)
            {
                //Show user not found
                return null;
            }
            else
            {
                //We got ADPath lets build the GUI
                return ADPath;
            }
        }

        public static string GetADPath(string username)
        {
            username = NormalizeUsername(username);

            if (username.Length == 4 && int.TryParse(username, out _))
            {
                return DoPhoneSearch(username);
            }
            else
            {
                return GetADPathFromUsername(username);
            }
        }

        public static string NormalizeUsername(string username)
        {
            if (username.Contains('(') && username.Contains(')'))
            {
                username = username.Split('(', ')')[1];
            }

            return username;
        }

        private static string GlobalSearch(string seachString)
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
                string ADPath = r.Properties["ADsPath"][0].ToString();
                return ADPath.Replace("aau.dk/", "").Replace("GC:", "LDAP:");
            }
            else
            {
                return null;
            }
        }

        //Searhces on a phone numer (internal or external), and returns a upn (later ADsPath) to a use or null if not found
        private static string DoPhoneSearch(string numberIn)
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

        public static string ConvertToStringWithCorrectFormatIfDate(dynamic v)
        {
            if (v.GetType().Equals(typeof(DateTime)))
            {
                return DateTimeConverter.Convert((DateTime)v);
            }
            else if (v.GetType().ToString() == "System.__ComObject")
            {
                try
                {
                    int test = (int)v.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, v, null);
                    return DateTimeConverter.Convert(v);
                }
                catch (Exception) { }
            }
            return v.ToString();
        }
    }
}
