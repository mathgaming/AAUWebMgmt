using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;

namespace ITSWebMgmt.ViewInitialisers.Computer
{
    public static class SCCMInfo
    {
        public static ComputerModel Init(ComputerModel model)
        {
            /*
             *     strQuery = "SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID="& computerID
                    for each fc in foundCollections
                       Set collection = SWbemServices.Get ("SMS_Collection.CollectionID=""" & fc.CollectionID &"""")
                       stringResult = stringResult & "<li> "  & collection.Name & "<br />"
                Next

             * SMS_Collection.CollectionID =
             *
             */

            //XXX: remeber to filter out computers that are obsolite in sccm (not active)
            string error = "";
            HTMLTableHelper groupTableHelper = new HTMLTableHelper(new string[] { "Collection Name" });
            var names = model.setConfig();

            if (names != null)
            {
                foreach (var name in names)
                {
                    groupTableHelper.AddRow(new string[] { name });
                }
            }
            else
            {
                error = "Computer not found i SCCM";
            }

            //Basal Info
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(model.System, new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build", "Config" }, "Computer not found i SCCM");

            model.SCCMComputers = error + groupTableHelper.GetTable();
            model.SCCMCollectionsTable = tableAndList.Item1; //Table
            model.SCCMCollections = tableAndList.Item2; //List

            return model;
        }
    }
}
