using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class Authentication
    {
        public static bool IsPlatfromAD(string user)
        {
            if (user != null)
            {
                UserModel userModel = new UserModel(user, false);
                var temp = userModel.ADcache.getGroups("memberOf");

                if (temp.Any(x => x.Contains("CN=platform")))
                {
                    return true;
                }
            }

            return false;
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

            return false;
        }
    }
}
