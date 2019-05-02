using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public abstract class DynamicLoadingWebMgmtController : WebMgmtController
    {
        [HttpGet]
        public abstract ActionResult LoadTab(string tabName, string name);
    }
}
