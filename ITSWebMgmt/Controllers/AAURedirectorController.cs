using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ITSWebMgmt.Controllers
{
    public class AAURedirectorController : Controller
    {
        public IActionResult Index(string id)
        {
            if (id == null)
            { 
                return View(new ChangeModel() { Error = "Type not found" });
            }

            string emailFilter = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

            if (Regex.IsMatch(id, emailFilter, RegexOptions.IgnoreCase))
            {
                Response.Redirect($"https://webmgmt.aau.dk/User?username={id}");
            }
            else if (id.StartsWith("IR", StringComparison.CurrentCultureIgnoreCase))
            {
                //Is incident
                Response.Redirect("https://service.aau.dk/Incident/Edit/" + id);
            }
            else if (id.StartsWith("SR", StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Redirect("https://service.aau.dk/ServiceRequest/Edit/" + id);
            }
            else if (id.StartsWith("PR", StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Redirect("https://service.aau.dk/Problem/Edit/" + id);
            }
            else if (id.StartsWith("C-", StringComparison.CurrentCultureIgnoreCase))
            {
                return View(GetChangeModel(id));
            }
            else if (id.StartsWith("EC-", StringComparison.CurrentCultureIgnoreCase))
            {
                return View(GetChangeModel(id));
            }
            else if (id.StartsWith("SC-", StringComparison.CurrentCultureIgnoreCase))
            {
                return View(GetChangeModel(id));
            }
            else if (id.StartsWith("AAU", StringComparison.CurrentCultureIgnoreCase))
            {
                Response.Redirect($"https://webmgmt.aau.dk/Computer?computername={id}");
            }

            return View(new ChangeModel(){ Error = "Type not found" });
        }

        private ChangeModel GetChangeModel(string id)
        {
            SqlConnection myConnection = new SqlConnection("Data Source = ad-sql2-misc.aau.dk; Initial Catalog = webmgmt; Integrated Security=SSPI;");
            try
            {
                myConnection.Open();
            }
            catch (SqlException)
            {
                return new ChangeModel()
                {
                    Error = "Access denied to change. Can someone confirm if this worked between 2019-01-01 and 2019-06-25?"
                };
            }
            
            string command = "SELECT TOP (1) [ChangeID] ,[Navn] ,[Beskrivelse] ,[Start] ,[Slut] ,[Ansvarlig] FROM[webmgmt].[dbo].[Changes] WHERE changeid like @ChangeID";

            SqlCommand readSQLCommand = new SqlCommand(command, myConnection);
            readSQLCommand.Parameters.AddWithValue("@ChangeID", id);
            var reader = readSQLCommand.ExecuteReader();
            reader.Read();

            ChangeModel model = new ChangeModel()
            {
                ChangeID = reader["ChangeID"].ToString(),
                Name = reader["Navn"].ToString(),
                Discription = reader["Beskrivelse"].ToString(),
                Start = reader["Start"].ToString(),
                End = reader["Slut"].ToString(),
                Resposeble = reader["Ansvarlig"].ToString(),
                Error = null
            };

            myConnection.Close();

            return model;
        }
    }
}
