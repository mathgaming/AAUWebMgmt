using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models.Log;
using System;

namespace ITSWebMgmt.Controllers
{
    public class TestController : WebMgmtController
    {
        public TestController(LogEntryContext context) : base(context) { }

        public void Index()
        {
            var øss = new ØSSConnector();

            //throw new NotImplementedException();
        }
    }
}
