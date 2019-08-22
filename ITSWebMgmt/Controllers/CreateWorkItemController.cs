using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class CreateWorkItemController : Controller
    {
        public IActionResult Index(string userPrincipalName, string userID)
        {
            CreateWorkItemModel model = new CreateWorkItemModel();
            model.AffectedUser = userPrincipalName;
            model.UserID = userID;

            UserModel userModel = new UserModel(userPrincipalName);
            userModel.InitBasicInfo();

            model.Desription = "\n\n\n\n\n" +
                "\nDo not edit below this line" +
                "\n(The format is shown correctly on service.aau.dk)" +
                "\n----------------------------------------------------------" +
                "\nDepartment:                 " + userModel.BasicInfoDepartmentPDS +
                "\nOffice(Pure):                  " + userModel.BasicInfoOfficePDS +
                "\nPassword Expired:        " + userModel.BasicInfoPasswordExpired +
                "\nPassword Expire Date: " + userModel.BasicInfoPasswordExpireDate +
                "\nAAU-ID:                        " + userModel.AAUAAUID +
                "\nUserStatus:                   " + userModel.AAUUserStatus +
                "\nStaffID:                         " + userModel.AAUStaffID +
                "\nStudentID:                    " + userModel.AAUStudentID +
                "\nUserClassification:        " + userModel.AAUUserClassification +
                "\nTelephone:                    " + userModel.TelephoneNumber +
                "\nUses OneDrive?            " + userModel.UsesOnedrive +
                "\nRomaing Profile            " + userModel.BasicInfoRomaing;

            return View(model);
        }
        public IActionResult Win7Index (string userPrincipalName, string computerName, string userID)
        {
            //Special case of returning predefined form for upgrading windows 7 pc's
            return createWindows7UpgradeForm(userPrincipalName, computerName, userID);
        }

        protected string createRedirectCode(string userID, string userPrincipalName, string title, string description, string submiturl)
        {
            string jsondata = "{\"Title\":\"" + title + "\",\"Description\":\"" + description + "\",\"RequestedWorkItem\":{\"BaseId\":\"" + userID + "\",\"DisplayName\":\"" + userPrincipalName + "\",}}";
            var sb = new StringBuilder();

            sb.Append("<html><head>");
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
            if (model.UserID == null)
            {
                SCSMConnector sccm = new SCSMConnector();

                _ = sccm.getActiveIncidents(model.AffectedUser).Result;

                model.UserID = sccm.userID;
            }

            string descriptionConverted = "";
            if (model.Desription != null)
            {
                descriptionConverted = model.Desription.Replace("\n", "\\n").Replace("\r", "\\r");
            }
            return Content(createRedirectCode(model.UserID, model.AffectedUser, model.Title, descriptionConverted, url), "text/html");
        }

        [HttpPost]
        protected ActionResult createWindows7UpgradeForm(string computerOwner, string affectedComputerName, string userID)
        {
            CreateWorkItemModel newUpgradeForm = new CreateWorkItemModel();
            newUpgradeForm.AffectedUser = computerOwner;
            newUpgradeForm.Title = "Opgradering af Windows 7 til 10";
            newUpgradeForm.Desription = "PC-navn: " + affectedComputerName;
            newUpgradeForm.UserID = userID;
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
