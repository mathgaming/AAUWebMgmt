using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    public class NetaaudkConnector
    {
        public string Auth { get; }
        public NetaaudkConnector()
        {
            Auth = "Bearer " + Startup.Configuration["cred:netaaudk:token"];
        }

        public List<NetaaudkModel> GetData(string username)
        {
            string url = "https://net.aau.dk/api/owned-by";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", Auth);
            HttpResponseMessage response;

            using (var request = new HttpRequestMessage(new HttpMethod("PUT"), url))
            {
                request.Content = new StringContent("{\"user\": \"" + username + "\"}");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = client.SendAsync(request).Result;
            }
            client.Dispose();

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response.Content.ReadAsAsync<List<NetaaudkModel>>().Result;
        }

        public class NetaaudkModel
        {
            public string username { get; set; }
            public DateTime? created_at { get; set; }
            public DateTime? first_use { get; set; }
            public DateTime? last_used { get; set; }
            public string mac_address { get; set; }
            public string name { get; set; }
            public string devicetype { get; set; }
            public string id => username.Split('@')[0];
        }
    }
}
