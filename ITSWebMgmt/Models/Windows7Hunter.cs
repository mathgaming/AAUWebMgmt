using ITSWebMgmt.Caches;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class Windows7Hunter
    {
        List<Windows7Model> windows7s = new List<Windows7Model>();

        public Windows7Hunter()
        {
            //SCCM.CreateWindows7List();
            ParseOutput();
            LookUpComputers();
            SaveFile();
            Console.WriteLine();
        }

        public void LookUpComputers()
        {
            foreach (var computer in windows7s)
            {
                var ADcache = new ComputerADcache(computer.Name);
                if (ADcache.ComputerFound)
                {
                    var ManagedByAD = ADcache.getProperty("managedBy");
                    if (!string.IsNullOrWhiteSpace(ManagedByAD))
                    {
                        computer.ManageBy = ADHelper.DistinguishedNameToUPN(ManagedByAD);
                    }
                }
                if (INDBConnector.LookupComputer(computer.Name) != "Computer not found")
                {
                    var indb = INDBConnector.getInfo(computer.Name);
                    if (indb[0].ErrorMessage == null)
                    {
                        computer.KøbtTilHvem = indb[0].Rows[5][1];
                        computer.ComputerBrand = indb[0].Rows[1][1];
                        computer.ComputerModel = indb[0].Rows[2][1];
                    }
                }
                SCCMcache SCCMCache = new SCCMcache(computer.Name);
                if (SCCMCache.ResourceID != null)
                {
                    try
                    {
                        computer.ComputerBrandSCCM = SCCM.GetPropertyAsString(SCCMCache.Computer, "Manufacturer");
                        computer.ComputerModelSCCM = SCCM.GetPropertyAsString(SCCMCache.Computer, "Model");
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void SaveFile()
        {
            using (StreamWriter file = new StreamWriter(@"C:\webmgmtlog\windows7list.csv"))
            {
                file.WriteLine("Name;ComputerBrandINDB;ComputerModelINDB;ComputerBrandSCCM;ComputerModelSCCM;MACAddresses;OSVersion;OSBuildVersion;Domain;LastActiveTime;UserDomainName;UserName;LastActiveTime;Emailadress;ManageBy;KøbtTilHvem");
                foreach (var computer in windows7s)
                {
                    file.WriteLine(computer);
                }
            }
        }

        public void ParseOutput()
        {
            string path = @"C:\webmgmtlog\windows7noquotes.csv";
            string text = File.ReadAllText(@"C:\webmgmtlog\windows7.csv");
            text = text.Replace("\"", "");
            text = text.Replace("#TYPE Selected.Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine.WqlResultObject\r\nname,MACAddress,DeviceOS,DeviceOSBuild,Domain,LastLogonUser,UserDomainName,UserName,LastActiveTime", "");
            File.WriteAllText(path, text);

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    int macOffset = 0;
                    foreach (var field in fields)
                    {
                        if (field.StartsWith("Microsoft"))
                        {
                            break;
                        }

                        macOffset++;
                    }
                    Windows7Model windows7 = new Windows7Model()
                    {
                        Name = fields[0],
                        MACAddresses = new List<string>(),
                        OSVersion = fields[macOffset + 0],
                        OSBuildVersion = fields[macOffset + 1],
                        Domain = fields[macOffset + 2],
                        LastLogonUser = fields[macOffset + 3],
                        UserDomainName = fields[macOffset + 4],
                        UserName = fields[macOffset + 5],
                        LastActiveTime = fields[macOffset + 6],
                        Emailadress = $"{fields[macOffset + 5]}@{fields[macOffset + 4]}.aau.dk"
                    };
                    for (int i = 1; i < macOffset; i++)
                    {
                        windows7.MACAddresses.Add(fields[i]);
                    }
                    if (windows7.OSVersion.Contains("6.1") || windows7.OSVersion.Contains("Workstation 6.3"))
                    {
                        windows7s.Add(windows7);
                    }
                }
            }
        }
    }

    public class Windows7Model
    {
        public string Name { get; set; }
        public string ComputerBrand { get; set; }
        public string ComputerModel { get; set; }
        public string ComputerBrandSCCM { get; set; }
        public string ComputerModelSCCM { get; set; }
        public List<string> MACAddresses { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildVersion { get; set; }
        public string Domain { get; set; }
        public string LastLogonUser { get; set; }
        public string UserDomainName { get; set; }
        public string UserName { get; set; }
        public string LastActiveTime { get; set; }
        public string Emailadress { get; set; }
        public string ManageBy { get; set; }
        public string KøbtTilHvem { get; set; }

        public override string ToString()
        {
            return $"{Name};{ComputerBrand};{ComputerModel};{ComputerBrandSCCM};{ComputerModelSCCM};{string.Join(",", MACAddresses)};{OSVersion};{OSBuildVersion};{Domain};{LastActiveTime};{UserDomainName};{UserName};{LastActiveTime};{Emailadress};{ManageBy};{KøbtTilHvem}";
        }
    }
}
