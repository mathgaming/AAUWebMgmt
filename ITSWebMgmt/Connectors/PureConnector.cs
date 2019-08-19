using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ITSWebMgmt.Connectors
{
    public class PureConnector
    {
        private string APIkey = Startup.Configuration["PureApiKey"];
        string street = null;
        string building = null;

        public string Department { get; } = "";

        public string OfficeAddress
        {
            get
            {
                if (street != null) 
                {
                    if (building != null)
                        return street + " (" + building.Trim() + ")";
                    else
                        return street;
                }
                
                return "Address not found";
            }
        }

        public List<string> GetUsersByName(string name)
        {
            List<string> emails = new List<string>();
            DataObject dataObject = getRequest($"?q={name}&size=30&fields=staffOrganisationAssociations.person.names.value&fields=staffOrganisationAssociations.emails.value");

            if (dataObject != null)
            {
                foreach (var item in dataObject.items)
                {
                    if (item.staffOrganisationAssociations != null)
                    {
                        var user = item.staffOrganisationAssociations[0];
                        if (user?.emails != null && user?.person?.names != null)
                        {
                            emails.Add(user.person.names[0].value + " (" + user.emails[0].value + ")");
                        }
                    }
                }
            }

            return emails;
        }

        private DataObject getRequest(string urlParameters, string subSite = "")
        {
            string url = "https://vbn.aau.dk/ws/api/514/persons" + subSite;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("api-key", APIkey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            client.Dispose();

            DataObject dataObject = null;

            if (response.IsSuccessStatusCode)
            {
                dataObject = response.Content.ReadAsAsync<DataObject>().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return dataObject;
        }

        public PureConnector() { }

        public PureConnector(string empID)
        {
            DataObject dataObject = getRequest("?fields=staffOrganisationAssociations.addresses.street&fields=staffOrganisationAssociations.addresses.building&fields=staffOrganisationAssociations.organisationalUnit.names.value&locale=en_GB", "/" + empID);

            if (dataObject != null)
            {
                if (dataObject.staffOrganisationAssociations[0].organisationalUnit != null)
                {
                    Department = dataObject.staffOrganisationAssociations[0].organisationalUnit.names[0].value;
                }
                if (dataObject.staffOrganisationAssociations[0].addresses != null)
                {
                    street = dataObject.staffOrganisationAssociations[0].addresses[0].street;
                    building = dataObject.staffOrganisationAssociations[0].addresses[0].building;
                }
            }
        }
    }

    public class DataObject
    {
        public List<StaffOrganisationAssociations> staffOrganisationAssociations { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public List<StaffOrganisationAssociations> staffOrganisationAssociations { get; set; }
    }

    public class StaffOrganisationAssociations
    {
        public OrganisationalUnit organisationalUnit { get; set; }
        public List<Address> addresses { get; set; }
        public Person person { get; set; }
        public List<EMail> emails { get; set; }
    }

    public class Person
    {
        public List<Name> names { get; set; }
    }

    public class OrganisationalUnit
    {
        public List<Name> names { get; set; }
    }
    public class EMail
    {
        public string value { get; set; }
    }

    public class Name
    {
        public string value { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string building { get; set; }
    }
}