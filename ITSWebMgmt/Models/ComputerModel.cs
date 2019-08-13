using System.Collections.Generic;
using ITSWebMgmt.Controllers;
using System.Management;
using System.Threading;
using ITSWebMgmt.Caches;
using ITSWebMgmt.WebMgmtErrors;
using ITSWebMgmt.Helpers;
using System;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

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
        public bool UsesOnedrive = false;

        public ComputerModel(string computerName)
        {
            if (computerName != null)
            {
                ADcache = new ComputerADcache(computerName);
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

        public void AddComputerToCollection(string collectionId)
        {
            try
            {
                // Add to All System collection.
                IResultObject collection = SCCM.cm.GetInstance($"SMS_Collection.collectionId='{collectionId}'");
                IResultObject collectionRule = SCCM.cm.CreateEmbeddedObjectInstance("SMS_CollectionRuleDirect");
                collectionRule["ResourceClassName"].StringValue = "SMS_R_System";
                collectionRule["ResourceID"].IntegerValue = int.Parse(SCCMcache.ResourceID);

                Dictionary<string, object> inParams2 = new Dictionary<string, object>();
                inParams2.Add("collectionRule", collectionRule);

                collection.ExecuteMethod("AddMembershipRule", inParams2);
            }
            catch (SmsException e)
            {
                Console.WriteLine("failed to add the computer" + e.Message);
                throw;
            }

            /*try
            {
                ManagementClass cls = new ManagementClass(SCCM.ms.Path.Path, "SMS_Client", null);
                ManagementBaseObject inParams = cls.GetMethodParameters("Add-CMDeviceCollectionDirectMembershipRule");
                inParams["CollectionId"] = collectionId;
                inParams["ResourceId"] = SCCMcache.ResourceID;
                ManagementBaseObject outMPParams = cls.InvokeMethod("Add-CMDeviceCollectionDirectMembershipRule", inParams, null);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to execute method", e);
            }*/
        }

        public List<string> setConfig()
        {
            ManagementObjectCollection Collection = this.Collection;
            ComputerModel ComputerModel = this;
            
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
    }
}