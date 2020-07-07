using System.Threading;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class ComputerListController : WebMgmtController
    {
        public ComputerListController(LogEntryContext context) : base(context) { }

        public IActionResult Index()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            return View();
        }

        public ActionResult GenerateList([FromBody]string email)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            ComputerListModel.AddEmail(email);

            if (!ComputerListModel.Running)
            {
                Thread thread = new Thread(new ThreadStart(GenerateListInBackground));
                thread.Start();
            }

            return Success($"An email will be sent to {email} when it is done");
        }

        private void GenerateListInBackground()
        {
            new ComputerListModel();
        }
    }
}
