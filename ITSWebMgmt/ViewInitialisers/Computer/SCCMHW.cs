using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.Computer
{
    public static class SCCMHW
    {
        public static ComputerModel Init(ComputerModel Model)
        {
            Model.SCCMLD = TableGenerator.CreateVerticalTableFromDatabase(Model.LogicalDisk,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                new List<string>() { "DeviceID", "File system", "Size (GB)", "FreeSpace (GB)" },
                "Disk information not found");

            if (SCCM.HasValues(Model.RAM))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in Model.RAM) //Has one!
                {
                    total += int.Parse(o.Properties["Capacity"].Value.ToString()) / 1024;
                    count++;
                }

                Model.SCCMRAM = $"{total} GB RAM in {count} slot(s)";
            }
            else
            {
                Model.SCCMRAM = "RAM information not found";
            }

            Model.SCCMBIOS = TableGenerator.CreateVerticalTableFromDatabase(Model.BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            Model.SCCMVC = TableGenerator.CreateVerticalTableFromDatabase(Model.VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            Model.SCCMProcessor = TableGenerator.CreateVerticalTableFromDatabase(Model.Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            Model.SCCMDisk = TableGenerator.CreateVerticalTableFromDatabase(Model.Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");

            return Model;
        }
    }
}
