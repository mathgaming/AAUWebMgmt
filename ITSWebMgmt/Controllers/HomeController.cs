using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Models;

namespace ITSWebMgmt.Controllers
{
    public class HomeController : WebMgmtController
    {
        public HomeController(LogEntryContext context) : base(context) { }

        public IActionResult Index()
        {
            return View(new HomeModel(_context, HttpContext.User.Identity.Name));
        }

        [Route("/Home/Search")]
        public void Search(string searchstring)
        {
            if (searchstring != null)
            {
                searchstring = searchstring.Replace("\t","");
                if (ADHelper.GetADPath(searchstring) != null)
                {
                    Response.Redirect("/User?username=" + searchstring);
                }
                else
                {
                    Response.Redirect("/Computer?computername=" + searchstring);
                }
            }
            else
            {
                Response.Redirect("/");
            }
        }

        [HttpPost]
        public void GiveFeedback()
        {
            Response.Redirect("/CreateWorkItem?isfeedback=true");
        }

        public IActionResult ChangeLog()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public void StopMaintenance()
        {
            if (Authentication.IsPlatform(HttpContext.User.Identity.Name))
            {
                MaintenanceHelper.IsDownForMaintenance = false;
                Response.Redirect("/");
            }
        }

        [HttpPost]
        public IActionResult StartMaintenance([FromBody] string message)
        {
            if (Authentication.IsPlatform(HttpContext.User.Identity.Name))
            {
                MaintenanceHelper.IsDownForMaintenance = true;
                MaintenanceHelper.Message = message;
                return Success("WebMgmt is now down for maintenance");
            }

            return Error("You do no have access to do this");
        }

        public void Redirector(string ADPath)
        {
            if (ADPath != null)
            {
                if (!ADPath.StartsWith("LDAP://"))
                {
                    ADPath = "LDAP://" + ADPath;
                }
                DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);

                var type = de.SchemaEntry.Name;

                if (type.Equals("user"))
                {
                    string param = "?" + "username=" + de.Properties["userPrincipalName"].Value.ToString();
                    Response.Redirect("/User" + param);
                }
                else if (type.Equals("computer"))
                {
                    var ldapSplit = ADPath.Replace("LDAP://", "").Split(',');
                    var name = ldapSplit[0].Replace("CN=", "");
                    var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                    string param = "?" + "computername=" + domain + "\\" + name;
                    Response.Redirect("/Computer" + param);
                }
                else if (type.Equals("group"))
                {
                    string param = "?" + "grouppath=" + ADPath;
                    Response.Redirect("/Group" + param);
                }
            }
        }
    }
}
