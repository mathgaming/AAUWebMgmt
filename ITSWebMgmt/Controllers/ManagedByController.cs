using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public class ManagedByController : WebMgmtController
    {
        public string ErrorMessage;

        [HttpPost]
        public ActionResult SaveEditManagedBy([FromBody]string email)
        {
            SaveManagedBy(email);

            if (ErrorMessage == "")
            {
                return Success();
            }
            else
            {
                return Error(ErrorMessage);
            }
        }

        public void SaveManagedBy(string email)
        {
            try
            {
                UserModel model = new UserModel(email);
                if (model.DistinguishedName.Contains("CN="))
                {
                    new GroupADcache(model.adpath).saveProperty("managedBy", model.DistinguishedName);
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