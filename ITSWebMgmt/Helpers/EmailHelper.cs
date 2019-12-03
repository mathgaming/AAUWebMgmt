using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public static class EmailHelper
    {
        public static void SendEmail(string subject, string body)
        {
            MailMessage mail = new MailMessage("platform@its.aau.dk", "platform@its.aau.dk");
            SmtpClient client = new SmtpClient();
            client.Host = "smtp-internal.aau.dk";
            client.Port = 25;
            client.EnableSsl = true;
            client.UseDefaultCredentials = true;
            client.Timeout = 10000;
            mail.Subject = subject;
            mail.Body = body;
            client.Send(mail);
        }
    }
}
