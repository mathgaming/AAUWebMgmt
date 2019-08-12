using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public class LogController : WebMgmtController
    {
        public IActionResult Index()
        {
            SimpleModel model = new SimpleModel();
            model.Text = "You do not have access to the log";

            if (HttpContext.User.Identity.Name != null)
            {
                UserModel userModel = new UserModel(HttpContext.User.Identity.Name, false);                
                var temp = userModel.ADcache.getGroups("memberOf");

                if (temp.Any(x => x.Contains("CN=platform")))
                {
                    List<string> lines = new List<string>();
                    string line;
                    System.IO.StreamReader file = new System.IO.StreamReader(@"C:\webmgmtlog\logfile.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!line.Contains("(Hidden)"))
                        {
                            lines.Add(line);
                        }
                    }

                    file.Close();

                    lines.Reverse();

                    model.Text = string.Join("<br />", lines);
                }
            }

            return View(model);
        }
    }
}
