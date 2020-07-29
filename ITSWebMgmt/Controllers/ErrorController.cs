using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace ITSWebMgmt.Controllers
{
    public class ErrorController : Controller
    {
        public ErrorController()
        {
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var e = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            return View("Error", new ErrorViewModel(e.Error, HttpContext, e.Path));
        }
    }
}
