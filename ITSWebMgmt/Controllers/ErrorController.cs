using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mail;
using System.Threading;
using ITSWebMgmt.Helpers;
using System;
using ITSWebMgmt.Models.Log;

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

            return View("Error", new ErrorViewModel(e.Error, HttpContext));
        }
    }
}
