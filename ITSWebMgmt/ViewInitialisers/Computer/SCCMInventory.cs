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
        public static ComputerModel Init(ComputerModel Model)
        {
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(Model.Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "No inventory data");
            Model.SSCMInventoryTable = tableAndList.Item1; //Table
            Model.SCCMCollecionsSoftware = TableGenerator.CreateTableFromDatabase(Model.Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");
            Model.SCCMInventory += tableAndList.Item2; //List

            return Model;
        }
    }
}
