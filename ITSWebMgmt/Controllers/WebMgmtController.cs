using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

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
            return new ErrorViewModel(e, HttpContext);
        }
    }
}
