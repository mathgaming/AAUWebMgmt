using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models.Log
{
    public class LogEntryContext : DbContext
    {
        public LogEntryContext(DbContextOptions<LogEntryContext> options) : base(options) { }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<LogEntryArgument> LogEntryArguments { get; set; }
        public DbSet<KnownIssue> KnownIssues { get; set; }
    }
}
