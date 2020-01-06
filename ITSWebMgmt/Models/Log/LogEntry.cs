using ITSWebMgmt.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            switch (Type)
            {
                case LogEntryType.UserLookup:
                    return $"lookedup user {Arguments[0]}";
                case LogEntryType.ComputerLookup:
                    return $"requesed info about computer {Arguments[0]}";
                case LogEntryType.ComputerAdminPassword:
                    return $"requesed localadmin password for computer {Arguments[0]}";
                case LogEntryType.Bitlocker:
                    return $"enabled bitlocker for {Arguments[0]}.";
                case LogEntryType.ComputerDeletedFromAD:
                    return $"deleted {Arguments[0]} from AD";
                case LogEntryType.ResponceChallence:
                    return $"generated challange with reason {Arguments[0]}";
                case LogEntryType.UserMoveOU:
                    return $"changed OU on user to: {Arguments[0]} from {Arguments[1]}.";
                case LogEntryType.UnlockUserAccount:
                    return $"unlocked useraccount {Arguments[0]}";
                case LogEntryType.ToggleUserProfile:
                    return $"toggled romaing profile for user {Arguments[0]}";
                case LogEntryType.Onedrive:
                    return $"added user {Arguments[0]} and {Arguments[1]} to Onedrive groups, case: {Arguments[2]}";
                case LogEntryType.LoadedTabUser:
                    return $"loaded tab {Arguments[0]} for user {Arguments[1]}";
                case LogEntryType.LoadedTabComputer:
                    return $"loaded tab {Arguments[0]} for computer {Arguments[1]}";
                case LogEntryType.DisabledAdUser:
                    return $"disabled {Arguments[0]} from AD beacuse {Arguments[1]}, case: {Arguments[2]}";
                case LogEntryType.FixPCConfig:
                    return $"set PCConfig for {Arguments[0]} to {Arguments[1]}";
                case LogEntryType.ChangedManagedBy:
                    return $"changed ManagedBy for computer/group {Arguments[0]} to {Arguments[1]} from {Arguments[2]}";
                case LogEntryType.GroupLookup:
                    return $"lookedup group {Arguments[0]}";
                case LogEntryType.AddedToADGroup:
                    return $"{Arguments[0]} was added to the AD group: {Arguments[1]}";
                case LogEntryType.EnabledAdUser:
                    return $"enaabled {Arguments[0]} from AD, case: {Arguments[1]}";
                default:
                    return "LogEntry type not found";
            }
        }

        public override string ToString()
        {
            return $"{DateTimeConverter.Convert(TimeStamp)} - {UserName} {GetDescription()}";
        }
    }

    public enum LogEntryType { UserLookup, ComputerLookup, ComputerAdminPassword, Bitlocker, ComputerDeletedFromAD, ResponceChallence, UserMoveOU, UnlockUserAccount, ToggleUserProfile, Onedrive, LoadedTabUser, LoadedTabComputer, DisabledAdUser, FixPCConfig, ChangedManagedBy, GroupLookup, AddedToADGroup, EnabledAdUser, All = 100};
}
