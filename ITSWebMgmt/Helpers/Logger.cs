using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class Logger
    {
        private readonly LogEntryContext _context;

        public Logger(LogEntryContext context)
        {
            _context = context;
        }

        public void Log(LogEntryType type, string userName, List<string> arguments, bool hidden = false)
        {
            LogEntry entry = new LogEntry(type, userName, arguments, hidden);

            _context.Add(entry);
            _context.SaveChangesAsync();
        }

        public void Log(LogEntryType type, string userName, string argument, bool hidden = false) => Log(type, userName, new List<string>() { argument }, hidden);
    }
}
