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

         public IActionResult Index()
        {
            LIMCModel limcModel = new LIMCModel();
            return View(limcModel);
        }

    }
}
