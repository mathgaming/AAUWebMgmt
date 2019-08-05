using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class CreateWorkItemController : Controller
    {
        public IActionResult Index(string userDisplayName)
        {
            CreateWorkItemModel model = new CreateWorkItemModel();
            model.AffectedUser = userDisplayName;

            return View(model);
        }
        public IActionResult Win7Index (string userDisplayName, string computerName)
        {
            //Special case of returning predefined form for upgrading windows 7 pc's
            return createWindows7UpgradeForm(userDisplayName, computerName);
        }

        protected string createRedirectCode(string userID, string displayName, string title, string description, string submiturl)
        {
            string jsondata = "{\"Title\":\"" + title + "\",\"Description\":\"" + description + "\",\"RequestedWorkItem\":{\"BaseId\":\"" + userID + "\",\"DisplayName\":\"" + displayName + "\",}}";
            var sb = new StringBuilder();

            sb.Append("<html><head>");
            //sb.Append("<script src=\"https://cdnjs.cloudflare.com/ajax/libs/jquery/3.0.0-alpha1/jquery.min.js\" ></script>");
            sb.Append("</head>");
            sb.Append(@"<body onload='document.forms[""form""].submit()'>");

            sb.Append("<form name='form' target='_blank' method='post' action='" + submiturl + "'>");

            sb.Append("<input hidden='true' name='vm' value='" + jsondata + "'/>");
            sb.Append("<input type='submit' value='Recreate IR/SR (if someting whent wrong)' />");

            sb.Append(@"<br /><br /><a href=""#"" onclick=""window.history.go(-2); return false;""> Go back to UserInfo </a>");

            sb.Append("</form>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        protected ActionResult createForm(string url, CreateWorkItemModel model)
        {
            string descriptionConverted = model.Desription.Replace("\n", "\\n").Replace("\r", "\\r");
            return Content(createRedirectCode(HttpContext.User.Identity.Name, model.AffectedUser, model.Title, descriptionConverted, url), "text/html");
        }

        [HttpPost]
        protected ActionResult createWindows7UpgradeForm(string computerOwner, string affectedComputerName)
        {
            CreateWorkItemModel newUpgradeForm = new CreateWorkItemModel();
            newUpgradeForm.AffectedUser = computerOwner;
            newUpgradeForm.Title = "Ominstallation af Windows 7-datamat";
            newUpgradeForm.Desription = "PC-navn: " + affectedComputerName;
            return CreateSR(newUpgradeForm);
        }

        [HttpPost]
        public ActionResult CreateIR(CreateWorkItemModel workitem)
        {
            return createForm("https://service.aau.dk/Incident/New/", workitem);
        }
        [HttpPost]
        public ActionResult CreateSR(CreateWorkItemModel workitem)
        {
            return createForm("https://service.aau.dk/ServiceRequest/New/", workitem);
        }
    }
}
