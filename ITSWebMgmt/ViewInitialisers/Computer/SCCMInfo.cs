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
            var names = setConfig(model.Collection, model);

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

        public static List<string> setConfig(ManagementObjectCollection Collection, ComputerModel ComputerModel)
        {
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
