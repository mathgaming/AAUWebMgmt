using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ITSWebMgmt.Caches
{
    public class ComputerADCache : ADCache
    {
        public string ComputerName;
        public bool ComputerFound = false;
        public string Domain;
        public object AdmPwdExpirationTime;
        public ComputerADCache(string computerName) : base()
        {
            ComputerName = computerName;

            DE = DirectoryEntryCreator.CreateNewDirectoryEntry("LDAP://" + getDomain());
            var search = new DirectorySearcher(DE);
            bool failed = false;

            try
            {
                search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
                result = search.FindOne();
            }
            catch (Exception)
            {
                failed = true;
            }

            if (failed || result == null)
            { //Computer not found
                return;
            }

            ComputerFound = true;

            ADPath = result.Properties["ADsPath"][0].ToString();
            DE = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);

            var PropertyNames = new List<string> { "memberOf", "cn", "ms-Mcs-AdmPwdExpirationTime", "managedBy", "whenCreated", "operatingSystem" };

            search = new DirectorySearcher(DE);
            foreach (string p in PropertyNames)
            {
                search.PropertiesToLoad.Add(p);
            }
            search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);
            result = search.FindOne();

            List<Property> properties = new List<Property>
            {
                new Property("distinguishedName", typeof(string)),
                new Property("managedBy", typeof(string)),
                new Property("cn", typeof(string)),
                new Property("memberOf", typeof(string)),
                new Property("ms-Mcs-AdmPwdExpirationTime", typeof(object)), //System.__ComObject
                new Property("whenCreated", typeof(object)), //System.__ComObject
                new Property("operatingSystem", typeof(string))
            };

            SaveCache(properties, null);
        }

        public void DeleteComputer()
        {
            DirectoryEntry computerToDel = result.GetDirectoryEntry();
            computerToDel.DeleteTree();
            computerToDel.CommitChanges();
        }

        private string getDomain()
        {
            var tmpName = ComputerName;

            if (tmpName.Contains("\\"))
            {
                var tmp = tmpName.Split('\\');
                ComputerName = tmp[1];

                if (!tmp[0].Equals("aau", StringComparison.CurrentCultureIgnoreCase))
                {
                    Domain = tmp[0] + ".aau.dk";
                }
                else
                {
                    Domain = "aau.dk";
                }
            }

            if (Domain == null)
            {
                var de = DirectoryEntryCreator.CreateNewDirectoryEntry("GC://aau.dk");
                string filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);

                var search = new DirectorySearcher(de);
                search.Filter = filter;
                search.PropertiesToLoad.Add("distinguishedName");

                var r = search.FindOne();

                if (r == null)
                { //Computer not found

                    return null;
                }

                var distinguishedName = r.Properties["distinguishedName"][0].ToString();
                var split = distinguishedName.Split(',');

                var len = split.GetLength(0);
                Domain = (split[len - 3] + "." + split[len - 2] + "." + split[len - 1]).Replace("DC=", "");
            }

            return Domain;
        }
    }
}