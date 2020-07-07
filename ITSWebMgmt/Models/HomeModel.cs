using ITSWebMgmt.Models.Log;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ITSWebMgmt.Models
{
    public class HomeModel
    {
        public IEnumerable<KnownIssue> KnownIssues { get; set; }
        public IEnumerable<LogEntry> LastSearches { get; set; }
        public bool IsFeedback = true;
        public HomeModel(LogEntryContext context, string username)
        {
            KnownIssues = context.KnownIssues.Where(x => x.Active);
            if (username == null)
            {
                username = "Testing account";
            }
            LastSearches = context.LogEntries.Where(x => x.UserName == username && (x.Type == LogEntryType.UserLookup || (x.Type == LogEntryType.ComputerLookup)))
                .Include(x => x.Arguments).OrderByDescending(x => x.TimeStamp).Take(10).ToList();
            LastSearches = LastSearches.Where(x => !x.Arguments[0].Value.Contains("Not found"));
        }
    }
}
