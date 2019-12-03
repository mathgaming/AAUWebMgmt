using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mail;
using System.Threading;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Controllers
{
    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var e = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var model = new ErrorViewModel();

            model.QueryString = HttpContext.Request.QueryString.Value;
            model.Path = e.Path;
            model.Error = e.Error;
            model.AffectedUser = HttpContext.User.Identity.Name;
            model.HttpStatusCode = HttpContext.Response.StatusCode;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                sendEmail(model);
            }, null);

            return View("Error", model);
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
