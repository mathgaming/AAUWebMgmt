using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        public void SaveManagedBy(string email, string adpath, string oldEmail)
        {
            try
            {
                if (email == "")
                {
                    new GroupADcache(adpath).clearProperty("managedBy");
                    new Logger(_context).Log(LogEntryType.ChangedManagedBy, HttpContext.User.Identity.Name, new List<string>() { adpath, "(Nothing)", oldEmail });
                    ErrorMessage = "";
                }
                else
                { 
                    UserModel model = new UserModel(email);
                    if (model.DistinguishedName.Contains("CN="))
                    {
                        new GroupADcache(adpath).saveProperty("managedBy", model.DistinguishedName);
                        new Logger(_context).Log(LogEntryType.ChangedManagedBy, HttpContext.User.Identity.Name, new List<string>() { adpath, model.UserPrincipalName, oldEmail });
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