using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public static class JamfCSVToMailList
    {
        public static string Convert(string csv)
        {
            List<string> lines = csv.Split('\n').ToList();
            List<string> headers = lines[0].Split(',').ToList();

            int emailIndex = headers.IndexOf("Email Address");
            int secundaryEmailIndex = headers.IndexOf("AAU-1x Username");
            int aauNumberIndex = headers.IndexOf("Asset Tag");
            StringBuilder sb = new StringBuilder();

            sb.Append("Email;AAUNumber;Navn\n");

            foreach (var line in lines.Skip(1))
            {
                List<string> columbs = line.Split(',').ToList();
                string email = columbs[emailIndex];
                if (email != "")
                {
                    email = columbs[secundaryEmailIndex];
                }
                string name = new UserModel(email, false).DisplayName;
                sb.Append($"{email};{columbs[aauNumberIndex]};{name}\n");
            }

            return sb.ToString();
        }
    }
}
