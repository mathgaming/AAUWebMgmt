using System.DirectoryServices;

namespace ITSWebMgmt.Helpers
{
    public class DirectoryEntryCreator
    {
        private static readonly string username = Startup.Configuration["cred:ad:username"];
        private static readonly string password = Startup.Configuration["cred:ad:password"];
        public static DirectoryEntry CreateNewDirectoryEntry(string ADPath)
        {
            return new DirectoryEntry(ADPath, username, password);
        }
    }
}
