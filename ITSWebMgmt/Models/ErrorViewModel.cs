using ITSWebMgmt.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;

namespace ITSWebMgmt.Models
{
    public class ErrorViewModel
    {
        public Exception Error { get; set; }
        public string AffectedUser { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public int HttpStatusCode { get; set; }
        public string Url { get => Path + QueryString; }
        public string ErrorMessage { get => Error.Message; }
        public string Stacktrace { get => Error.StackTrace; }

        public ErrorViewModel(Exception e, HttpContext HttpContext, string path = null)
        {
            QueryString = HttpContext.Request.QueryString.Value;
            if (path != null)
            {
                Path = path;
            }
            else
            {
                Path = HttpContext.Request.Path;
            }
            Error = e;
            AffectedUser = HttpContext.User.Identity.Name;
            HttpStatusCode = HttpContext.Response.StatusCode;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                SendEmail();
            }, null);
        }

        private void SendEmail()
        {
            string body = $"Person: {AffectedUser}\n" +
                        $"Http status code: {HttpStatusCode}\n" +
                        $"Error: {ErrorMessage}\n" +
                        $"Url: {Url}\n" +
                        $"Stacktrace:\n{Stacktrace}\n";
            EmailHelper.SendEmail("WebMgmt error", body);
        }
    }
}