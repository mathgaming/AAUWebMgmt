using Microsoft.EntityFrameworkCore;

namespace ITSWebMgmt.Models.Log
{
    public class WebMgmtContext : DbContext
    {
        public WebMgmtContext(DbContextOptions<WebMgmtContext> options) : base(options) { }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<LogEntryArgument> LogEntryArguments { get; set; }
        public DbSet<KnownIssue> KnownIssues { get; set; }
        public DbSet<MissingGroup> MacErrors { get; set; }
        public DbSet<TrashRequest> TrashRequests { get; set; }
        public DbSet<MacCSVInfo> MacCSVInfos { get; set; }
    }
}
