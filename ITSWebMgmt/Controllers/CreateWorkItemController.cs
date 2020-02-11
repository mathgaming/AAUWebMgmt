using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ITSWebMgmt.Controllers
{
    public class CreateWorkItemController : Controller
    {
        private static readonly HttpClient HttpClient;

        static CreateWorkItemController()
        {
            HttpClient = new HttpClient();
        }
        public IActionResult Index(string userPrincipalName, string userID, bool isfeedback = false)
        {
            CreateWorkItemModel model = new CreateWorkItemModel();
            model.IsFeedback = isfeedback;
            model.UserID = userID;
            if (isfeedback)
            {
                string[] parts = HttpContext.User.Identity.Name.Split('\\');
                string domain = $"{parts[0].ToLower()}.aau.dk";
                string user = parts[1];
                model.AffectedUser = $"{user}@{domain}";
            }
            else
            {
                model.AffectedUser = userPrincipalName;
            }

            UserModel userModel = new UserModel(model.AffectedUser);
            userModel.InitBasicInfo();

            if (!isfeedback)
            {
                model.Desription = "\n\n\n\n\n" +
                    "\nDo not edit below this line" +
                    "\n(The format is shown correctly on service.aau.dk)" +
                    "\n----------------------------------------------------------" +
                    "\nDepartment:                 " + userModel.BasicInfoDepartmentPDS +
                    "\nOffice(Pure):                  " + userModel.BasicInfoOfficePDS +
                    "\nPassword Expired:        " + userModel.BasicInfoLocked +
                    "\nPassword Expire Date: " + userModel.BasicInfoPasswordExpireDate +
                    "\nAAU-ID:                        " + userModel.AAUAAUID +
                    "\nUserStatus:                   " + userModel.AAUUserStatus +
                    "\nStaffID:                         " + userModel.AAUStaffID +
                    "\nStudentID:                    " + userModel.AAUStudentID +
                    "\nUserClassification:        " + userModel.AAUUserClassification +
                    "\nTelephone:                    " + userModel.TelephoneNumber +
                    "\nUses OneDrive?            " + userModel.UsesOnedrive +
                    "\nRomaing Profile            " + userModel.BasicInfoRomaing;
            }

            return View(model);
        }
        public IActionResult Win7Index (string userPrincipalName, string computerName, string userID)
        {
            //Special case of returning predefined form for upgrading windows 7 pc's
            return createWindows7UpgradeForm(userPrincipalName, computerName, userID);
        }

        protected string createRedirectCode(CreateWorkItemModel model, string description, string submiturl)
        /**Legacy function, do not use.
         * Only left in until the function CreateItemInServiceManagerAsync has been tested with the Service Manager
         * I would've done it myself, but I do not have permission to access it.
         * Oh the joy of development.**/
        {
            string supportGroup = "";
            if (model.IsFeedback)
            {
                supportGroup = ",\"TierQueue\":{\"Id\":\"41f4f742-129f-1aa1-5e81-636653b38fb9\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                            ",\"SupportGroup\":{\"Id\":\"bfbd6899-ab71-d508-7f09-4a337763a468\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                             ",\"Classification\":{\"Id\":\"ab6f9057-874d-36bb-5d4d-d9117b878916\",\"Name\":\"Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                              ",\"Area\":{ \"Id\":\"5316e1e3-4ad0-bead-c437-68b84a90e725\",\"Name\":\"Andet - Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}";
            }

            string jsondata = "{\"Title\":\"" + model.Title + "\"" + supportGroup + ",\"Description\":\"" + description + "\",\"RequestedWorkItem\":{\"BaseId\":\"" + model.UserID + "\",\"DisplayName\":\"" + model.AffectedUser + "\",}}";
            var sb = new StringBuilder();

            sb.Append("<html><head>");
            sb.Append("</head>");
            sb.Append(@"<body onload='document.forms[""form""].submit()'>");

            sb.Append("<form name='form' target='_blank' method='post' action='" + submiturl + "'>");

            sb.Append("<input hidden='true' name='vm' value='" + jsondata + "'/>");
            sb.Append("<input type='submit' value='Recreate IR/SR (if someting whent wrong)' />");

            sb.Append(@"<br /><br /><a href=""#"" onclick=""window.history.go(-2); return false;""> Go back </a>");

            sb.Append("</form>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private string InitFeedbackGroup(CreateWorkItemModel model)
        {
            string feedbackGroup = "";
            if (model.IsFeedback)
            {
                feedbackGroup = ",\"TierQueue\":{\"Id\":\"41f4f742-129f-1aa1-5e81-636653b38fb9\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                            ",\"SupportGroup\":{\"Id\":\"bfbd6899-ab71-d508-7f09-4a337763a468\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                             ",\"Classification\":{\"Id\":\"ab6f9057-874d-36bb-5d4d-d9117b878916\",\"Name\":\"Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                              ",\"Area\":{ \"Id\":\"5316e1e3-4ad0-bead-c437-68b84a90e725\",\"Name\":\"Andet - Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}";
            }
            return JsonConvert.SerializeObject(feedbackGroup, Formatting.None);
        }

        protected string FormatWorkItemToJson(CreateWorkItemModel model)
        {
            if (model.UserID == null)
            {
                SCSMConnector sccm = new SCSMConnector();

                _ = sccm.getServiceManager(model.AffectedUser).Result;

                model.UserID = sccm.userID;
            }

            if (model.Desription != null)
            {
                model.Desription = model.Desription.Replace("\n", "\\n").Replace("\r", "\\r");
            }

            string feedbackGroup = InitFeedbackGroup(model);
            return JsonConvert.SerializeObject(model, Formatting.None);
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

        private async Task<ActionResult> CreateItemInServiceManagerAsync(string url, CreateWorkItemModel model)
        {
            string modelAsJson = FormatWorkItemToJson(model);
            StringContent content = new StringContent(modelAsJson);
            try
            {
                HttpResponseMessage response = await HttpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return new RedirectResult(url);
            }
            catch (HttpRequestException e)
            {
                return new StatusCodeResult(502); //Bad Gateway
            }
        }
        
        private ActionResult CreateItemInServiceManager(string url, CreateWorkItemModel model)
        {
            return CreateItemInServiceManagerAsync(url, model).Result;
        }

        [HttpPost]
        public ActionResult CreateIR(CreateWorkItemModel workitem)
        {
            workitem.IsFeedback = false;
            return CreateItemInServiceManager("https://service.aau.dk/Incident/New/", workitem);
        }
        [HttpPost]
        public ActionResult CreateSR(CreateWorkItemModel workitem)
        {
            workitem.IsFeedback = false;
            return CreateItemInServiceManager("https://service.aau.dk/ServiceRequest/New/", workitem);
        }

        [HttpPost]
        public ActionResult ReportIssue(CreateWorkItemModel workitem)
        {
            workitem.IsFeedback = true;
            return CreateItemInServiceManager("https://service.aau.dk/Incident/New/", workitem);
        }
        [HttpPost]
        public ActionResult RequestNewFeature(CreateWorkItemModel workitem)
        {
            workitem.IsFeedback = true;
            return CreateItemInServiceManager("https://service.aau.dk/ServiceRequest/New/", workitem);
        }
    }
}
