using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ITSWebMgmt.Helpers;
using Newtonsoft.Json.Linq;

namespace ITSWebMgmt.Connectors
{
    public class JamfConnector
    {
        public static string Auth { private get; set; }
        public static Dictionary<string, List<int>> JamfDictionary;

        public JamfConnector()
        {
            string user = Startup.Configuration["cred:jamf:username"];
            string pass = Startup.Configuration["cred:jamf:password"];
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(user + ":" + pass);
            string base64encodedusernpass = Convert.ToBase64String(plainTextBytes);
            Auth = "Basic " + base64encodedusernpass;
        }

        public HttpResponseMessage SendGetReuest(string url, string urlParameters)
        {
            url = "https://aaudk.jamfcloud.com/JSSResource/" + url;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url),
            };
            client.DefaultRequestHeaders.Add("Authorization", Auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            client.Dispose();

            return response;
        }

        public string GetAllComputerInformationAsJSONString(int id)
        { 
            return SendGetReuest("computers/id/" + id, "?fiels=computer.general").Content.ReadAsStringAsync().Result;
        }

        public List<string> GetComputerNamesForUser(string user)
        {
            HttpResponseMessage response = SendGetReuest("computers/match/" + user, "");
            List<string> computerNames = new List<string>();

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                ComputerList computers = response.Content.ReadAsAsync<ComputerList>().Result;
                foreach (Computer computer in computers.computers)
                {
                    computerNames.Add(computer.asset_tag);
                }
            }

            return computerNames;
        }

        public List<string> GetComputerNamesForUserWith1X(string user)
        {
            List<string> computerNames = GetComputerNamesForUser(user);
            if (JamfDictionary == null)
            {
                GetJamfDictionary();
            }

            if (JamfDictionary.ContainsKey(user))
            {
                foreach (var id in JamfDictionary[user])
                {
                    HttpResponseMessage response = SendGetReuest($"computers/id/{id}/subset/General", "");

                    if (response.IsSuccessStatusCode)
                    {
                        response.Content.Headers.ContentType.MediaType = "application/json";
                        var result = response.Content.ReadAsAsync<JObject>().Result;
                        computerNames.Add(result.SelectToken("computer.general.name").ToString());
                    }
                }
            }

            return computerNames.Distinct().ToList();
        }

        public int GetComputerIdByName(string name)
        {
            HttpResponseMessage response = SendGetReuest("computers/match/" + name, "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                ComputerList computers = response.Content.ReadAsAsync<ComputerList>().Result;
                if (computers.computers.Count != 0)
                {
                    return computers.computers[0].id;
                }
            }

            return -1;
        }

        public List<Computer> GetAllComputers()
        {
            HttpResponseMessage response = SendGetReuest("computers", "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                return response.Content.ReadAsAsync<ComputerList>().Result.computers;
            }

            return null;
        }

        public Dictionary<string, List<int>> Get1xDictionaty()
        {
            var d = new Dictionary<string, List<int>>();

            foreach (var computer in GetAllComputers())
            {
                HttpResponseMessage response = SendGetReuest($"computers/id/{computer.id}/subset/ExtensionAttributes", "");

                if (response.IsSuccessStatusCode)
                {
                    response.Content.Headers.ContentType.MediaType = "application/json";
                    var result = response.Content.ReadAsAsync<JObject>().Result.First.First.First.First;

                    string aau1x = "";

                    foreach (var att in result.Children())
                    {
                        if (att["id"].ToObject<int>() == 9)
                        {
                            aau1x = att["value"].ToString();
                        }
                    }

                    if (aau1x != "" && aau1x != "Unknown")
                    {
                        if (d.ContainsKey(aau1x))
                        {
                            d[aau1x].Add(computer.id);
                        }
                        else
                        {
                            d.Add(aau1x, new List<int> { computer.id });
                        }
                    }
                }
            }

            return d;
        }

#pragma warning disable IDE1006 // Naming Styles
        public class ComputerList
        {
            public List<Computer> computers { get; set; }
        }

        public class Computer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string asset_tag { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles

        public Dictionary<string, List<int>> GetJamfDictionary(bool updateCache = false)
        {
            string filename = @"jamf-aau1x.bin";
            if (File.Exists(filename) && !(updateCache && File.GetLastWriteTime(filename) > DateTime.Now.AddDays(-7)))
            {
                JamfDictionary = ReadDictionary(filename);
            }
            else
            {
                JamfDictionary = Get1xDictionaty();
                SaveDictionary(JamfDictionary, filename);
            }

            return JamfDictionary;
        }

        private void SaveDictionary(Dictionary<string, List<int>> d, string path)
        {
            var fi = new FileInfo(path);
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using var binaryFile = fi.Create();
            binaryFormatter.Serialize(binaryFile, d);
            binaryFile.Flush();
        }

        private Dictionary<string, List<int>> ReadDictionary(string path)
        {
            var fi = new FileInfo(path);
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using var binaryFile = fi.OpenRead();
            return (Dictionary<string, List<int>>)binaryFormatter.Deserialize(binaryFile);
        }
    }
}