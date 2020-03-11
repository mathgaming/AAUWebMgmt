using ITSWebMgmt.Caches;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class ComputerListModel
    {
        private static string path = Path.Combine(Directory.GetCurrentDirectory(), "computer-list/");
        private static readonly string filename = path + "computer-list.txt";
        private static List<string> emails = new List<string>();
        private static readonly string mailbody = "Computer list is attached.\n";
        public static bool Running { private set; get; } = false;

        public ComputerListModel()
        {
            if (!Running)
            {
                Running = true;
                MakeList();
                CombineLists();
                foreach (var e in emails)
                {
                    EmailHelper.SendEmailWithAttachment("Computer list", mailbody, e, path + "computer-list-full.txt");
                }
                CleanUp();
                emails.Clear();
                Running = false;
            }
        }

        public static void AddEmail(string email)
        {
            if (!emails.Contains(email))
            {
                emails.Add(email);
            }
        }

        public void CleanUp()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name != "computer-list-full.txt")
                {
                    file.Delete();
                }
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public void CombineLists()
        {
            int batchId = 0;
            List<List<string>> batches = Readbatches();
            string outFilename = path + "computer-list-full.txt";
            using StreamWriter outFile = new StreamWriter(outFilename);
            outFile.WriteLine("upn;last logon for user;user uses onedrive;computername;os;uses onedrive;free disk space (GB);virtual?;last login date;last login user");

            foreach (var batch in batches)
            {
                string batchFilename = $"{path}computer-list-{batchId}.txt";
                if (File.Exists(batchFilename))
                {
                    StreamReader file = new StreamReader(batchFilename);
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        outFile.WriteLine(line);
                    }
                    file.Close();
                }
                batchId++;
            }

            outFile.Close();
        }

        private List<List<string>> Readbatches()
        {
            List<List<string>> batches = new List<List<string>>();
            StreamReader file = new StreamReader(filename);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                batches.Add(line.Split(";").ToList());
            }
            file.Close();

            return batches;
        }

        public void MakeList()
        {
            string group = "LDAP://CN=Aau-staff,OU=Email,OU=Groups,DC=aau,DC=dk";
            GroupADcache ad = new GroupADcache(group);
            var members = ad.getGroups("member");
            List<string> allMembers = new List<string>();

            foreach (var member in members)
            {
                ad = new GroupADcache("LDAP://" + member);
                var temp = ad.getGroupsTransitive("member");
                allMembers.AddRange(temp);
            }

            var membersADPath = allMembers.Distinct().ToList();

            List<List<string>> batches = new List<List<string>>();
            int count = 0;
            List<string> batch = new List<string>();
            if (File.Exists(filename))
            {
                batches = Readbatches();
            }
            else
            {
                using StreamWriter file = new StreamWriter(filename);
                foreach (var adpath in membersADPath)
                {
                    if (adpath.Contains("OU=People") || adpath.Contains("OU=Admin Identities"))
                    {
                        if (count == 100)
                        {
                            file.WriteLine(string.Join(";", batch));
                            batches.Add(batch);
                            batch = new List<string>();
                            count = 0;
                        }
                        batch.Add(adpath);
                        count++;
                    }
                }

                file.WriteLine(string.Join(";", batch));
                batches.Add(batch);
                file.Close();
            }

            count = 0;

            foreach (var b in batches)
            {
                RunBatch(b, count);
                count++;
            }
        }

        private List<string> getWindowsInformation(UserModel model, string formattedName)
        {
            List<string> computerInfo = new List<string>();

            foreach (ManagementObject o in model.getUserMachineRelationshipFromUserName(formattedName))
            {
                var computerName = o.Properties["ResourceName"].Value.ToString();
                var computerModel = new WindowsComputerModel(computerName);
                var onedrive = computerModel.UsesOnedrive;
                var @virtual = "unknown";
                if (computerName.StartsWith("AAU"))
                {
                    @virtual = "false";
                }
                else if (computerName.StartsWith("SRV"))
                {
                    @virtual = "true";
                }
                try
                {
                    var diskspace = -1;
                    if (SCCM.HasValues(computerModel.Disk))
                    {
                        var disk = computerModel.LogicalDisk.OfType<ManagementObject>().FirstOrDefault();
                        diskspace = disk.Properties["FreeSpace"].Value != null ? (int.Parse(disk.Properties["FreeSpace"].Value.ToString()) / 1024) : -1;
                    }
                    string time = computerModel.System.GetProperty("LastLogonTimestamp");
                    var date = time != null ? DateTimeConverter.Convert(time) : "";
                    var lastLoginUser = computerModel.System.GetProperty("LastLogonUserDomain") + "\\" + computerModel.System.GetProperty("LastLogonUserName");
                    computerInfo.Add($"{computerName};windows;{onedrive};{diskspace};{@virtual};{date};{lastLoginUser}");
                }
                catch (Exception e )
                {
                    computerInfo.Add($"{computerName};windows;{onedrive};;{@virtual};;;Failed to get data for {computerName}");
                }
            }

            return computerInfo;
        }

        private List<string> getMacInformation(string upn)
        {
            List<string> computerInfo = new List<string>();
            JamfConnector jamf = new JamfConnector();
            foreach (var email in new UserModel(upn, false).getUserMails())
            {
                foreach (var computerName in jamf.getComputerNamesForUser(email))
                {
                    var @virtual = "unknown";
                    var onedrive = "";
                    var date = "";
                    var lastLoginUser = "";
                    MacComputerModel macComputer = new MacComputerModel(computerName);
                    var diskspace = macComputer.FreeSpace;
                    if (computerName.StartsWith("AAU"))
                    {
                        @virtual = "false";
                    }
                    else if (computerName.StartsWith("AAUVM"))
                    {
                        @virtual = "true";
                    }
                    computerInfo.Add($"{computerName};mac;{onedrive};{diskspace};{@virtual};{date};{lastLoginUser}");
                }
            }

            return computerInfo;
        }

        public void RunBatch(List<string> adpaths, int batch)
        {
            string batchFilename = $"{path}computer-list-{batch}.txt";
            if (!File.Exists(batchFilename))
            {
                using StreamWriter file = new StreamWriter(batchFilename);
                foreach (var adpath in adpaths)
                {
                    string upn = "";
                    try
                    {
                        var split = adpath.Split(',');
                        var name = split[0].Replace("CN=", "");
                        var domain = split.Where(s => s.StartsWith("DC=")).ToArray()[0].Replace("DC=", "");
                        upn = $"{name}@{domain}.aau.dk";

                        UserModel model = new UserModel(adpath, "");
                        string formattedName = string.Format("{0}\\\\{1}", domain, name);
                        List<string> computerInfo = new List<string>();

                        computerInfo.AddRange(getWindowsInformation(model, formattedName));
                        computerInfo.AddRange(getMacInformation(upn));

                        var lastLogon = model.LastLogon;
                        var usesOnedrive = model.UsesOnedrive;
                        foreach (var computer in computerInfo)
                        {
                            file.WriteLine($"{upn};{lastLogon};{usesOnedrive};{computer}");
                        }

                        if (computerInfo.Count == 0)
                        {
                            file.WriteLine($"{upn};{lastLogon};{usesOnedrive};;;;;;;;No computer found for user");
                        }
                    }
                    catch (Exception e)
                    {
                        file.WriteLine($"{upn};;;;;;;;;;Error finding {adpath}");
                        Console.WriteLine(e.Message);
                    }
                    Thread.Sleep(1000);
                }

                file.Close();
            }
        }
    }
}
