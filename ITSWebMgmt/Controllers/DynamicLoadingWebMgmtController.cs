using Microsoft.AspNetCore.Mvc;

namespace ITSWebMgmt.Controllers
{
    public abstract class DynamicLoadingWebMgmtController : WebMgmtController
    {
        [HttpGet]
        public abstract ActionResult LoadTab(string tabName, string name);
    }
}
