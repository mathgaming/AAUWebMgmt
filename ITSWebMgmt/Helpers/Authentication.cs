using ITSWebMgmt.Models;
using System.Collections.Generic;
using System.Linq;

namespace ITSWebMgmt.Helpers
{
    public class Authentication
    {
        public static bool IsPlatfromAD(string user)
        {
            if (user != null)
            {
                UserModel userModel = new UserModel(user, false);
                var temp = userModel.ADCache.GetGroups("memberOf");

                if (temp.Any(x => x.Contains("CN=platform")))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPlatform(string user)
        {
            return !IsNotPlatform(user);
        }

        //Hardcoded, but fast
        public static bool IsNotPlatform(string user)
        {
            if (user != null)
            {
                List<string> members = new List<string>() { "abn", "jaa", "lbrack17", "lel", "mhsv16", "mrm", "torknu" };
                user = user.Substring(user.IndexOf(@"\") + 1);

                if (members.Contains(user))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
