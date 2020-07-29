using System.Collections.Generic;
using System.Linq;

namespace ITSWebMgmt.Models.Log
{
    public class LogStatisticModel
    {
        private readonly IQueryable<LogEntry> logEntries;
        public List<LogCountStatisticsModel> ComputerTabs { get; set; } = new List<LogCountStatisticsModel>();
        public List<LogCountStatisticsModel> UserTabs { get; set; } = new List<LogCountStatisticsModel>();
        public List<LogCountStatisticsModel> LookupCounts { get; set; }
        public List<LogCountStatisticsModel> TasksCounts { get; set; }

        public LogStatisticModel(IQueryable<LogEntry> logEntries)
        {
            this.logEntries = logEntries;

            List<(string name, string date)> computerTabNames = new List<(string, string)>() {
                ( "basicinfo", "2019-08-27"),
                ( "groups", "2019-08-27"),
                ( "sccmInfo", "2019-08-27"),
                ( "sccmInventory", "2019-08-27"),
                ( "sccmAV", "2019-08-27"),
                ( "sccmHW", "2019-08-27"),
                ( "rawdata", "2019-08-27"),
                ( "tasks", "2019-08-27"),
                ( "warnings", "2019-08-27"),
                ( "purchase", "2019-09-19"),
                ( "sccmcollections" ,"2019-08-30"),
                ( "rawdatasccm", "2019-08-30"),
                ( "macbasicinfo", "2019-08-30"),
                ( "macHW", "2019-08-30"),
                ( "macSW", "2019-08-30"),
                ( "macgroups", "2019-08-30"),
                ( "macnetwork", "2019-08-30"),
                ( "macloaclaccounts", "2019-08-30"),
                ( "macDisk", "2019-08-30") };
            List<(string name, string date)> userTabNames = new List<(string, string)>() {
                ( "basicinfo", "2019-08-27"),
                ( "groups", "2019-08-27"),
                ( "servicemanager", "2019-08-27"),
                ( "calAgenda", "2019-08-27"),
                ( "computerInformation", "2019-08-27"),
                ( "win7to10", "2019-08-27"),
                ( "fileshares", "2019-08-27"),
                ( "exchange", "2019-08-27"),
                ( "loginscript", "2019-08-27"),
                ( "print", "2019-08-27"),
                ( "rawdata", "2019-08-27"),
                ( "tasks", "2019-08-27"),
                ( "netaaudk", "2019-11-14") };

            var loadedComputerTabs = logEntries.Where(x => x.Type == LogEntryType.LoadedTabComputer).ToList();

            foreach (var (name, date) in computerTabNames)
            {
                int count = loadedComputerTabs.Where(x => x.Arguments[0].Value == name).ToList().Count;
                ComputerTabs.Add(new LogCountStatisticsModel(name, count, date));
            }

            var loadeduserTabs = logEntries.Where(x => x.Type == LogEntryType.LoadedTabUser).ToList();

            foreach (var (name, date) in userTabNames)
            {
                int count = loadeduserTabs.Where(x => x.Arguments[0].Value == name).ToList().Count;
                UserTabs.Add(new LogCountStatisticsModel(name, count, date));
            }

            LookupCounts = new List<LogCountStatisticsModel>
            {
                new LogCountStatisticsModel("Computer lookups", GetCount(LogEntryType.ComputerLookup)),
                new LogCountStatisticsModel("User lookups", GetCount(LogEntryType.UserLookup))
            };

            TasksCounts = new List<LogCountStatisticsModel>
            {
                new LogCountStatisticsModel($"Get admin password", GetCount(LogEntryType.ComputerAdminPassword)),
                new LogCountStatisticsModel($"Bitlocker enabled", GetCount(LogEntryType.Bitlocker)),
                new LogCountStatisticsModel($"Computer deleted from AD", GetCount(LogEntryType.ComputerDeletedFromAD), "2019-08-16"),
                new LogCountStatisticsModel($"Responce challence", GetCount(LogEntryType.ResponceChallence)),
                new LogCountStatisticsModel($"Moved user OU", GetCount(LogEntryType.UserMoveOU)),
                new LogCountStatisticsModel($"unlocked user account", GetCount(LogEntryType.UnlockUserAccount)),
                new LogCountStatisticsModel($"Toggled user profile", GetCount(LogEntryType.ToggleUserProfile)),
                new LogCountStatisticsModel($"Onedrive", GetCount(LogEntryType.Onedrive), "2019-08-13"),
                new LogCountStatisticsModel($"Disabled user from AD", GetCount(LogEntryType.DisabledAdUser), "2019-08-23"),
                new LogCountStatisticsModel($"Fixed PCConfigs", GetCount(LogEntryType.FixPCConfig), "2019-08-23"),
                new LogCountStatisticsModel($"Changed ManagedBy", GetCount(LogEntryType.ChangedManagedBy), "2019-08-27"),
                new LogCountStatisticsModel($"Group lookups", GetCount(LogEntryType.GroupLookup), "2019-08-27"),
                new LogCountStatisticsModel($"Computer added to AD group", GetCount(LogEntryType.AddedToADGroup), "2019-12-03"),
                new LogCountStatisticsModel($"Enabled user from AD", GetCount(LogEntryType.EnabledAdUser), "2020-01-06")
            };
        }

        private int GetCount(LogEntryType type)
        {
            return logEntries.Where(x => x.Type == type).ToList().Count;
        }
    }
}
