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
    public class FindComputersForAllStaff
    {
        private static string path = @"C:\webmgmtlog\testlist\";
        private readonly string filename = path + "computer-list.txt";

        public FindComputersForAllStaff()
        {
            MakeList();
            CombineLists();
        }

        public void CombineLists()
        {
            int batchId = 0;
            List<List<string>> batches = Readbatches();
            string outFilename = path + "computer-list-full.txt";
            using StreamWriter outFile = new StreamWriter(outFilename);

            foreach (var batch in batches)
            {
                string batchFilename = $"{path}computer-list-{batchId}.txt";
                StreamReader file = new StreamReader(batchFilename);
                string line;
                int count = 0;
                while ((line = file.ReadLine()) != null)
                {
                    outFile.WriteLine(line);
                    count++;
                }

                if (count != batch.Count)
                {
                    Console.WriteLine($"Error in batch {batchId}");
                }

                batchId++;
            }
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

            return batches;
        }

        public void MakeList()
        {
            string group = "LDAP://CN=Aau-staff,OU=Email,OU=Groups,DC=aau,DC=dk";
            GroupADcache ad = new GroupADcache(group);
            var membersADPath = ad.getGroupsTransitive("member");
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
                            file.WriteLine(String.Join(";", batch));
                            batches.Add(batch);
                            batch = new List<string>();
                            count = 0;
                        }
                        batch.Add(adpath);
                        count++;
                    }
                }

                file.WriteLine(String.Join(";", batch));
                batches.Add(batch);
            }

            count = 0;

            foreach (var b in batches)
            {
                RunBatch(b, count);
                count++;
            }
        }

        public void RunBatch(List<string> adpaths, int batch)
        {
            string batchFilename = $"{path}computer-list-{batch}.txt";
            try
            {
                if (!File.Exists(batchFilename))
                {
                    using StreamWriter file = new StreamWriter(batchFilename);
                    foreach (var adpath in adpaths)
                    {
                        var split = adpath.Split(',');
                        var name = split[0].Replace("CN=", "");
                        var domain = split.Where(s => s.StartsWith("DC=")).ToArray()[0].Replace("DC=", "");
                        var upn = $"{name}@{domain}.aau.dk";

                        UserModel model = new UserModel(adpath, "");
                        List<string> windowsNames = new List<string>();
                        string formattedName = string.Format("{0}\\\\{1}", domain, name);

                        foreach (ManagementObject o in model.getUserMachineRelationshipFromUserName(formattedName))
                        {
                            windowsNames.Add(o.Properties["ResourceName"].Value.ToString());
                        }

                        JamfConnector jamf = new JamfConnector();
                        List<string> macComputers = new List<string>();
                        foreach (var email in new UserModel(upn, false).getUserMails())
                        {
                            macComputers.AddRange(jamf.getComputerNamesForUser(email));
                        }

                        string line = $"{upn};{string.Join(",", windowsNames)};{string.Join(",", macComputers)}";
                        file.WriteLine(line);

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
            }
        }
    }
}
