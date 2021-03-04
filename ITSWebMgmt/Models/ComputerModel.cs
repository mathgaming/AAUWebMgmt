using System.Collections.Generic;
using System.Linq;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Models.Log;

namespace ITSWebMgmt.Models
{
    public class ComputerModel
    {
        public List<TabModel> Tabs = new List<TabModel>();

        public MacComputerModel Mac;
        public WindowsComputerModel Windows;
        public string øSSAssetnumber;
        private string øSSSegment;
        private bool? isTrashedInØSS;

        //Display
        public bool IsWindows { get; set; }
        public bool IsInAD { get; set; }
        public bool OnlyFoundInOESS { get; set; }
        public MacCSVInfo MacCSVInfo { get; set; }
        public string ComputerName { get; set; } = "AAU115359";
        public string ErrorCountMessage { get; set; }
        public string ErrorMessages { get; set; }
        public string ResultError { get; set; }
        public ØSSTableModel OESSTables { get; set; }
        public string GetØSSAssetnumber(string input = "")
        {
            if (øSSAssetnumber == null)
            {
                øSSAssetnumber = GetAssetNumber(input);
            }
            return øSSAssetnumber;
        }

        public void SetØSSAssetnumber(string asssetNumber)
        {
            øSSAssetnumber = asssetNumber;
        }


        public string GetØSSSegment(string input = "")
        {
            if (øSSSegment == null)
            {
                øSSSegment = new ØSSConnector().GetSegmentFromAssetNumber(GetØSSAssetnumber(input));
            }
            return øSSSegment;
        }
        public bool IsTrashedInØSS { get
            {
                if (isTrashedInØSS == null)
                {
                    isTrashedInØSS = new ØSSConnector().IsTrashed(GetØSSAssetnumber());
                }

                return isTrashedInØSS == true;
            }
            set => isTrashedInØSS = value;
        }

        public TrashRequest TrashRequest { get; set; }

        public bool IsTrashedInWebMgmt()
        {
            return TrashRequest != null;
        }

        public TableModel OESSResponsiblePersonTable { get; set; }
        public virtual bool ComputerFound { get; set; }

        public ComputerModel(string computerName, TrashRequest request)
        {
            if (computerName != null)
            {
                ComputerName = computerName;
                TrashRequest = request;
            }
        }

        public void SetTabs()
        {
            if (IsWindows)
            {
                Tabs.Add(new TabModel("groups", "Groups"));
                Tabs.Add(new TabModel("sccminfo", "SCCM info"));
                Tabs.Add(new TabModel("sccmcollections", "SCCM Collections"));
                Tabs.Add(new TabModel("sccmInventory", "Software info"));
                Tabs.Add(new TabModel("sccmHW", "Hardware info"));
                Tabs.Add(new TabModel("sccmAV", "Antivirus"));
                Tabs.Add(new TabModel("purchase", "Purchase info (INDB)"));
                Tabs.Add(new TabModel("øss", "ØSS Info"));
                Tabs.Add(new TabModel("rawdata", "Raw data (AD)"));
                Tabs.Add(new TabModel("rawdatasccm", "Raw data (SCCM)"));
                Tabs.Add(new TabModel("tasks", "Tasks"));
                Tabs.Add(new TabModel("warnings", "Warnings"));
            }
            else
            {
                Tabs.Add(new TabModel("macgroups", "Groups"));
                Tabs.Add(new TabModel("macHW", "Hardware info"));
                Tabs.Add(new TabModel("macDisk", "Disk info"));
                Tabs.Add(new TabModel("macSW", "Software info"));
                Tabs.Add(new TabModel("macnetwork", "Network info"));
                Tabs.Add(new TabModel("macloaclaccounts", "Local accounts"));
                Tabs.Add(new TabModel("purchase", "Purchase info (INDB)"));
                Tabs.Add(new TabModel("øss", "ØSS Info"));
                Tabs.Add(new TabModel("warnings", "Warnings"));
                if (IsInAD)
                {
                    Tabs.Add(new TabModel("groups", "Groups (AD)"));
                    Tabs.Add(new TabModel("rawdata", "Raw data (AD)"));
                }
            }
        }

        private string GetAssetNumber(string input = "")
        {
            ØSSConnector øss = new ØSSConnector();
            string assetNumber = "";

            if (input != "")
            {
                assetNumber = øss.GetAssetNumberFromTagNumber(input);
                if (assetNumber.Length != 0)
                {
                    return assetNumber;
                }
                else
                {
                    return øss.GetAssetNumberFromSerialNumber(input);
                }
            }
            else if (IsWindows)
            {
                return øss.GetAssetNumberFromTagNumber(ComputerName);
            }
            else // Mac
            {
                assetNumber = øss.GetAssetNumberFromTagNumber(Mac.ComputerName);
                if (assetNumber.Length != 0)
                {
                    return assetNumber;
                }
                if (Mac.AssetTag.Length > 0)
                {
                    assetNumber = øss.GetAssetNumberFromTagNumber(Mac.AssetTag);
                    if (assetNumber.Length != 0)
                    {
                        return assetNumber;
                    }
                }

                return øss.GetAssetNumberFromSerialNumber(Mac.SerialNumber);
            }
        }

        public void InitØSSInfo(string input = "")
        {
            OESSTables = new ØSSConnector().GetØssTable(GetØSSAssetnumber(input));
            OESSResponsiblePersonTable = new ØSSConnector().GetResponsiblePersonTable(GetØSSSegment(input));
        }
    }
}