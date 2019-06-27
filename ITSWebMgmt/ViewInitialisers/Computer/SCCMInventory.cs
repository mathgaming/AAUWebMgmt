using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.Computer
{
    public static class SCCMInventory
    {
        public static ComputerModel Init(ComputerModel model)
        {
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(model.Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "No inventory data");
            model.SSCMInventoryTable = tableAndList.Item1; //Table
            model.SCCMCollecionsSoftware = TableGenerator.CreateTableFromDatabase(model.Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");
            model.SCCMInventory += tableAndList.Item2; //List

            return model;
        }
    }
}
