using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public abstract class DynamicLoadingWebMgmtController : WebMgmtController
    {        
        protected DynamicLoadingWebMgmtController(WebMgmtContext context) : base(context) {}

        [HttpGet]
        public abstract Task<IActionResult> LoadTab(string tabName, string name);
    }
}
