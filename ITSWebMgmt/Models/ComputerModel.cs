using System;
using System.Collections.Generic;
using System.Management;
using ITSWebMgmt.Caches;
using ITSWebMgmt.Helpers;

namespace ITSWebMgmt.Models
{
    public class ComputerModel
    {
        public List<TabModel> Tabs = new List<TabModel>();

        public MacComputerModel Mac;
        public WindowsComputerModel Windows;

        //Display
        public bool IsWindows { get; set; }
        public string ComputerName { get; set; } = "AAU115359";
        public string ErrorCountMessage { get; set; }
        public string ErrorMessages { get; set; }
        public string ResultError { get; set; }
        public bool ShowResultDiv { get; set; } = false;
        public bool ShowErrorDiv { get; set; } = false;
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
                Tabs.Add(new TabModel("sccmInfo", "SCCM Info"));
                Tabs.Add(new TabModel("sccmInventory", "Inventory"));
                Tabs.Add(new TabModel("sccmAV", "Antivirus"));
                Tabs.Add(new TabModel("sccmHW", "Hardware inventory"));
                Tabs.Add(new TabModel("rawdata", "Raw Data"));
                Tabs.Add(new TabModel("tasks", "Tasks"));
                Tabs.Add(new TabModel("warnings", "Warnings"));
            }
            else
            {
                Tabs.Add(new TabModel("macHW", "Hardware info"));
            }
        }
    }
}