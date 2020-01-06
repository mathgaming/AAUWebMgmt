using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading;

namespace ITSWebMgmt.Controllers
{
    public abstract class WebMgmtController : Controller
    {
        protected readonly LogEntryContext _context;

        protected WebMgmtController(LogEntryContext context)
        {
            _context = context;
        }

        public ActionResult Error(string message = "Error")
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new { success = false, errorMessage = message });
        }

        public ActionResult Success(string Message = "Success")
        {
            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new { success = true, message = Message });
        }

        public ContentResult AccessDenied()
        {
            return Content("You do not have access to this");
        }

        protected ErrorViewModel HandleError(Exception e)
        {
            var model = new ErrorViewModel();

            model.QueryString = HttpContext.Request.QueryString.Value;
            model.Path = HttpContext.Request.Path;
            model.Error = e;
            model.AffectedUser = HttpContext.User.Identity.Name;
            model.HttpStatusCode = HttpContext.Response.StatusCode;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                sendEmail(model);
            }, null);

            return model;
        }

        private void sendEmail(ErrorViewModel model)
        {
            string body = $"Person: {model.AffectedUser}\n" +
                        $"Http status code: {model.HttpStatusCode}\n" +
                        $"Error: {model.ErrorMessage}\n" +
                        $"Url: {model.Url}\n" +
                        $"Stacktrace:\n{model.Stacktrace}\n";
            EmailHelper.SendEmail("WebMgmt error", body);
        }
    }
}
