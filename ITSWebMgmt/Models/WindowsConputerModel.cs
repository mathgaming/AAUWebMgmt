using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Models
{
    public class WindowsComputerModel : WebMgmtModel<ComputerADCache>
    {
        //SCCMCache
        public string LastLogonUserName { get => SCCMCache.LastLogonUserName(); }

        //ADCache
        public string ComputerName { get => BaseModel.ComputerName; }
        public string ComputerNameAD { get => ADCache.ComputerName; }
        public string Domain { get => ADCache.Domain; }
        public bool ComputerFound { get => ADCache.ComputerFound; set => ADCache.ComputerFound = value; }
        public string AdminPasswordExpirationTime { get => ADCache.GetProperty("ms-Mcs-AdmPwdExpirationTime"); }
        public string ManagedByAD { get => ADCache.GetProperty("managedBy"); set => ADCache.SaveProperty("managedBy", value); }
        public string DistinguishedName { get => ADCache.GetProperty("distinguishedName"); }
        public DateTime WhenCreated
        {
            get => DateTime.Parse(DateTimeConverter.Convert(SCCMCache.System().Result.GetProperty("CreationDate")));
        }

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
        public List<string> SCCMCollectionNames {get;set;}
        public bool ShowResultGetPassword { get; set; } = false;
        public bool ShowMoveComputerOUdiv { get; set; } = false;
        public bool UsesOnedrive { get; set; } = false;
        public async Task<bool> HasJavaAsync()
        {
            if (SCCMCache.ResourceID == "")
            {
                return false;
            }

            foreach (ManagementObject o in await SCCMCache.Software())
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
                ADCache = new ComputerADCache(ComputerName);
                if (ADCache.ComputerFound)
                {
                    baseModel.IsInAD = true;
                    BaseModel.ComputerName = ADCache.ComputerName;
                    BaseModel.ComputerFound = ADCache.ComputerFound;
                    baseModel.IsWindows = !(ADCache.GetProperty("operatingSystem").ToLower().Contains("mac os") || ADCache.GetProperty("operatingSystem").ToLower().Contains("macos"));
                    if (baseModel.IsWindows)
                    {
                        SCCMCache = new SCCMCache();
                        SCCMCache.ResourceID = GetSCCMResourceIDFromComputerName(ComputerNameAD);
                    }
                }
            }
        }

        public WindowsComputerModel(string computerName) : this(new ComputerModel(computerName, null)) { }

        public string GetSCCMResourceIDFromComputerName(string computername)
        {
            string resourceID = "";
            //XXX use ad path to get right object in sccm, also dont get obsolite
            foreach (ManagementObject o in SCCMCache.GetResourceIDFromComputerName(computername))
            {
                var tempCache = new SCCMCache
                {
                    ResourceID = o.Properties["ResourceID"].Value.ToString()
                };

                if (tempCache.System().Result.GetProperty("Obsolete") != 1)
                {
                    resourceID = tempCache.ResourceID;
                    break;
                }
            }

            return resourceID;
        }

        public async Task SetConfigAsync()
        {
            ManagementObjectCollection Collection = await SCCMCache.Collection();
            WindowsComputerModel ComputerModel = this;

            if (SCCM.HasValues(Collection))
            {
                foreach (ManagementObject o in Collection)
                {
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
                }
            }
        }

        public async Task SetSCCMCollectionNamesAsync()
        {
            ManagementObjectCollection Collection = await SCCMCache.Collection();

            if (SCCM.HasValues(Collection))
            {
                List<string> namesInCollection = new List<string>();
                foreach (ManagementObject o in Collection)
                {
                    var collectionID = o.Properties["CollectionID"].Value.ToString();

                    var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
                    ManagementPath path = new ManagementPath(pathString);
                    ManagementObject obj = new ManagementObject
                    {
                        Scope = SCCM.MS,
                        Path = path
                    };
                    obj.Get();

                    namesInCollection.Add(obj["Name"].ToString());
                }
                SCCMCollectionNames = namesInCollection;
            }
            else
            {
                SCCMCollectionNames = new List<string>();
            }
        }

        #region loading data
        public void InitBasicInfo()
        {
            //Managed By
            if (!string.IsNullOrWhiteSpace(ManagedByAD))
            {
                string email = ADHelper.DistinguishedNameToUPN(ManagedByAD);
                ManagedBy = new ManagedByModel(ADPath, HttpUtility.HtmlEncode("LDAP://" + ManagedByAD), email);
            }
            else
            {
                ManagedBy = new ManagedByModel(ADPath, "", "");
            }

            UsesOnedrive = OneDriveHelper.ComputerUsesOneDrive(ADCache);

            if (AdminPasswordExpirationTime != null)
            {
                ShowResultGetPassword = true;
            }
        }

        public async Task InitSCCMHWAsync()
        {
            await SCCMCache.LoadAllIntoCacheAsync();

            SCCMLD = CreateVerticalTableFromDatabase(await SCCMCache.LogicalDisk(),
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                "Disk information not found");

            if (SCCM.HasValues(await SCCMCache.RAM()))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in await SCCMCache.RAM()) //Has one!
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

            SCCMBIOS = CreateVerticalTableFromDatabase(await SCCMCache.BIOS(),
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            SCCMVC = CreateVerticalTableFromDatabase(await SCCMCache.VideoController(),
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            SCCMProcessor = CreateVerticalTableFromDatabase(await SCCMCache.Processor(),
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            SCCMDisk = CreateVerticalTableFromDatabase(await SCCMCache.Disk(),
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

        public async Task InitSCCMCollectionsAsync()
        {
            List<string[]> rows = new List<string[]>();
            if (SCCMCollectionNames == null)
            {
                await SetSCCMCollectionNamesAsync();
            }

            if (SCCMCollectionNames.Count > 0)
            {
                foreach (var name in SCCMCollectionNames)
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

        public async Task InitSCCMAVAsync()
        {
            SCCMAV = CreateTableFromDatabase(await SCCMCache.Antivirus(), new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" }, "Antivirus information not found", "Antivirus infomation");
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

        public async Task InitSCCMSoftwareAsync()
        {
            SCCMSoftware = CreateTableFromDatabase(await SCCMCache.Software(),
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found", "Software infomation");    
        }

        public async Task InitSCCMInfoAsync()
        {
            SCCMInfoSystem = CreateVerticalTableFromDatabase(await SCCMCache.System(), new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build" }, "Computer not found in SCCM");
            SCCMInfoComputer = CreateVerticalTableFromDatabase(await SCCMCache.Computer(), new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "Computer information not found");
        }
        #endregion loding data
    }
}
