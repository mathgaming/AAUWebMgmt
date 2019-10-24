using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mail;
using System.Threading;

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
            MailMessage mail = new MailMessage("platform@its.aau.dk", "platform@its.aau.dk");
            SmtpClient client = new SmtpClient();
            client.Host = "smtp-internal.aau.dk";
            client.Port = 25;
            client.EnableSsl = true;
            client.UseDefaultCredentials = true;
            client.Timeout = 10000;
            mail.Subject = "WebMgmt error";
            mail.Body = $"Person: {model.AffectedUser}\n" +
                        $"Http status code: {model.HttpStatusCode}\n" +
                        $"Error: {model.ErrorMessage}\n" +
                        $"Url: {model.Url}\n" +
                        $"Stacktrace:\n{model.Stacktrace}\n";
            client.Send(mail);
        }
    }
}
