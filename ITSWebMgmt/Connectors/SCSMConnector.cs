using ITSWebMgmt.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    //Before you venture into this file, make sure you have been blessed by a priest first. It has been created in the fires of hell and should just be nuked back to oblivion, but we still need it. So good luck.
    public class SCSMConnector
    {
        private const string webserviceURL = "https://service.aau.dk";
        public string userID = "";
        private static readonly string idForConvertedToSR = "d283d1f2-5660-d28e-f0a3-225f621394a9";

        public SCSMConnector()
        {
        }

        protected async Task<string> GetAuthKeyAsync()
        {
            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/Authorization/GetToken");
            request.Method = "POST";
            request.ContentType = "text/json";
            
            string domain = Startup.Configuration["cred:scsm:domain"];
            string username = Startup.Configuration["cred:scsm:username"];
            string secret = Startup.Configuration["cred:scsm:password"];

            if (domain == null || username == null || secret == null)
            {
                return null;
            }

            string json = "{\"Username\": \"" + domain + "\\\\" + username + "\",\"Password\": \"" + secret + "\",\"LanguageCode\": \"ENU\"}";

            var requestStream = new StreamWriter(request.GetRequestStream());
            requestStream.Write(json);
            requestStream.Flush();
            requestStream.Close();

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();

            var streamReader = new StreamReader(responseSteam);

            var responseText = await streamReader.ReadToEndAsync();

            return responseText.Replace("\"", "");
        }

        protected async Task<ServiceManagerModel> CreateServiceManagerAsync(string userId)
        {
            if (userId == null)
            {
                return new ServiceManagerModel(null, null);
            }
            return new ServiceManagerModel(userId, await LookupWorkItemsByUUIDAsync(userId));
        }

        //returns json string for uuid
        protected async Task<List<Case>> LookupWorkItemsByUUIDAsync(string uuid)
        {
            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/WorkItem/GetGridWorkItemsMyRequests?userid=" + uuid + "&showInactiveItems=true");
            request.Method = "Get";
            request.Headers.Add("Authorization", "Token " + await GetAuthKeyAsync());
            request.ContentType = "application/json; text/json";

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();
            var streamReader = new StreamReader(responseSteam);
            var responseText = await streamReader.ReadToEndAsync();

            //Make a breakpoint here to see what the response actually is.
            //This is probably easier than looking at the API documentation tbh.
            List<Case> caseList = JsonConvert.DeserializeObject<List<Case>>(responseText);

            return caseList;
        }

        //Takes a upn and retuns the users uuid
        protected async Task<string> GetUserUUIDByUPNAsync(string upn, List<string> emails)
        {
            //Get username from UPN
            WebRequest request = WebRequest.Create(webserviceURL + "/api/V3/User/GetUserList?fetchAll=false&userFilter=" + upn);
            request.Method = "Get";
            request.ContentType = "text/json";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("Authorization", "Token " + await GetAuthKeyAsync());

            var response = await request.GetResponseAsync();
            var responseSteam = response.GetResponseStream();
            var streamReader = new StreamReader(responseSteam);
            var responseText = await streamReader.ReadToEndAsync();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(responseText);

            //TODO: Don't await for each item, make all requests and await, then look over data
            foreach (dynamic obj in json)
            {
                if (emails.Contains((string)obj["Email"]))
                {
                    userID = (string)obj["Id"];
                    return userID;
                }
            }
            return null;
        }

        public async Task<ServiceManagerModel> GetServiceManagerAsync(string upn, List<string> emails)
        {
            string uuid = await GetUUIDAsync(upn, emails);
            
            return await CreateServiceManagerAsync(uuid);
        }

        public async Task<string> GetUUIDAsync(string upn, List<string> emails)
        {
            return await GetUserUUIDByUPNAsync(upn, emails);
        }
    }
}