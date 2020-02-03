using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models.Log
{
    public class LogStatisticModel
    {
        private IQueryable<LogEntry> logEntries;
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

            foreach (var tab in computerTabNames)
            {
                int count = loadedComputerTabs.Where(x => x.Arguments[0].Value == tab.name).ToList().Count;
                ComputerTabs.Add(new LogCountStatisticsModel(tab.name, count, tab.date));
            }

            var loadeduserTabs = logEntries.Where(x => x.Type == LogEntryType.LoadedTabUser).ToList();

            sb.Append($"<h1>User tabs</h1>");
            foreach (var tab in userTabNames)
            {
                int count = loadeduserTabs.Where(x => x.Arguments[0].Value == tab.name).ToList().Count;
                UserTabs.Add(new LogCountStatisticsModel(tab.name, count, tab.date));
            }

            LookupCounts = new List<LogCountStatisticsModel>
            {
                new LogCountStatisticsModel("Computer lookups", getCount(LogEntryType.ComputerLookup)),
                new LogCountStatisticsModel("User lookups", getCount(LogEntryType.UserLookup))
            };

            TasksCounts = new List<LogCountStatisticsModel>
            {
                new LogCountStatisticsModel($"Get admin password", getCount(LogEntryType.ComputerAdminPassword)),
                new LogCountStatisticsModel($"Bitlocker enabled", getCount(LogEntryType.Bitlocker)),
                new LogCountStatisticsModel($"Computer deleted from AD", getCount(LogEntryType.ComputerDeletedFromAD), "2019-08-16"),
                new LogCountStatisticsModel($"Responce challence", getCount(LogEntryType.ResponceChallence)),
                new LogCountStatisticsModel($"Moved user OU", getCount(LogEntryType.UserMoveOU)),
                new LogCountStatisticsModel($"unlocked user account", getCount(LogEntryType.UnlockUserAccount)),
                new LogCountStatisticsModel($"Toggled user profile", getCount(LogEntryType.ToggleUserProfile)),
                new LogCountStatisticsModel($"Onedrive", getCount(LogEntryType.Onedrive), "2019-08-13"),
                new LogCountStatisticsModel($"Disabled user from AD", getCount(LogEntryType.DisabledAdUser), "2019-08-23"),
                new LogCountStatisticsModel($"Fixed PCConfigs", getCount(LogEntryType.FixPCConfig), "2019-08-23"),
                new LogCountStatisticsModel($"Changed ManagedBy", getCount(LogEntryType.ChangedManagedBy), "2019-08-27"),
                new LogCountStatisticsModel($"Group lookups", getCount(LogEntryType.GroupLookup), "2019-08-27"),
                new LogCountStatisticsModel($"Computer added to AD group", getCount(LogEntryType.AddedToADGroup), "2019-12-03"),
                new LogCountStatisticsModel($"Enabled user from AD", getCount(LogEntryType.EnabledAdUser), "2020-01-06")
            };
        }

        private int getCount(LogEntryType type)
        {
            return logEntries.Where(x => x.Type == type).ToList().Count;
        }
    }
}
