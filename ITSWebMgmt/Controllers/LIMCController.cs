using System;
using System.Collections.Generic;
using System.Linq;
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
            model.MailOutput = model.RawCSVInput;
            List<string> usernameList = inputCSV.Split(",").ToList();
            List<string> emailList = new List<string>();
            foreach (string username in usernameList)
            {
                UserModel userModel = new UserModel(username);
            }

            return Success(model.MailOutput);
        }

    }
}
