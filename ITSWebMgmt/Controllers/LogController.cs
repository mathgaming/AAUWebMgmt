using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models;
using System.Linq;
using ITSWebMgmt.Helpers;

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
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, string showHidden, int? type, int? pageNumber)
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

            new Logger(_context).ImportLogEntriesFromFile();

            ViewData["CurrentSort"] = sortOrder;

            if (string.IsNullOrEmpty(sortOrder))
            {
                ViewData["NameSortParm"] = "name_desc";
                ViewData["DateSortParm"] = "date_desc";
                ViewData["TypeSortParm"] = "type_desc";
            }
            else
            {
                ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
                ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
                ViewData["TypeSortParm"] = sortOrder == "type" ? "type_desc" : "type";
            }

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var logEntries = _context.LogEntries.Include(e => e.Arguments).AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
            {
                logEntries = logEntries.Where(s => s.UserName.Contains(searchString));
            }

            if (showHidden != "on")
            {
                logEntries = logEntries.Where(s => s.Hidden == false);
            }

            if (type != null && type != 100)
            {
                logEntries = logEntries.Where(s => (int)s.Type == type);
            }

            switch (sortOrder)
            {
                case "name":
                    logEntries = logEntries.OrderBy(s => s.UserName);
                    break;
                case "name_desc":
                    logEntries = logEntries.OrderByDescending(s => s.UserName);
                    break;
                case "type":
                    logEntries = logEntries.OrderBy(s => s.Type);
                    break;
                case "type_desc":
                    logEntries = logEntries.OrderByDescending(s => s.Type);
                    break;
                case "date":
                    logEntries = logEntries.OrderBy(s => s.TimeStamp);
                    break;
                default:
                    logEntries = logEntries.OrderByDescending(s => s.TimeStamp);
                    break;
            }

            int pageSize = 20;
            return View(await PaginatedList<LogEntry>.CreateAsync(logEntries.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}
