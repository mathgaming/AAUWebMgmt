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
        public static ComputerModel Init(ComputerModel model)
        {
            model.SCCMLD = TableGenerator.CreateVerticalTableFromDatabase(model.LogicalDisk,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                new List<string>() { "DeviceID", "File system", "Size (GB)", "FreeSpace (GB)" },
                "Disk information not found");

            if (SCCM.HasValues(model.RAM))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in model.RAM) //Has one!
                {
                    total += int.Parse(o.Properties["Capacity"].Value.ToString()) / 1024;
                    count++;
                }

                model.SCCMRAM = $"{total} GB RAM in {count} slot(s)";
            }
            else
            {
                model.SCCMRAM = "RAM information not found";
            }

            model.SCCMBIOS = TableGenerator.CreateVerticalTableFromDatabase(model.BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            model.SCCMVC = TableGenerator.CreateVerticalTableFromDatabase(model.VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            model.SCCMProcessor = TableGenerator.CreateVerticalTableFromDatabase(model.Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            model.SCCMDisk = TableGenerator.CreateVerticalTableFromDatabase(model.Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");

            return model;
        }
    }
}
