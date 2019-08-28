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
        public string HTMLForLocalAccounts { get; set; }
        public string HTMLForDisk { get; set; }
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

            setHardware(jsonVal);
            setSoftware(jsonVal);
            setBasic(jsonVal);
            setNetwork(jsonVal);
            setGroups(jsonVal);
            setLocalAccounts(jsonVal);
            setDisk(jsonVal);
        }

        private void setHardware(JObject jsonVal)
        {
            HTMLForHardware = TableGenerator.CreateRawTableFromJamf(jsonVal, "computer.hardware", new List<string>() { "filevault2_users", "storage", "mapped_printers" }, true);
            //TODO Add disk info til hardware info
            //TODO consider printers
        }

        private void setSoftware(JObject jsonVal)
        {
            string applications = TableGenerator.CreateTableFromJamf(jsonVal, "computer.software.applications", new List<string>() { "name", "path", "version" }, new string[] { "Name", "Path", "Version" });
            string plugins = TableGenerator.CreateTableFromJamf(jsonVal, "computer.software.plugins", new List<string>() { "name", "path", "version" }, new string[] { "Name", "Path", "Version" });
            HTMLForSoftware = $"<h3>Applications</h3>{applications}<h3>Plugins</h3>{plugins}";
        }

        private void setNetwork(JObject jsonVal)
        {
            HTMLForNetwork = TableGenerator.CreateRawTableFromJamf(jsonVal, "computer.general", new List<string>() { "mac_address", "alt_mac_address", "ip_address", "last_reported_ip" });
        }

        private void setDisk(JObject jsonVal)
        {
            dynamic token = jsonVal.SelectToken("computer.hardware.storage");

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Disk", "Model", "Revision", "Name", "Size (MB)", "Percentage full", "Filevault status" });

            List<string> attributeNames = new List<string>() { "disk", "model", "revision" };
            List<string> partitionAttributeNames = new List<string>() { "name", "size", "percentage_full", "filevault_status" };

            foreach (dynamic info in token)
            {
                List<string> rowEntries = new List<string>();
                foreach (var name in attributeNames)
                {
                    rowEntries.Add(info.SelectToken(name).Value.ToString());
                }
                dynamic partition = info.SelectToken("partition");
                foreach (var name in partitionAttributeNames)
                {
                    rowEntries.Add(partition.SelectToken(name).Value.ToString());
                }

                tableHelper.AddRow(rowEntries.ToArray());
            }

            HTMLForDisk = tableHelper.GetTable();
        }

        private void setBasic(JObject jsonVal)
        {
            dynamic token = jsonVal.SelectToken("computer.location");

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Property name", "Value" });

            List<string> names = new List<string>() { "username", "real_name", "email_address", "position"};

            foreach (dynamic info in token)
            {
                if (names.Contains(info.Name))
                {
                    tableHelper.AddRow(new string[] { info.Name.Replace('_', ' '), info.Value.Value.ToString() });
                }
            }

            dynamic jamfVersion = jsonVal.SelectToken("computer.general.jamf_version");
            tableHelper.AddRow(new string[] { "Jamf version", jamfVersion.Value });

            token = jsonVal.SelectToken("computer.extension_attributes");

            foreach (dynamic info in token)
            {
                dynamic name = info.SelectToken("name");
                dynamic value = info.SelectToken("value");
                tableHelper.AddRow(new string[] { name.Value, value.Value });
            }

            HTMLForBasicInfo = tableHelper.GetTable();
        }

        private void setLocalAccounts(JObject jsonVal)
        {
            HTMLForLocalAccounts = TableGenerator.CreateTableFromJamf(jsonVal, "computer.groups_accounts.local_accounts", new List<string>() { "name", "realname", "uid", "home", "home_size", "administrator", "filevault_enabled" }, new string[] { "Name", "Real name", "uid", "Home directory", "Home size", "Administrator", "Filevault enabled" });
        }

        private void setGroups(JObject jsonVal)
        {
            JArray groups = jsonVal.SelectToken("computer.groups_accounts.computer_group_memberships").ToObject<JArray>();
            HTMLForGroups = $"{string.Join("<br />", groups)}";
        }
    }
}
