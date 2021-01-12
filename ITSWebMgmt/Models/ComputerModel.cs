using System.Collections.Generic;
using ITSWebMgmt.Connectors;

namespace ITSWebMgmt.Models
{
    public class ComputerModel
    {
        public List<TabModel> Tabs = new List<TabModel>();

        public MacComputerModel Mac;
        public WindowsComputerModel Windows;
        private string øSSAssetnumber;
        private string øSSSegment;
        private bool? isTrashedInØSS;

        //Display
        public bool IsWindows { get; set; }
        public bool IsInAD { get; set; }
        public bool OnlyFoundInOESS { get; set; }
        public string ComputerName { get; set; } = "AAU115359";
        public string ErrorCountMessage { get; set; }
        public string ErrorMessages { get; set; }
        public string ResultError { get; set; }
        public ØSSTableModel OESSTables { get; set; }
        public string ØSSAssetnumber
        {
            get
            {
                if (øSSAssetnumber == null)
                {
                    (string assetNumber, string segment) = GetAssetNumberAndSegment();
                    øSSAssetnumber = assetNumber;
                    øSSSegment = segment;
                }
                return øSSAssetnumber;
            }
            set => øSSAssetnumber = value;
        }

        public string ØSSSegment
        {
            get
            {
                if (øSSSegment == null)
                {
                    (string assetNumber, string segment) = GetAssetNumberAndSegment();
                    øSSAssetnumber = assetNumber;
                    øSSSegment = segment;
                }
                return øSSSegment;
            }
            set => øSSSegment = value;
        }
        public bool IsTrashedInØSS { get
            {
                if (isTrashedInØSS == null)
                {
                    isTrashedInØSS = new ØSSConnector().IsTrashed(ØSSAssetnumber);
                }

                return isTrashedInØSS == true;
            }
            set => isTrashedInØSS = value;
        }
        public TableModel OESSResponsiblePersonTable { get; set; }
        public virtual bool ComputerFound { get; set; }

        public ComputerModel(string computerName)
        {
            if (computerName != null)
            {
                ComputerName = computerName;
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

        private (string assetNumber, string segment) GetAssetNumberAndSegment(bool input_as_search = false)
        {
            ØSSConnector øss = new ØSSConnector();
            string assetNumber = "";
            string segment = "";

            if (input_as_search)
            {
                assetNumber = øss.GetAssetNumberFromTagNumber(ComputerName);
                if (assetNumber.Length != 0)
                {
                    segment = øss.GetSegmentFromAssetTag(ComputerName);
                }
                else
                {
                    assetNumber = øss.GetAssetNumberFromSerialNumber(ComputerName);
                    segment = øss.GetSegmentFromSerialNumber(ComputerName);
                }
            }
            else if (IsWindows)
            {
                assetNumber = øss.GetAssetNumberFromTagNumber(ComputerName);
                segment = øss.GetSegmentFromAssetTag(ComputerName);
            }
            else // Mac
            {
                assetNumber = øss.GetAssetNumberFromTagNumber(Mac.ComputerName);
                if (assetNumber.Length != 0)
                {
                    segment = øss.GetSegmentFromAssetTag(Mac.ComputerName);
                }
                else if (Mac.AssetTag.Length > 0)
                {
                    assetNumber = øss.GetAssetNumberFromTagNumber(Mac.AssetTag);
                    if (assetNumber.Length != 0)
                    {
                        segment = øss.GetSegmentFromAssetTag(Mac.AssetTag);
                    }
                }
                else
                {
                    assetNumber = øss.GetAssetNumberFromSerialNumber(Mac.SerialNumber);
                    segment = øss.GetSegmentFromSerialNumber(Mac.SerialNumber);
                }
            }

            return (assetNumber, segment);
        }

        public void InitØSSInfo(bool input_as_search = false)
        {
            (string assetNumber, string segment) = GetAssetNumberAndSegment(input_as_search);

            OESSTables = new ØSSConnector().GetØssTable(assetNumber);
            OESSResponsiblePersonTable = new ØSSConnector().GetResponsiblePersonTable(segment);
        }
    }
}