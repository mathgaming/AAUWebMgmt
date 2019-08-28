using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class MacComputerModel
    {
        public ComputerModel BaseModel { get; set; }
        public JamfConnector Jamf { get; set; } = new JamfConnector();
        public bool ComputerFound { get; set; } = false;
        public string HTMLForBasicInfo { get; set; }
        public string HTMLForGroups { get; set; }
        public string HTMLForSoftware { get; set; }
        public string HTMLForHardware { get; set; }
        public string HTMLForNetwork { get; set; }
        public MacComputerModel(ComputerModel baseModel)
        {
            BaseModel = baseModel;

            int id = Jamf.GetComputerIdByName(BaseModel.ComputerName);

            if (id != -1) //Computer found
            {
                ComputerFound = true;
                InitViews(id);
            }

            BaseModel.ComputerFound = ComputerFound;
        }

        public void InitViews(int id)
        {
            var jsonString = Jamf.GetAllComputerInformationAsJSONString(id);
            JObject jsonVal = JObject.Parse(jsonString) as JObject;

            dynamic computer = jsonVal.SelectToken("computer.hardware");
            HTMLForHardware = TableGenerator.CreateTableFromJamf(jsonVal, "computer.hardware", new List<string>() { "filevault2_users", "storage", "mapped_printers" });
        }
    }
}
