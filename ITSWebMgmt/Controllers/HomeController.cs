using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;
using System;

namespace ITSWebMgmt.Controllers
{
    public class HomeController : WebMgmtController
    {
        public HomeController(LogEntryContext context) : base(context) { }

        public IActionResult Index()
        {
            new FindComputersForAllStaff();
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

        public void Redirector(string adpath)
        {
            if (adpath != null)
            {
                if (!adpath.StartsWith("LDAP://"))
                {
                    adpath = "LDAP://" + adpath;
                }
                DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(adpath);

                var type = de.SchemaEntry.Name;

                if (type.Equals("user"))
                {
                    string param = "?" + "username=" + de.Properties["userPrincipalName"].Value.ToString();
                    Response.Redirect("/User" + param);
                }
                else if (type.Equals("computer"))
                {
                    var ldapSplit = adpath.Replace("LDAP://", "").Split(',');
                    var name = ldapSplit[0].Replace("CN=", "");
                    var domain = ldapSplit.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

                    string param = "?" + "computername=" + domain + "\\" + name;
                    Response.Redirect("/Computer" + param);
                }
                else if (type.Equals("group"))
                {
                    string param = "?" + "grouppath=" + adpath;
                    Response.Redirect("/Group" + param);
                }
            }
        }
    }
}
