using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITSWebMgmt.Connectors
{
    public class JamfConnector
    {
        public static string Auth { private get; set; }

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

        public int GetComputerIdByName(string name)
        {
            HttpResponseMessage response = sendGetReuest("computers/match/" + name, "");

            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType.MediaType = "application/json";
                Computers computers = response.Content.ReadAsAsync<Computers>().Result;
                return computers.computers[0].id;
            }

            return -1;
        }

        public class Computers
        {
            public List<Computer> computers { get; set; }
        }

        public class Computer
        {
            public int id { get; set; }
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