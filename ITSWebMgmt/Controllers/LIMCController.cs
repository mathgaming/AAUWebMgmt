using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public class LIMCController : WebMgmtController
    {
        //LIMC stands for LIMC IDM to Mail Converter
        public LIMCController(LogEntryContext context) : base(context) { }
        LIMCModel model;

         public IActionResult Index()
        {
            model = new LIMCModel();
            return View(model);
        }

        public ActionResult GetMail([FromBody]string inputCSV)
        {
            model = new LIMCModel(inputCSV);
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
                List<string> columbs = line.Split(',').ToList();
                string email = columbs[emailIndex];
                if (email != "")
                {
                    email = columbs[secundaryEmailIndex];
                }
                string name = new UserModel(email, false).DisplayName;
                sb.Append($"{email};{columbs[aauNumberIndex]};{name}\n");
            }

            return Success(sb.ToString());
        }
    }
}
