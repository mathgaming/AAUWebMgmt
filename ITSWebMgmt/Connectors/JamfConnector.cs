using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ITSWebMgmt.Helpers;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using ITSWebMgmt.Models;
using System.Threading.Tasks;

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

        public async Task<HttpResponseMessage> SendGetReuestAsync(string url, string urlParameters)
        {
            url = "https://aaudk.jamfcloud.com/JSSResource/" + url;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url),
            };
            client.DefaultRequestHeaders.Add("Authorization", Auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(urlParameters);
            client.Dispose();

            return response;
        }

        public async Task<string> GetAllComputerInformationAsJSONStringAsync(int id)
        { 
            return await (await SendGetReuestAsync("computers/id/" + id, "?fiels=computer.general")).Content.ReadAsStringAsync();
        }

        public async Task<List<string>> GetComputerNamesForUserAsync(string user)
        {
            HttpResponseMessage response = await SendGetReuestAsync("computers/match/" + user, "");
            List<string> computerNames = new List<string>();

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                ComputerList computers = await response.Content.ReadAsAsync<ComputerList>();
                foreach (Computer computer in computers.computers)
                {
                    computerNames.Add(computer.asset_tag);
                }
            }

            return computerNames;
        }

        public async Task<List<string>> GetComputerNamesForUserWith1XAsync(string user)
        {
            List<string> computerNames = await GetComputerNamesForUserAsync(user);
            if (JamfDictionary == null)
            {
                _ = await GetJamfDictionaryAsync();
            }

            if (JamfDictionary.ContainsKey(user))
            {
                foreach (var id in JamfDictionary[user])
                {
                    HttpResponseMessage response = await SendGetReuestAsync($"computers/id/{id}/subset/General", "");

                    if (response.IsSuccessStatusCode)
                    {
                        response.Content.Headers.ContentType.MediaType = "application/json";
                        var result = await response.Content.ReadAsAsync<JObject>();
                        computerNames.Add(result.SelectToken("computer.general.name").ToString());
                    }
                }
            }

            return computerNames.Distinct().ToList();
        }

        public async Task<int> GetComputerIdByNameAsync(string name)
        {
            HttpResponseMessage response = await SendGetReuestAsync("computers/match/" + name, "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                ComputerList computers = await response.Content.ReadAsAsync<ComputerList>();
                if (computers.computers.Count != 0)
                {
                    return computers.computers[0].id;
                }
            }

            return -1;
        }

        public async Task<List<Computer>> GetAllComputersAsync()
        {
            HttpResponseMessage response = await SendGetReuestAsync("computers", "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                return (await response.Content.ReadAsAsync<ComputerList>()).computers;
            }

            return null;
        }

        public async Task<Dictionary<string, List<int>>> Get1xDictionatyAsync()
        {
            var d = new Dictionary<string, List<int>>();

            foreach (var computer in await GetAllComputersAsync())
            {
                HttpResponseMessage response = await SendGetReuestAsync($"computers/id/{computer.id}/subset/ExtensionAttributes", "");

                if (response.IsSuccessStatusCode)
                {
                    response.Content.Headers.ContentType.MediaType = "application/json";
                    var result = (await response.Content.ReadAsAsync<JObject>()).First.First.First.First;

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

        public async Task<HttpResponseMessage> SendUpdateReuestAsync(string url, string value)
        {
            url = "https://aaudk.jamfcloud.com/JSSResource/" + url;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url),
            };

            client.DefaultRequestHeaders.Add("Authorization", Auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);

            HttpContent content = new ByteArrayContent(bytes);

            HttpResponseMessage response = await client.PutAsync("", content);
            client.Dispose();

            return response;
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

        public async Task<Dictionary<string, List<int>>> GetJamfDictionaryAsync(bool updateCache = false)
        {
            string filename = @"jamf-aau1x.bin";
            if (File.Exists(filename) && !(updateCache && File.GetLastWriteTime(filename) > DateTime.Now.AddDays(-7)))
            {
                JamfDictionary = await ReadDictionaryAsync(filename);
            }
            else
            {
                JamfDictionary = await Get1xDictionatyAsync();
                SaveDictionary(JamfDictionary, filename);
            }

            return JamfDictionary;
        }

        private void SaveDictionary(Dictionary<string, List<int>> d, string path)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.Write(JsonSerializer.Serialize(d));
            }
        }

        private async Task<Dictionary<string, List<int>>> ReadDictionaryAsync(string path)
        {
            using (StreamReader file = new StreamReader(path, true))
            {
                string input = await file.ReadToEndAsync();
                return JsonSerializer.Deserialize<Dictionary<string, List<int>>>(input);
            }
        }
    }
}