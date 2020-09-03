using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class CSVController : WebMgmtController
    {
        public CSVController(LogEntryContext context) : base(context) { }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult GetMail([FromBody]string inputCSV)
        {
            string outputString = "";
            List<string> usernameList = inputCSV.Split(",").ToList();
            foreach (string username in usernameList)
            {
                if (!outputString.Length.Equals(0))
                {
                    outputString += ',';
                }
                UserModel userModel = new UserModel(username, false);
                List<string> emails = userModel.GetUserMails();
                outputString += string.Join(",", emails);
            }
            return Success(outputString);
        }

        public ActionResult JamfConvert([FromBody]string inputCSV)
        {
            List<string> lines = inputCSV.Split('\n').ToList();
            List<string> headers = lines[0].Split('\t').ToList();

            int emailIndex = headers.IndexOf("Email Address");
            int secundaryEmailIndex = headers.IndexOf("AAU-1x Username");
            int aauNumberIndex = headers.IndexOf("Asset Tag");
            int computernameIndex = headers.IndexOf("Computer Name");
            int snIndex = headers.IndexOf("Serial Number");
            
            StringBuilder sb = new StringBuilder();

            sb.Append("Email;AAUNumber;Navn;ComputerNavn;Serial Number\n");

            foreach (var line in lines.Skip(1))
            {
                if (line.Length > 3)
                {
                    List<string> columbs = line.Split('\t').ToList();
                    string email = columbs[emailIndex];
                    if (email == "" || email == "Unknown")
                    {
                        email = columbs[secundaryEmailIndex];
                    }
                    if (email != "" && email != "Unknown")
                    {
                        try
                        {
                            var user = new UserModel(email, false);
                            string user_email = user.UserFound ? user.GetUserMails()[0] : "not found";
                            string name = user.UserFound ? user.DisplayName : "Not found";
                            string computerName = columbs[computernameIndex];
                            string sn = columbs[snIndex];
                            sb.Append($"{user_email};{columbs[aauNumberIndex]};{name};{computerName};{sn}\n");
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            return Success(sb.ToString());
        }
    }
}
