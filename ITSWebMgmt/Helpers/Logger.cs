using ITSWebMgmt.Models.Log;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            _context.SaveChanges();
        }

        public void Log(LogEntryType type, string userName, string argument, bool hidden = false) => Log(type, userName, new List<string>() { argument }, hidden);

        public void Log(LogEntryType type, string userName, List<string> arguments, bool hidden, DateTime date)
        {
            LogEntry entry = new LogEntry(type, userName, arguments, hidden);
            entry.TimeStamp = date;

            _context.Add(entry);
        }

        public void FixImport()
        {
            if (File.Exists(@"C:\webmgmtlog\fix-importlogfile.txt"))
            {
                List<DateTime> dates = new List<DateTime>();
                List<string> argumentsTo = new List<string>();
                List<string> argumentsFrom = new List<string>();
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\webmgmtlog\fix-importlogfile.txt");
                while ((line = file.ReadLine()) != null)
                {
                    var parts = line.Split(" ");

                    if (line.Contains("changed OU on user to:") && !line.Contains("{3}"))
                    {
                        int startTo = line.IndexOf("LDAP");
                        int endTo = line.IndexOf(" from");
                        int startFrom = line.IndexOf("LDAP", line.IndexOf("LDAP") + 1);
                        int endFrom = line.Length - 1;

                        DateTime date = DateTime.Parse(parts[0] + " " + parts[1].Split("|")[0]);

                        argumentsTo.Add(line.Substring(startTo, endTo - startTo));
                        argumentsFrom.Add(line.Substring(startFrom, endFrom - startFrom));
                        dates.Add(date);
                    }
                }

                file.Close();
                File.Delete(@"C:\webmgmtlog\fix-importlogfile.txt");

                var logEntries = _context.LogEntries.Include(e => e.Arguments).AsNoTracking().Where(s => s.Type == LogEntryType.UserMoveOU);

                foreach (var logEntry in logEntries)
                {
                    int index = dates.IndexOf(logEntry.TimeStamp);
                    if (index != -1)
                    {
                        LogEntry current = _context.LogEntries.Include(e => e.Arguments).AsNoTracking().Where(s => s.TimeStamp == logEntry.TimeStamp).FirstOrDefault();
                        List<LogEntryArgument> args = _context.LogEntryArguments.AsNoTracking().Where(b => EF.Property<int>(b, "LogEntryId") == current.Id).ToList();
                        foreach (var arg in args)
                        {
                            current.Arguments.Remove(arg);
                            _context.Remove(arg);
                        }

                        current.Arguments.Clear();
                        current.Arguments.Add(new LogEntryArgument(argumentsTo[index]));
                        current.Arguments.Add(new LogEntryArgument(argumentsFrom[index]));
                        _context.Update(current);
                    }
                }

                _context.SaveChanges();
            }
        }

        public string getADPathFromString(string input)
        {
            int start = input.IndexOf("LDAP");
            int end = input.IndexOf("(Hidden)");
            
            if (start == -1)
            {
                var parts = input.Split(" ");
                return parts[parts.Length - 1];
            }
            else if (end > start)
            {
                return input.Substring(start, end - start);
            }

            return input.Substring(start);
        }

        public void UpdateIdsFromFile()
        {
            if (File.Exists(@"C:\webmgmtlog\server-backup - Copy.sql"))
            {
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\webmgmtlog\server-backup - Copy.sql");
                FileStream outputFile = File.Create(@"C:\webmgmtlog\server-backup-updated.sql");
                StreamWriter output = new StreamWriter(outputFile);
                while ((line = file.ReadLine()) != null)
                {
                    var parts = line.Split(" ");

                    if (line.Length > 90)
                    {
                        if (line.Contains("LogEntries"))
                        {
                            int startId = line.IndexOf("S (") + 3;
                            int IdLength = 6;
                            string oldId = line.Substring(startId, IdLength);

                            line = line.Replace(oldId, (int.Parse(oldId) + 47634).ToString());
                        }
                        else if (line.Contains("LogEntryArguments"))
                        {
                            int startId = line.IndexOf("S (") + 3;
                            int IdLength = 5;
                            int startIdRef = line.IndexOf("', ") + 3;
                            int IdRefLength = 6;

                            string oldId = line.Substring(startId, IdLength);
                            string oldIdRef = line.Substring(startIdRef, IdRefLength);

                            line = line.Replace(oldId, (int.Parse(oldId) + 120).ToString());
                            line = line.Replace(oldIdRef, (int.Parse(oldIdRef) + 47634).ToString());
                        }
                    }
                    output.WriteLine(line);
                }

                file.Close();
                output.Close();
            }
        }

        public void ImportLogEntriesFromFile()
        {
            FixImport(); // Fix wrongly imported ADPaths because of spaces in the path (only for LogEntryType.UserMoveOU)
            UpdateIdsFromFile();
            if (File.Exists(@"C:\webmgmtlog\importlogfile.txt"))
            {
                //Newest added from new = 2019-08-16 13:58:01.9908|INFO|ITSWebMgmt.Controllers.WebMgmtController|User ITS\ampo18 lookedup user mgranl18@student.aau.dk (Hidden)
                //Newest added from old = 2019-08-20 10:45:15.8220|INFO|ITSWebMgmt.Controllers.Controller`1|User AUB\ano requesed localadmin password for computer LDAP://aub.aau.dk/CN=AAU112419,OU=Clients,DC=aub,DC=aau,DC=dk
                string line;
                int count = 0;
                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\webmgmtlog\importlogfile.txt");
                while ((line = file.ReadLine()) != null)
                {
                    var parts = line.Split(" ");
                    DateTime date = DateTime.Parse(parts[0] + " " + parts[1].Split("|")[0]);
                    bool hidden = false;
                    string username = parts[2];
                    LogEntryType type = 0;
                    List<string> arguments = new List<string>();

                    if (parts[parts.Length - 1] == "(Hidden)")
                    {
                        hidden = true;
                    }

                    if (line.Contains("requesed info about computer"))
                    {
                        type = LogEntryType.ComputerLookup;
                        arguments.Add(getADPathFromString(line));
                        if (!hidden)
                        {
                            hidden = true;
                        }
                    }

                    else if (line.Contains("lookedup user"))
                    {
                        type = LogEntryType.UserLookup;
                        if (hidden)
                        {
                            arguments.Add(parts[parts.Length - 2]);
                        }
                        else
                        {
                            arguments.Add(parts[parts.Length - 1]);
                            hidden = true;
                        }
                    }

                    else if (line.Contains("requesed localadmin password for computer"))
                    {
                        type = LogEntryType.ComputerAdminPassword;
                        arguments.Add(getADPathFromString(line));
                        if (!hidden)
                        {
                            hidden = true;
                        }
                    }

                    else if (line.Contains("to Onedrive groups"))
                    {
                        type = LogEntryType.Onedrive;
                        arguments.Add(parts[5]);
                        arguments.Add(parts[7]);
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    // Spelling mistake in log
                    else if (line.Contains("unlocked useraccont"))
                    {
                        type = LogEntryType.UnlockUserAccount;
                        arguments.Add(getADPathFromString(line));
                    }

                    else if (line.Contains("toggled romaing profile for user"))
                    {
                        type = LogEntryType.ToggleUserProfile;
                        arguments.Add(getADPathFromString(line));
                    }

                    else if (line.Contains("generated challange with reason"))
                    {
                        type = LogEntryType.ResponceChallence;
                        string search = " generated challange with reason ";
                        int start = line.IndexOf(search) + search.Length;
                        arguments.Add(line.Substring(start));
                    }

                    else if (line.Contains("changed OU on user to:"))
                    {
                        type = LogEntryType.UserMoveOU;

                        int startTo = line.IndexOf("LDAP");
                        int endTo = line.IndexOf(" from");
                        int startFrom = line.IndexOf("LDAP", line.IndexOf("LDAP") + 1);
                        int endFrom = line.Length - 1;

                        arguments.Add(line.Substring(startTo, endTo - startTo));
                        arguments.Add(line.Substring(startFrom, endFrom - startFrom));
                    }

                    else if (line.Contains(" from AD"))
                    {
                        type = LogEntryType.ComputerDeletedFromAD;
                        arguments.Add(parts[parts.Length - 3]);
                    }

                    else if (line.Contains("enabled bitlocker for"))
                    {
                        type = LogEntryType.Bitlocker;
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    Log(type, username, arguments, hidden, date);

                    count++;
                    if (count % 100 == 0)
                    {
                        _context.SaveChanges();
                    }
                }

                file.Close();
                File.Delete(@"C:\webmgmtlog\importlogfile.txt");
                _context.SaveChanges();
            }
        }
    }
}
