﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Web;
using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Models
{
    public class WindowsComputerModel : WebMgmtModel<ComputerADcache>
    {
        //SCCMcache
        public ManagementObjectCollection RAM { get => SCCMcache.RAM; private set { } }
        public ManagementObjectCollection LogicalDisk { get => SCCMcache.LogicalDisk; private set { } }
        public ManagementObjectCollection BIOS { get => SCCMcache.BIOS; private set { } }
        public ManagementObjectCollection VideoController { get => SCCMcache.VideoController; private set { } }
        public ManagementObjectCollection Processor { get => SCCMcache.Processor; private set { } }
        public ManagementObjectCollection Disk { get => SCCMcache.Disk; private set { } }
        public ManagementObjectCollection Software { get => SCCMcache.Software; private set { } }
        public ManagementObjectCollection Computer { get => SCCMcache.Computer; private set { } }
        public ManagementObjectCollection Antivirus { get => SCCMcache.Antivirus; private set { } }
        public ManagementObjectCollection System { get => SCCMcache.System; private set { } }
        public ManagementObjectCollection Collection { get => SCCMcache.Collection; private set { } }

        //ADcache
        public string ComputerName { get => BaseModel.ComputerName; }
        public string ComputerNameAD { get => ADcache.ComputerName; }
        public string Domain { get => ADcache.Domain; }
        public bool ComputerFound { get => ADcache.ComputerFound; set => ADcache.ComputerFound = value; }
        public string AdminPasswordExpirationTime { get => ADcache.getProperty("ms-Mcs-AdmPwdExpirationTime"); }
        public string ManagedByAD { get => ADcache.getProperty("managedBy"); set => ADcache.saveProperty("managedBy", value); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }
        public DateTime WhenCreated { get => ADcache.getProperty("whenCreated"); }

        //Display
        public string PasswordExpireDate
        {
            get
            {
                if (AdminPasswordExpirationTime != null)
                {
                    return AdminPasswordExpirationTime;
                }
                else
                {
                    return "LAPS not Enabled";
                }
            }
        }
        public string ConfigPC { get; set; } = "Unknown";
        public string ConfigExtra { get; set; } = "False";
        public ManagedByModel ManagedBy { get; set; }
        public TableModel SCCMInfoComputer { get; set; }
        public TableModel SCCMInfoSystem { get; set; }
        public TableModel SCCMSoftware { get; set; }
        public TableModel SCCMCollections { get; set; }
        public TableModel SCCMLD { get; set; }
        public string SCCMRAM { get; set; }
        public TableModel SCCMBIOS { get; set; }
        public TableModel SCCMVC { get; set; }
        public TableModel SCCMProcessor { get; set; }
        public TableModel SCCMDisk { get; set; }
        public TableModel SCCMAV { get; set; }
        public bool ShowResultGetPassword { get; set; } = false;
        public bool ShowMoveComputerOUdiv { get; set; } = false;
        public bool UsesOnedrive { get; set; } = false;
        public bool HasJava()
        {
            foreach (ManagementObject o in Software)
            {
                string name = SCCM.GetPropertyAsString(o.Properties["ProductName"]).ToLower();
                if (name.Contains("java") && name.Contains("Update"))
                {
                    return true;
                }
            }
            return false;
        }
        public ComputerModel BaseModel { get; set; }

        public WindowsComputerModel(ComputerModel baseModel)
        {
            BaseModel = baseModel;
            if (ComputerName != null)
            {
                ADcache = new ComputerADcache(ComputerName);
                if (ADcache.ComputerFound)
                {
                    SCCMcache = new SCCMcache(ComputerNameAD);
                    BaseModel.ComputerName = ADcache.ComputerName;
                    BaseModel.ComputerFound = ADcache.ComputerFound;
                    LoadDataInbackground();
                }
            }
        }

        public WindowsComputerModel(string computerName) : this(new ComputerModel(computerName)) { }

        public List<string> setConfig()
        {
            ManagementObjectCollection Collection = this.Collection;
            WindowsComputerModel ComputerModel = this;

            if (SCCM.HasValues(Collection))
            {
                List<string> namesInCollection = new List<string>();
                foreach (ManagementObject o in Collection)
                {
                    //o.Properties["ResourceID"].Value.ToString();
                    var collectionID = o.Properties["CollectionID"].Value.ToString();

                    if (collectionID.Equals("AA100015"))
                    {
                        ComputerModel.ConfigPC = "AAU7 PC";
                    }
                    else if (collectionID.Equals("AA100087"))
                    {
                        ComputerModel.ConfigPC = "AAU8 PC";
                    }
                    else if (collectionID.Equals("AA1000BC"))
                    {
                        ComputerModel.ConfigPC = "AAU10 PC";
                        ComputerModel.ConfigExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA100027"))
                    {
                        ComputerModel.ConfigPC = "Administrativ7 PC";
                    }
                    else if (collectionID.Equals("AA1001BD"))
                    {
                        ComputerModel.ConfigPC = "Administrativ10 PC";
                        ComputerModel.ConfigExtra = "True"; // Hardcode AAU10 is bitlocker enabled
                    }
                    else if (collectionID.Equals("AA10009C"))
                    {
                        ComputerModel.ConfigPC = "Imported";
                    }

                    if (collectionID.Equals("AA1000B8"))
                    {
                        ComputerModel.ConfigExtra = "True";
                    }

                    var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                    ManagementPath path = new ManagementPath(pathString);
                    ManagementObject obj = new ManagementObject();

                    obj.Scope = SCCM.ms;
                    obj.Path = path;
                    obj.Get();

                    namesInCollection.Add(obj["Name"].ToString());
                }
                return namesInCollection;
            }
            return null;
        }

        #region loading data
        public void InitBasicInfo()
        {
            //Managed By
            if (!string.IsNullOrWhiteSpace(ManagedByAD))
            {
                string email = ADHelper.DistinguishedNameToUPN(ManagedByAD);
                ManagedBy = new ManagedByModel(adpath, HttpUtility.HtmlEncode("LDAP://" + ManagedByAD), email);
            }
            else
            {
                ManagedBy = new ManagedByModel(adpath, "", "");
            }

            UsesOnedrive = OneDriveHelper.ComputerUsesOneDrive(ADcache);

            if (AdminPasswordExpirationTime != null)
            {
                ShowResultGetPassword = true;
            }
        }

        public void InitSCCMHW()
        {
            SCCMLD = CreateVerticalTableFromDatabase(LogicalDisk,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                "Disk information not found");

            if (SCCM.HasValues(RAM))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in RAM) //Has one!
                {
                    total += int.Parse(o.Properties["Capacity"].Value.ToString()) / 1024;
                    count++;
                }

                SCCMRAM = $"{total} GB RAM in {count} slot(s)";
            }
            else
            {
                SCCMRAM = "RAM information not found";
            }

            SCCMBIOS = CreateVerticalTableFromDatabase(BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            SCCMVC = CreateVerticalTableFromDatabase(VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            SCCMProcessor = CreateVerticalTableFromDatabase(Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            SCCMDisk = CreateVerticalTableFromDatabase(Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");
        }

        public TableModel CreateVerticalTableFromDatabase(ManagementObjectCollection results, List<string> keys, string errorMessage)
        {
            List<string[]> rows = new List<string[]>();

            if (SCCM.HasValues(results))
            {
                var o = results.OfType<ManagementObject>().FirstOrDefault();

                foreach (var p in keys)
                {
                    var property = o.Properties[p];
                    if (p == "Size" || p == "FreeSpace")
                    {
                        var value = o.Properties[p].Value;
                        if (value != null)
                        {
                            rows.Add(new string[] { p + " (GB)", (int.Parse(value.ToString()) / 1024).ToString() });
                        }
                        else
                        {
                            rows.Add(new string[] { p + " (GB)", "missing" });
                        }
                    }
                    else
                    {
                        rows.Add(new string[] { p, SCCM.GetPropertyAsString(property) });
                    }
                }

                return new TableModel(new string[] { "Property", "Value" }, rows);
            }
            else
            {
                return new TableModel(errorMessage);
            }
        }

        public void InitSCCMCollections()
        {
            List<string[]> rows = new List<string[]>();
            var names = setConfig();

            if (names != null)
            {
                foreach (var name in names)
                {
                    rows.Add(new string[] { name });
                }
                SCCMCollections = new TableModel(new string[] { "Collection Name" }, rows, "SCCM collections");
            }
            else
            {
                SCCMCollections = new TableModel("Computer not found i SCCM", "SCCM collections");
            }
        }

        public void InitSCCMAV()
        {
            SCCMAV = CreateTableFromDatabase(Antivirus, new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" }, "Antivirus information not found", "Antivirus infomation");
        }

        public TableModel CreateTableFromDatabase(ManagementObjectCollection results, List<string> keys, string errorMessage, string title) => CreateTableFromDatabase(results, keys, keys, errorMessage, title);

        public TableModel CreateTableFromDatabase(ManagementObjectCollection results, List<string> keys, List<string> names, string errorMessage, string title)
        {
            if (SCCM.HasValues(results))
            {
                List<string[]> rows = new List<string[]>();

                foreach (ManagementObject o in results) //Has one!
                {
                    List<string> properties = new List<string>();
                    foreach (var p in keys)
                    {
                        properties.Add(SCCM.GetPropertyAsString(o.Properties[p]));
                    }
                    rows.Add(properties.ToArray());
                }

                return new TableModel(names.ToArray(), rows, title);
            }
            else
            {
                return new TableModel(errorMessage, title);
            }
        }

        public void InitSCCMSoftware()
        {
            SCCMSoftware = CreateTableFromDatabase(Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found", "Software infomation");    
        }

        public void InitSCCMInfo()
        {
            SCCMInfoSystem = CreateVerticalTableFromDatabase(System, new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build" }, "Computer not found in SCCM");
            SCCMInfoComputer = CreateVerticalTableFromDatabase(Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "Computer information not found");
        }
        #endregion loding data
    }
}
