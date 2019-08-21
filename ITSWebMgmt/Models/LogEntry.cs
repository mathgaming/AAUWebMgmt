using ITSWebMgmt.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class LogEntryContext : DbContext
    {
        public LogEntryContext(DbContextOptions<LogEntryContext> options) : base(options) { }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<LogEntryArgument> LogEntryArguments { get; set; }
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
                    return $"unlocked useraccont {Arguments[0]}";
                case LogEntryType.ToggleUserProfile:
                    return $"toggled romaing profile for user {Arguments[0]}";
                case LogEntryType.Onedrive:
                    return $"added user {Arguments[0]} and {Arguments[1]} to Onedrive groups, case: {Arguments[2]}";
                default:
                    return "LogEntry type not found";
            }
        }

        public override string ToString()
        {
            return $"{DateTimeConverter.Convert(TimeStamp)} - {UserName} {GetDescription()}";
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

    public enum LogEntryType { UserLookup, ComputerLookup, ComputerAdminPassword, Bitlocker, ComputerDeletedFromAD, ResponceChallence, UserMoveOU, UnlockUserAccount, ToggleUserProfile, Onedrive, All = 100};

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
