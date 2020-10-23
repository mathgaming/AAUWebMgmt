using ITSWebMgmt.Connectors;
using System.DirectoryServices;

namespace ITSWebMgmt.Helpers
{
    public class ADConnector
    {
        public static DirectoryEntry CreateNewDirectoryEntry(string ADPath)
        {
            Secret secret = new PasswordManagerConnector().GetSecret("ad");
            return new DirectoryEntry(ADPath, secret.UserName, secret.Password);
        }
    }
}
