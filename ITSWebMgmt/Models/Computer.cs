using System.Collections.Generic;
using ITSWebMgmt.Controllers;
using System.Management;
using System.Threading;
using ITSWebMgmt.Caches;
using ITSWebMgmt.WebMgmtErrors;

namespace ITSWebMgmt.Models
{
    public class ComputerModel : WebMgmtModel<ComputerADcache>
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
        public string ComputerNameAD { get => ADcache.ComputerName; }
        public string Domain { get => ADcache.Domain; }
        public bool ComputerFound { get => ADcache.ComputerFound; }
        public string AdminPasswordExpirationTime { get => ADcache.getProperty("ms-Mcs-AdmPwdExpirationTime"); }
        public string ManagedByAD { get => ADcache.getProperty("managedBy"); set => ADcache.saveProperty("managedBy", value); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }

        //Display
        public string ConfigPC = "Unknown";
        public string ConfigExtra = "False";
        public string ComputerName = "ITS\\AAU115359";
        public string ManagedBy;
        public string Raw;
        public string Result;
        public string PasswordExpireDate { get
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
        public string SSCMInventoryTable;
        public string SCCMCollecionsSoftware;
        public string SCCMInventory;
        public string SCCMComputers;
        public string SCCMCollectionsTable;
        public string SCCMCollections;
        public string SCCMAV;
        public string SCCMLD;
        public string SCCMRAM;
        public string SCCMBIOS;
        public string SCCMVC;
        public string SCCMProcessor;
        public string SCCMDisk;
        public string ErrorCountMessage;
        public string ErrorMessages;
        public string ResultError;
        public bool ShowResultDiv = false;
        public bool ShowResultGetPassword = false;
        public bool ShowMoveComputerOUdiv = false;
        public bool ShowErrorDiv = false;

        public ComputerModel(string username, string computername)
        {
            if (computername != null)
            {
                ADcache = new ComputerADcache(computername, username);
                if (ADcache.ComputerFound)
                {
                    SCCMcache = new SCCMcache();
                    SCCMcache.ResourceID = getSCCMResourceIDFromComputerName(ComputerNameAD);
                    ComputerName = ADcache.ComputerName;
                    LoadDataInbackground();
                }
                else
                {
                    ShowResultDiv = false;
                    ShowErrorDiv = true;
                    ResultError = "Not found";
                }
            }
        }

        public string getSCCMResourceIDFromComputerName(string computername)
        {
            string resourceID = "";
            //XXX use ad path to get right object in sccm, also dont get obsolite
            foreach (ManagementObject o in SCCMcache.getResourceIDFromComputerName(computername))
            {
                resourceID = o.Properties["ResourceID"].Value.ToString();
                break;
            }

            return resourceID;
        }
    }
}