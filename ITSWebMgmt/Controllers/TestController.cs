using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public class TestController : WebMgmtController
    {
        public TestController(LogEntryContext context) : base(context) { }

        public void Index()
        {
            //GroupModel m = new GroupModel("LDAP://CN=APP_LIC_O365-A5-For-Faculty,OU=Application Access,OU=Groups,DC=srv,DC=aau,DC=dk");

            string[] lines = System.IO.File.ReadAllLines(@"C:\webmgmtlog\mac.csv");
            var headings = lines[0].Split(';').ToList();

            int e1 = headings.FindIndex(x => x == "Email Address");
            int e2 = headings.FindIndex(x => x == "AAU-1x Username");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\webmgmtlog\mac-office.csv"))
            {
                System.IO.StreamWriter falsefile = new System.IO.StreamWriter(@"C:\webmgmtlog\mac-office-false.csv");
                file.WriteLine(lines[0]+";Have Office");

                foreach (string line in lines.Skip(1))
                {
                    string haveOffice = "False";

                    var parts = line.Split(';');
                    var e = parts[e1];
                    if (e == "")
                    {
                        e = parts[e2];
                    }
                    if (e == "")
                    {
                        haveOffice = "user not found";
                    }
                    else
                    {
                        UserModel model = new UserModel(e);
                        if (model.UserFound)
                        {
                            var groups = model.ADcache.getGroupsTransitive("memberOf");
                            haveOffice = groups.Any(x => x.Contains("APP_LIC_O365-A5-For-Faculty")).ToString();
                        }
                        else
                        {
                            haveOffice = "user not found";
                        }

                        if (haveOffice == "False")
                        {
                            falsefile.WriteLine(model.UserPrincipalName);
                        }
                    }

                    file.WriteLine(line + ";" + haveOffice);
                }
                falsefile.Close();
            }

            //throw new NotImplementedException();
        }
    }
}
