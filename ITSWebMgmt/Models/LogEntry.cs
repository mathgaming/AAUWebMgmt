using ITSWebMgmt.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ITSWebMgmt.Models
{
    public class LogEntryContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=WebMgmtDB;Trusted_Connection=True;MultipleActiveResultSets=true;");
        }

        public DbSet<LogEntry> LogEntries { get; set; }
    }

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

        public override string ToString()
        {
            string dateAndTime = $"{DateTimeConverter.Convert(TimeStamp)} - {UserName} ";
            switch (Type)
            {
                case LogEntryType.UserLookup:
                    return dateAndTime + $"lookedup user {Arguments[0]}";
                case LogEntryType.ComputerLookup:
                    return dateAndTime + $"requesed info about computer {Arguments[0]}";
                case LogEntryType.ComputerAdminPassword:
                    return dateAndTime + $"requesed localadmin password for computer {Arguments[0]}";
                case LogEntryType.Bitlocker:
                    return dateAndTime + $"enabled bitlocker for {Arguments[0]}.";
                case LogEntryType.ComputerDeletedFromAD:
                    return dateAndTime + $"deleted {Arguments[0]} from AD";
                case LogEntryType.ResponceChallence:
                    return dateAndTime + $"generated challange with reason {Arguments[0]}";
                case LogEntryType.UserMoveOU:
                    return dateAndTime + $"changed OU on user to: {Arguments[0]} from {Arguments[1]}.";
                case LogEntryType.UnlockUserAccount:
                    return dateAndTime + $"unlocked useraccont {Arguments[0]}";
                case LogEntryType.TuggleUserProfile:
                    return dateAndTime + $"toggled romaing profile for user {Arguments[0]}";
                case LogEntryType.Onedrive:
                    return dateAndTime + $"added user {Arguments[0]} and {Arguments[1]} to Onedrive groups, case: {Arguments[2]}";
                default:
                    return "LogEntry type not found";
            }
        }
    }

    public class LogEntryArgument
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public LogEntryArgument() { }

        public LogEntryArgument(string argument)
        {
            Value = argument;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public enum LogEntryType { UserLookup, ComputerLookup, ComputerAdminPassword, Bitlocker, ComputerDeletedFromAD, ResponceChallence, UserMoveOU, UnlockUserAccount, TuggleUserProfile, Onedrive };
}
