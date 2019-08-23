using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public class ManagedByController : WebMgmtController
    {
        public string ErrorMessage;

        public ManagedByController(LogEntryContext context) : base(context) {}

        [HttpPost]
        public ActionResult SaveEditManagedBy([FromBody]string data)
        {
            var parts = data.Split('|');
            SaveManagedBy(parts[0], parts[1]);

            if (ErrorMessage == "")
            {
                return Success();
            }
            else
            {
                return Error(ErrorMessage);
            }
        }

        public void SaveManagedBy(string email, string adpath)
        {
            try
            {
                UserModel model = new UserModel(email);
                if (model.DistinguishedName.Contains("CN="))
                {
                    new GroupADcache(adpath).saveProperty("managedBy", model.DistinguishedName);
                    ErrorMessage = "";
                }
                else
                {
                    ErrorMessage = "Error in email";
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}