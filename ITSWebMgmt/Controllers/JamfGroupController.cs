using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ITSWebMgmt.Controllers
{
    public class JamfGroupController : WebMgmtController
    {
        public JamfGroupController(LogEntryContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            var jamf = new JamfConnector();
            var response = jamf.SendGetReuest("advancedcomputersearches", "").Content;
            var list = response.ReadAsAsync<AdvancedComputerSearchList>().Result;

            return View(list);
        }

        public class Computer
        {
            public string Email_Address { get; set; }
            public string AAU_1x_Username { get; set; }
            public string Asset_Tag { get; set; }
            public string Serial_Number { get; set; }
            public string name { get; set; }
        }

        public class ComputerList
        {
            public List<Computer> computers { get; set; }
            public string name { get; set; }
        }

        public class AdvancedComputerSearchResult
        {
            public ComputerList advanced_computer_search { get; set; }
        }

        [HttpPost]
        public IActionResult GetEmailList([FromBody]int? id)
        {
            var jamf = new JamfConnector();

            var res = jamf.SendGetReuest("advancedcomputersearches/id/" + id, "").Content.ReadAsAsync<AdvancedComputerSearchResult>().Result.advanced_computer_search;
            var computers = res.computers;

            StringBuilder sb = new StringBuilder();

            sb.Append("Email;AAUNumber;Navn;ComputerNavn;Serial Number\n");

            foreach (var computer in computers)
            {
                string email = computer.Email_Address;
                if (email == "" || email == "Unknown")
                {
                    email = computer.AAU_1x_Username;
                }
                if (email != "" && email != "Unknown")
                {
                    try
                    {
                        var user = new UserModel(email, false);
                        string user_email = user.UserFound ? user.GetUserMails()[0] : "not found";
                        string name = user.UserFound ? user.DisplayName : "Not found";
                        string computerName = computer.name;
                        string sn = computer.Serial_Number;
                        sb.Append($"{user_email};{computer.Asset_Tag};{name};{computerName};{sn}\n");
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
            return Json(new { success = true, message = sb.ToString(), group = res.name});
        }
    }
}
