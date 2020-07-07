using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace ITSWebMgmt.Connectors
{
    public class SplunkConnector
    {
        private readonly IMemoryCache _cache;
        public static string Auth { private get; set; }
        public SplunkConnector(IMemoryCache cache)
        {
            _cache = cache;
            string user = Startup.Configuration["cred:ad:username"];
            string pass = Startup.Configuration["cred:ad:password"];
            var plainTextBytes = Encoding.UTF8.GetBytes(user + ":" + pass);
            string base64encodedusernpass = Convert.ToBase64String(plainTextBytes);
            Auth = "Basic " + base64encodedusernpass;
        }

        public string IsAccountADFSLocked(string upn)
        {
            if (!_cache.TryGetValue("ADFSLockedAccounts", out List<string> lockedAccounts))
            {
                lockedAccounts = GetLockedAccounts();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
                _cache.Set("ADFSLockedAccounts", lockedAccounts, cacheEntryOptions);
            }

            if (lockedAccounts.Count == 0)
            {
                return "Data not found";
            }

            return lockedAccounts.Contains(upn).ToString();
        }

        private List<string> GetLockedAccounts()
        {
            try
            {
                var temp = GetData().Content.ReadAsStringAsync().Result; // This is done with a regex, becuase i could not find a NDJSON parser
                Regex regex = new Regex(@"sec_id"":""(?<email>[^ ]*) "".*""nBad_Password_Count"":""(?<count>[^ ]*) ");
                var entries = temp[0..^2].Split('\n');
                List<string> lockedAccouts = new List<string>();
                foreach (var entry in entries)
                {
                    Match match = regex.Match(entry);
                    if (match.Success)
                    {
                        if (int.Parse(match.Groups["count"].Value) >= 5)
                        {
                            // The account that match this might only be the accounts that actualy exist, but all is added to the list to be sure.
                        }
                        lockedAccouts.Add(match.Groups["email"].Value);
                    }
                }

                return lockedAccouts;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private HttpResponseMessage GetData()
        {
            string url = "https://splunk.aau.dk:8089/services/search/jobs/export?output_mode=json";
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response;

            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
            {
                request.Headers.TryAddWithoutValidation("Authorization", Auth);

                request.Content = new StringContent($"search={Uri.EscapeDataString(@"|loadjob savedsearch=SVC_SplunkREPORTS@srv.aau.dk:aau:aau_adfs_locked_out_d-1d")}", Encoding.UTF8, "application/x-www-form-urlencoded");

                response = client.SendAsync(request).Result;
            }
            client.Dispose();

            return response;
        }
    }
}
