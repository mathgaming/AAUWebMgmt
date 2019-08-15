﻿using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public class ManagedByController : Controller
    {
        public string ErrorMessage;

        [HttpPost]
        public ActionResult SaveEditManagedBy([FromBody]string email)
        {
            SaveManagedBy(email);

            if (ErrorMessage == "")
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return Json(new { success = true });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, errorMessage = ErrorMessage });
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