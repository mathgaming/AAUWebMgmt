using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using System;
using System.Collections.Generic;

namespace ITSWebMgmt.Controllers
{
    public class TestController : WebMgmtController
    {
        public TestController(WebMgmtContext context) : base(context) { }

        public void Index()
        {
            throw new NotImplementedException();

            List<MacCSVInfo> infos = new ØSSCSVImporter().Import();

            _context.MacCSVInfos.AddRange(infos);
            _context.SaveChanges();
        }
    }
}
