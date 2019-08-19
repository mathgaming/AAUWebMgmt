using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;

namespace ITSWebMgmt.Controllers
{
    public class LogController : Controller
    {
        private readonly LogEntryContext _context;

        public LogController(LogEntryContext context)
        {
            _context = context;
        }

        // GET: LogEntries
        public async Task<IActionResult> Index()
        {
            ViewData["HaveAccess"] = false;
            if (HttpContext.User.Identity.Name != null)
            {
                UserModel userModel = new UserModel(HttpContext.User.Identity.Name, false);
                var temp = userModel.ADcache.getGroups("memberOf");

                if (temp.Any(x => x.Contains("CN=platform")))
                {
                    ViewData["HaveAccess"] = true;
                }
            }

            return View(await _context.LogEntries.Include(e => e.Arguments).ToListAsync());
        }
    }
}
