using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public abstract class DynamicLoadingWebMgmtController : WebMgmtController
    {        
        protected DynamicLoadingWebMgmtController(WebMgmtContext context) : base(context) {}

        [HttpGet]
        public abstract ActionResult LoadTab(string tabName, string name);
    }
}
