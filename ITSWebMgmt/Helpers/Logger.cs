using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public void ImportLogEntriesFromFile()
        {
            if (File.Exists(@"C:\webmgmtlog\importlogfile.txt"))
            {
                //Newest added from new = 2019-08-16 13:58:01.9908|INFO|ITSWebMgmt.Controllers.WebMgmtController|User ITS\ampo18 lookedup user mgranl18@student.aau.dk (Hidden)
                //Newest added from old = 2019-08-20 10:45:15.8220|INFO|ITSWebMgmt.Controllers.Controller`1|User AUB\ano requesed localadmin password for computer LDAP://aub.aau.dk/CN=AAU112419,OU=Clients,DC=aub,DC=aau,DC=dk
                string line;
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

                    else if (line.Contains("to Onedrive groups"))
                    {
                        type = LogEntryType.Onedrive;
                        arguments.Add(parts[5]);
                        arguments.Add(parts[7]);
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    else if (line.Contains("unlocked useraccont"))
                    {
                        type = LogEntryType.UnlockUserAccount;
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    else if (line.Contains("toggled romaing profile for user"))
                    {
                        type = LogEntryType.ToggleUserProfile;
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    else if (line.Contains("generated challange with reason"))
                    {
                        type = LogEntryType.ResponceChallence;
                        arguments.Add(parts[parts.Length - 1]);
                    }

                    else if (line.Contains("changed OU on user to:"))
                    {
                        type = LogEntryType.UserMoveOU;
                        arguments.Add(parts[parts.Length - 3]);
                        arguments.Add(parts[parts.Length - 1].Remove(parts[parts.Length - 1].Length - 1));
                    }

                    else if (line.Contains(" from AD"))
                    {
                        type = LogEntryType.ComputerDeletedFromAD;
                        arguments.Add(parts[parts.Length - 3]);
                    }

                    Log(type, username, arguments, hidden, date);
                }

                file.Close();
            }
        }
    }
}
