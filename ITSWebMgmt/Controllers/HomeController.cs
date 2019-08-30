using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using System.DirectoryServices;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mail;
using System.Threading;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Connectors;

namespace ITSWebMgmt.Controllers
{
    public class HomeController : WebMgmtController
    {
        private string Password = Startup.Configuration["Email-password"];

        public HomeController(LogEntryContext context) : base(context) { }

        public IActionResult Index()
        {
            return View(_context.KnownIssues.Where(x => x.Active));
        }

        public IActionResult ChangeLog()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var e = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var model = new ErrorViewModel();

            model.QueryString = HttpContext.Request.QueryString.Value;
            model.Path = e.Path;
            model.Error = e.Error;
            model.AffectedUser = HttpContext.User.Identity.Name;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                sendEmail(model);
            }, null);

            return View("Error", model);
        }

        private void sendEmail(ErrorViewModel model)
        {
            MailMessage mail = new MailMessage("mhsv16@its.aau.dk", "platform@its.aau.dk");
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Timeout = 10000;
            client.Credentials = new System.Net.NetworkCredential("mhsv16@its.aau.dk", Password);
            client.Host = "smtp.aau.dk";
            mail.Subject = "WebMgmt error";
            mail.Body = $"Person: {model.AffectedUser}\n" +
                        $"Error: {model.ErrorMessage}\n" +
                        $"Url: {model.Url}\n" +
                        $"Stacktrace:\n{model.Stacktrace}\n";
            client.Send(mail);
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
