using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;

namespace ITSWebMgmt.Models.Log
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
        public List<LogEntryArgument> Arguments { get; set; }
        public bool Hidden { get; set; }
        public LogEntryType Type { get; set; }

        public LogEntry() { }

        public LogEntry(LogEntryType type, string userName, List<string> arguments, bool hidden = false)
        {
            TimeStamp = DateTime.Now;
            Type = type;
            UserName = userName;
            if (UserName == null)
            {
                UserName = "Testing account";
            }
            Arguments = new List<LogEntryArgument>();
            foreach (string argument in arguments)
            {
                Arguments.Add(new LogEntryArgument(argument));
            }

            Hidden = hidden;
        }

        public LogEntry(LogEntryType type, string userName, string argument, bool hidden = false) : this(type, userName, new List<string>() { argument }, hidden) { }

        public string GetDescription()
        {
            return Type switch
            {
                LogEntryType.UserLookup => $"lookedup user {Arguments[0]}",
                LogEntryType.ComputerLookup => $"requesed info about computer {Arguments[0]}",
                LogEntryType.ComputerAdminPassword => $"requesed localadmin password for computer {Arguments[0]}",
                LogEntryType.Bitlocker => $"enabled bitlocker for {Arguments[0]}.",
                LogEntryType.ComputerDeletedFromAD => $"deleted {Arguments[0]} from AD",
                LogEntryType.ResponceChallence => $"generated challange with reason {Arguments[0]}",
                LogEntryType.UserMoveOU => $"changed OU on user to: {Arguments[0]} from {Arguments[1]}.",
                LogEntryType.UnlockUserAccount => $"unlocked useraccount {Arguments[0]}",
                LogEntryType.ToggleUserProfile => $"toggled romaing profile for user {Arguments[0]}",
                LogEntryType.Onedrive => $"added user {Arguments[0]} and {Arguments[1]} to Onedrive groups, case: {Arguments[2]}",
                LogEntryType.LoadedTabUser => $"loaded tab {Arguments[0]} for user {Arguments[1]}",
                LogEntryType.LoadedTabComputer => $"loaded tab {Arguments[0]} for computer {Arguments[1]}",
                LogEntryType.DisabledAdUser => $"disabled {Arguments[0]} from AD beacuse {Arguments[1]}, case: {Arguments[2]}",
                LogEntryType.FixPCConfig => $"set PCConfig for {Arguments[0]} to {Arguments[1]}",
                LogEntryType.ChangedManagedBy => $"changed ManagedBy for computer/group {Arguments[0]} to {Arguments[1]} from {Arguments[2]}",
                LogEntryType.GroupLookup => $"lookedup group {Arguments[0]}",
                LogEntryType.AddedToADGroup => $"{Arguments[0]} was added to the AD group: {Arguments[1]}",
                LogEntryType.EnabledAdUser => $"enaabled {Arguments[0]} from AD, case: {Arguments[1]}",
                _ => "LogEntry type not found",
            };
        }

        public override string ToString()
        {
            return $"{DateTimeConverter.Convert(TimeStamp)} - {UserName} {GetDescription()}";
        }
    }

    public enum LogEntryType { UserLookup, ComputerLookup, ComputerAdminPassword, Bitlocker, ComputerDeletedFromAD, ResponceChallence, UserMoveOU, UnlockUserAccount, ToggleUserProfile, Onedrive, LoadedTabUser, LoadedTabComputer, DisabledAdUser, FixPCConfig, ChangedManagedBy, GroupLookup, AddedToADGroup, EnabledAdUser, All = 100};
}
