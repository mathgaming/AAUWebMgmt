﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            List<string> emailList = new List<string>();
            foreach (string username in usernameList)
            {
                if (!outputString.Length.Equals(0))
                {
                    outputString += ',';
                }
                UserModel userModel = new UserModel(username, false);
                List<string> emails = userModel.getUserMails();
                outputString += string.Join(",", emails);
            }
            return Success(outputString);
        }

        public ActionResult JamfConvert([FromBody]string inputCSV)
        {
            List<string> lines = inputCSV.Split('\n').ToList();
            List<string> headers = lines[0].Split(',').ToList();

            int emailIndex = headers.IndexOf("Email Address");
            int secundaryEmailIndex = headers.IndexOf("AAU-1x Username");
            int aauNumberIndex = headers.IndexOf("Asset Tag");
            StringBuilder sb = new StringBuilder();

            sb.Append("Email;AAUNumber;Navn\n");

            foreach (var line in lines.Skip(1))
            {
                if (line.Length > 3)
                {
                    List<string> columbs = line.Split(',').ToList();
                    string email = columbs[emailIndex];
                    if (email == "" || email == "Unknown")
                    {
                        email = columbs[secundaryEmailIndex];
                    }
                    if (email != "" && email != "Unknown")
                    {
                        try
                        {
                            string name = new UserModel(email, false).DisplayName;
                            sb.Append($"{email};{columbs[aauNumberIndex]};{name}\n");
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
