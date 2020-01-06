using System;
using System.Collections.Generic;
using System.Management;
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
        public string ManagedBy { get; set; }
        public string LastLogonUserName { get; set; }
        public string IPAddresses { get; set; }
        public string MACAddresses { get; set; }
        public string Build { get; set; }
        public string SCCMAV { get; set; }
        public string SCCMRaw { get; set; }
        public string SCCMInfoComputer { get; set; }
        public string SCCMInfoSystem { get; set; }
        public string SCCMSoftware { get; set; }
        public string SCCMCollections { get; set; }
        public string SCCMLD { get; set; }
        public string SCCMRAM { get; set; }
        public string SCCMBIOS { get; set; }
        public string SCCMVC { get; set; }
        public string SCCMProcessor { get; set; }
        public string SCCMDisk { get; set; }
        public bool ShowResultGetPassword { get; set; } = false;
        public bool ShowMoveComputerOUdiv { get; set; } = false;
        public bool UsesOnedrive { get; set; } = false;
        public bool HasJava()
        {
            foreach (ManagementObject o in Software) //Has one!
            {
                string name = SCCM.GetPropertyAsString(o.Properties["ProductName"]).ToLower();
                if (name.Contains("java") && name.Contains("Update"))
                {
                    return false;
                }
            }
            return true;
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
                    SCCMcache = new SCCMcache();
                    SCCMcache.ResourceID = getSCCMResourceIDFromComputerName(ComputerNameAD);
                    BaseModel.ComputerName = ADcache.ComputerName;
                    BaseModel.ComputerFound = ADcache.ComputerFound;
                    LoadDataInbackground();
                }
            }
        }

        public WindowsComputerModel(string computerName) : this(new ComputerModel(computerName)) { }

        public string getSCCMResourceIDFromComputerName(string computername)
        {
            string resourceID = "";
            //XXX use ad path to get right object in sccm, also dont get obsolite
            foreach (ManagementObject o in SCCMcache.getResourceIDFromComputerName(computername))
            {
                var tempCache = new SCCMcache();
                tempCache.ResourceID = o.Properties["ResourceID"].Value.ToString();

                if (tempCache.System.GetProperty("Obsolete") != 1)
                {
                    resourceID = tempCache.ResourceID;
                    break;
                }
            }

            return resourceID;
        }

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
            ManagedBy = "none";

            if (ManagedByAD != null)
            {
                string managerVal = ManagedByAD;

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelper.DistinguishedNameToUPN(managerVal);
                    ManagedBy = email;
                }
            }

            UsesOnedrive = OneDriveHelper.ComputerUsesOneDrive(ADcache);

            if (AdminPasswordExpirationTime != null)
            {
                ShowResultGetPassword = true;
            }
        }

        public void InitSCCMHW()
        {
            SCCMLD = TableGenerator.CreateVerticalTableFromDatabase(LogicalDisk,
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

            SCCMBIOS = TableGenerator.CreateVerticalTableFromDatabase(BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            SCCMVC = TableGenerator.CreateVerticalTableFromDatabase(VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            SCCMProcessor = TableGenerator.CreateVerticalTableFromDatabase(Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            SCCMDisk = TableGenerator.CreateVerticalTableFromDatabase(Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");
        }

        public void InitSCCMCollections()
        {
            string error = "";
            HTMLTableHelper groupTableHelper = new HTMLTableHelper(new string[] { "Collection Name" });
            var names = setConfig();

            if (names != null)
            {
                foreach (var name in names)
                {
                    groupTableHelper.AddRow(new string[] { name });
                }
            }
            else
            {
                error = "Computer not found i SCCM";
            }

            SCCMCollections = error + groupTableHelper.GetTable();
        }

        public void InitSCCMSoftware()
        {
            SCCMSoftware = TableGenerator.CreateTableFromDatabase(Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");            
        }

        public void InitRawSCCM()
        {
            string computerSCCM = TableGenerator.CreateRawFromDatabase(Computer, "Computer information not found");
            string systemSCCM = TableGenerator.CreateRawFromDatabase(System, "Computer not found i SCCM");
            SCCMRaw = $"<h3>Computer (Inventory)</h3>{computerSCCM}<h3>System (SCCM Info)</h3>{systemSCCM}";
        }

        public void InitSCCMInfo()
        {
            SCCMInfoSystem = TableGenerator.CreateVerticalTableFromDatabase(System, new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build" }, "Computer not found in SCCM");
            SCCMInfoComputer = TableGenerator.CreateVerticalTableFromDatabase(Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "Computer information not found");
        }
        #endregion loding data
    }
}
