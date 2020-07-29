using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
            SaveManagedBy(parts[0], parts[1], parts[2]);

            if (ErrorMessage == "")
            {
                return Success();
            }
            else
            {
                return Error(ErrorMessage);
            }
        }

        public void SaveManagedBy(string email, string ADPath, string oldEmail)
        {
            try
            {
                if (email == "")
                {
                    new GroupADCache(ADPath).ClearProperty("managedBy");
                    new Logger(_context).Log(LogEntryType.ChangedManagedBy, HttpContext.User.Identity.Name, new List<string>() { ADPath, "(Nothing)", oldEmail });
                    ErrorMessage = "";
                }
                else
                { 
                    UserModel model = new UserModel(email);
                    if (model.DistinguishedName.Contains("CN="))
                    {
                        new GroupADCache(ADPath).SaveProperty("managedBy", model.DistinguishedName);
                        new Logger(_context).Log(LogEntryType.ChangedManagedBy, HttpContext.User.Identity.Name, new List<string>() { ADPath, model.UserPrincipalName, oldEmail });
                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "Error in email";
                    }
                }
                
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}