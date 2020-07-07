using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models.Log;
using System.Linq;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Controllers
{
    public class LogController : WebMgmtController
    {
        public LogController(LogEntryContext context) : base(context) { }

        private IQueryable<LogEntry> logEntries;

        public IActionResult Statistics()
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }
            logEntries = _context.LogEntries.Include(e => e.Arguments).AsNoTracking();

            return View(new LogStatisticModel(logEntries));
        }

        // GET: LogEntries
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, string showHidden, LogEntryType? type, int? pageNumber)
        {
            if (Authentication.IsNotPlatform(HttpContext.User.Identity.Name))
            {
                return AccessDenied();
            }

            //Does not work on server, because it does not have access to the file
            //new Logger(_context).ImportLogEntriesFromFile();

            if (type == null)
            {
                type = LogEntryType.All;
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["TypeFilter"] = type;
            ViewData["HiddenFilter"] = showHidden;

            if (string.IsNullOrEmpty(sortOrder))
            {
                ViewData["NameSortParm"] = "name_desc";
                ViewData["DateSortParm"] = "date";
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

            if (type != null && type != LogEntryType.All)
            {
                logEntries = logEntries.Where(s => s.Type == type);
            }

            logEntries = sortOrder switch
            {
                "name" => logEntries.OrderBy(s => s.UserName),
                "name_desc" => logEntries.OrderByDescending(s => s.UserName),
                "type" => logEntries.OrderBy(s => s.Type),
                "type_desc" => logEntries.OrderByDescending(s => s.Type),
                "date" => logEntries.OrderBy(s => s.TimeStamp),
                _ => logEntries.OrderByDescending(s => s.TimeStamp),
            };
            int pageSize = 20;
            return View(await PaginatedList<LogEntry>.CreateAsync(logEntries.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}
