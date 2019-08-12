using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class DirectoryEntryCreator
    {
        private static readonly string username = Startup.Configuration["cred:ad:username"];
        private static readonly string password = Startup.Configuration["cred:ad:password"];
        public static DirectoryEntry CreateNewDirectoryEntry(string adpath)
        {
            return new DirectoryEntry(adpath, username, password);
        }
    }
}
