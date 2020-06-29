using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.PowerShell.Commands;
using Newtonsoft.Json;
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

        private HttpResponseMessage sendGetReuest(string url, string urlParameters)
        {
            url = "https://aaudk.jamfcloud.com/JSSResource/" + url;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Authorization", Auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            client.Dispose();

            return response;
        }

        public string GetAllComputerInformationAsJSONString(int id)
        { 
            return sendGetReuest("computers/id/" + id, "?fiels=computer.general").Content.ReadAsStringAsync().Result;
        }

        public List<string> GetComputerNamesForUser(string user)
        {
            HttpResponseMessage response = sendGetReuest("computers/match/" + user, "");
            List<string> computerNames = new List<string>();

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                Computers computers = response.Content.ReadAsAsync<Computers>().Result;
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
                    HttpResponseMessage response = sendGetReuest($"computers/id/{id}/subset/General", "");

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
            HttpResponseMessage response = sendGetReuest("computers/match/" + name, "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                Computers computers = response.Content.ReadAsAsync<Computers>().Result;
                if (computers.computers.Count != 0)
                {
                    return computers.computers[0].id;
                }
            }

            return -1;
        }

        public List<Computer> GetAllComputers()
        {
            HttpResponseMessage response = sendGetReuest("computers", "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                return response.Content.ReadAsAsync<Computers>().Result.computers;
            }

            return null;
        }

        public Dictionary<string, List<int>> Get1xDictionaty()
        {
            var d = new Dictionary<string, List<int>>();

            foreach (var computer in GetAllComputers())
            {
                HttpResponseMessage response = sendGetReuest($"computers/id/{computer.id}/subset/ExtensionAttributes", "");

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

        public class Computers
        {
            public List<Computer> computers { get; set; }
        }

        public class Computer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string asset_tag { get; set; }
        }

        public Dictionary<string, List<int>> GetJamfDictionary()
        {
            string filename = @"jamf-aau1x.bin";
            if (File.Exists(filename) && File.GetLastWriteTime(filename) > DateTime.Now.AddDays(-7))
            {
                JamfDictionary = readDictionary(filename);
            }
            else
            {
                JamfDictionary = Get1xDictionaty();
                saveDictionary(JamfDictionary, filename);
            }

            return JamfDictionary;
        }

        private void saveDictionary(Dictionary<string, List<int>> d, string path)
        {
            var fi = new FileInfo(path);
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using (var binaryFile = fi.Create())
            {
                binaryFormatter.Serialize(binaryFile, d);
                binaryFile.Flush();
            }
        }

        private Dictionary<string, List<int>> readDictionary(string path)
        {
            var fi = new FileInfo(path);
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using (var binaryFile = fi.OpenRead())
            {
                return (Dictionary<string, List<int>>)binaryFormatter.Deserialize(binaryFile);
            }
        }

        #region Samples from new Jamf API
        private void SampleRequestNewAPI()
        {
            string token = getToken();
            string url = "https://aaudk.jamfcloud.com/uapi/auth/current";
            string urlParameters = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsync(urlParameters, null).Result;
            var test = response.Content.ReadAsStringAsync();
        }

        //Used in the new API
        private string getToken()
        {
            string user = Startup.Configuration["cred:jamf:username"];
            string pass = Startup.Configuration["cred:jamf:password"];
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(user + ":" + pass);
            string base64encodedusernpass = Convert.ToBase64String(plainTextBytes);
            string url = "https://aaudk.jamfcloud.com/uapi/auth/tokens";
            string urlParameters = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64encodedusernpass);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.PostAsync(urlParameters, null).Result;
            client.Dispose();

            if (response.IsSuccessStatusCode)
            {
                Token dataObject = response.Content.ReadAsAsync<Token>().Result;

                return dataObject.token;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return null;
        }

        public class Token
        {
            public string token { get; set; }
            public string expires { get; set; }
        }

        #endregion Samples from new Jamf API
    }
}