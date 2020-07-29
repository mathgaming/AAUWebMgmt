using System.Collections.Generic;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class CreateWorkItemController : Controller
    {
        public IActionResult Index(string userPrincipalName, string userID, bool isfeedback = false)
        {
            CreateWorkItemModel model = new CreateWorkItemModel
            {
                IsFeedback = isfeedback,
                UserID = userID
            };
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
                model.Description = "\n\n\n\n\n" +
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
            return CreateWindows7UpgradeForm(userPrincipalName, computerName, userID);
        }

        protected string FormatWorkItemToJson(CreateWorkItemModel model)
        {
            if (model.UserID == null)
            {
                SCSMConnector sccm = new SCSMConnector();

                _ = sccm.GetServiceManager(model.AffectedUser, new List<string>{model.AffectedUser}).Result;

                model.UserID = sccm.userID;
            }

            if (model.Description != null)
            {
                model.Description = model.Description.Replace("\n", "\\n").Replace("\r", "\\r");
            }

            string supportGroup = "";
            if (model.IsFeedback)
            {
                supportGroup = ",\"TierQueue\":{\"Id\":\"41f4f742-129f-1aa1-5e81-636653b38fb9\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                            ",\"SupportGroup\":{\"Id\":\"bfbd6899-ab71-d508-7f09-4a337763a468\",\"Name\":\"Client Management: Windows\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                             ",\"Classification\":{\"Id\":\"ab6f9057-874d-36bb-5d4d-d9117b878916\",\"Name\":\"Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}" +
                              ",\"Area\":{ \"Id\":\"5316e1e3-4ad0-bead-c437-68b84a90e725\",\"Name\":\"Andet - Web og Portalsværktøjer\",\"HierarchyLevel\":0,\"HierarchyPath\":null}";
            }

            return "{\"Title\":\"" + model.Title + "\"" + supportGroup + ",\"Description\":\"" + model.Description + "\",\"RequestedWorkItem\":{\"BaseId\":\"" + model.UserID + "\",\"DisplayName\":\"" + model.AffectedUser + "\",}}";
        }

        [HttpPost]
        protected ActionResult CreateWindows7UpgradeForm(string computerOwner, string affectedComputerName, string userID)
        {
            CreateWorkItemModel newUpgradeForm = new CreateWorkItemModel
            {
                AffectedUser = computerOwner,
                Title = "Opgradering af Windows 7 til 10",
                Description = "PC-navn: " + affectedComputerName,
                UserID = userID
            };
            return CreateSR(newUpgradeForm);
        }
        
        private ActionResult CreateItemInServiceManager(string url, CreateWorkItemModel model)
        {
            return View("RedirectView", new WorkItemRedirectModel(url, FormatWorkItemToJson(model)));
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
            SendEmail(workitem, "isssue reported");
            return CreateItemInServiceManager("https://service.aau.dk/Incident/New/", workitem);
        }
        [HttpPost]
        public ActionResult RequestNewFeature(CreateWorkItemModel workitem)
        {
            workitem.IsFeedback = true;
            SendEmail(workitem, "feature requested");
            return CreateItemInServiceManager("https://service.aau.dk/ServiceRequest/New/", workitem);
        }

        private void SendEmail(CreateWorkItemModel workitem, string type)
        {
            string title = $"WebMgmt: New {type}: {workitem.Title}";
            string description = $"Title: {workitem.Title}\nUser: {workitem.AffectedUser}\nDescription: {workitem.Description}";
            EmailHelper.SendEmail(title, description, "mhsv16@its.aau.dk");
        }
    }
}
