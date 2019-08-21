using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ITSWebMgmt.Controllers
{
    public abstract class WebMgmtController : Controller
    {
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
    }
}
