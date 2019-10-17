using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models.Log;
using System.Linq;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System.Collections.Generic;
using System.Text;

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

            StringBuilder sb = new StringBuilder();

            List<string> computerTabNames = new List<string>() { "basicinfo", "groups", "sccmInfo", "sccmInventory", "sccmAV", "sccmHW", "rawdata", "tasks", "warnings" };
            List<string> userTabNames = new List<string>() { "basicinfo", "groups", "servicemanager", "calAgenda", "computerInformation", "win7to10", "fileshares", "exchange", "loginscript", "print", "rawdata", "tasks", "warnings" };

            var loadedComputerTabs = logEntries.Where(x => x.Type == LogEntryType.LoadedTabComputer);

            sb.Append("<h1>General</h1>");
            sb.Append($"Computer lookups: {getCount(LogEntryType.ComputerLookup)}\n");
            sb.Append($"User lookups: {getCount(LogEntryType.UserLookup)}\n");

            sb.Append("<h1>Tasks</h1>");
            sb.Append($"Get admin password: {getCount(LogEntryType.ComputerAdminPassword)}\n");
            sb.Append($"Bitlocker enabled: {getCount(LogEntryType.Bitlocker)}\n");
            sb.Append($"Computer deleted from AD (since 2019-08-16): {getCount(LogEntryType.ComputerDeletedFromAD)}\n");
            sb.Append($"Responce challence: {getCount(LogEntryType.ResponceChallence)}\n");
            sb.Append($"Moved user OU: {getCount(LogEntryType.UserMoveOU)}\n");
            sb.Append($"unlocked user account: {getCount(LogEntryType.UnlockUserAccount)}\n");
            sb.Append($"Toggled user profile: {getCount(LogEntryType.ToggleUserProfile)}\n");
            sb.Append($"Onedrive (since 2019-08-13): {getCount(LogEntryType.Onedrive)}\n");
            sb.Append($"Disabled user from AD (since 2019-08-23): {getCount(LogEntryType.DisabledAdUser)}\n");
            sb.Append($"Fixed PCConfigs (since 2019-08-23): {getCount(LogEntryType.FixPCConfig)}\n");
            sb.Append($"Changed ManagedBy (since 2019-08-27): {getCount(LogEntryType.ChangedManagedBy)}\n");
            sb.Append($"Group lookups (since 2019-08-27): {getCount(LogEntryType.GroupLookup)}\n");

            sb.Append($"\n(All statistics below is since 2019-08-22)");
            sb.Append($"<h1>Computer tabs</h1>");
            foreach (var tab in computerTabNames)
            {
                int count = loadedComputerTabs.Where(x => x.Arguments[0].Value == tab).ToList().Count;
                sb.Append($"{tab}: {count}\n");
            }

            var loadeduserTabs = logEntries.Where(x => x.Type == LogEntryType.LoadedTabUser);

            sb.Append($"<h1>User tabs</h1>");
            foreach (var tab in userTabNames)
            {
                int count = loadeduserTabs.Where(x => x.Arguments[0].Value == tab).ToList().Count;
                sb.Append($"{tab}: {count}\n");
            }

            return View(new SimpleModel(sb.ToString()));
        }

        private int getCount(LogEntryType type)
        {
            return logEntries.Where(x => x.Type == type).ToList().Count;
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
