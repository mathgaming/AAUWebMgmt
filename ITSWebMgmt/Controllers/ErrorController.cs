using Microsoft.AspNetCore.Mvc;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mail;
using System.Threading;

namespace ITSWebMgmt.Controllers
{
    public class ErrorController : Controller
    {
        private string Password = Startup.Configuration["Email-password"];

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var e = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var model = new ErrorViewModel();

            model.QueryString = HttpContext.Request.QueryString.Value;
            model.Path = e.Path;
            model.Error = e.Error;
            model.AffectedUser = HttpContext.User.Identity.Name;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                sendEmail(model);
            }, null);

            return View("Error", model);
        }

        private void sendEmail(ErrorViewModel model)
        {
            MailMessage mail = new MailMessage("mhsv16@its.aau.dk", "platform@its.aau.dk");
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Timeout = 10000;
            client.Credentials = new System.Net.NetworkCredential("mhsv16@its.aau.dk", Password);
            client.Host = "smtp.aau.dk";
            mail.Subject = "WebMgmt error";
            mail.Body = $"Person: {model.AffectedUser}\n" +
                        $"Error: {model.ErrorMessage}\n" +
                        $"Url: {model.Url}\n" +
                        $"Stacktrace:\n{model.Stacktrace}\n";
            client.Send(mail);
        }
    }
}
