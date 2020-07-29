using System.Net.Mail;
using System.Net.Mime;

namespace ITSWebMgmt.Helpers
{
    public static class EmailHelper
    {
        public static void SendEmail(string subject, string body, string to = "platform@its.aau.dk")
        {
            MailMessage mail = new MailMessage("platform@its.aau.dk", to);
            SmtpClient client = new SmtpClient
            {
                Host = "smtp-internal.aau.dk",
                Port = 25,
                EnableSsl = true,
                UseDefaultCredentials = true,
                Timeout = 10000
            };
            mail.Subject = subject;
            mail.Body = body;
            client.Send(mail);
        }

        public static void SendEmailWithAttachment(string subject, string body, string reviever, string file)
        {
            MailMessage mail = new MailMessage("platform@its.aau.dk", reviever);
            SmtpClient client = new SmtpClient
            {
                Host = "smtp-internal.aau.dk",
                Port = 25,
                EnableSsl = true,
                UseDefaultCredentials = true,
                Timeout = 10000
            };
            Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            mail.Attachments.Add(data);
            mail.Subject = subject;
            mail.Body = body;
            client.Send(mail);
        }
    }
}
