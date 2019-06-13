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
        string department = "";
        string street = "";
        string building = "";

        public string Department
        {
            get { return department; }
        }

        public string OfficeAddress
        {
            get
            {
                if (street != "")
                    return street + " (" + building.Trim() + ")";
                return "Address not found";
            }
        }

        public PureConnector(string empID)
        {
            string url = "https://vbn.aau.dk/ws/api/514/persons/" + empID;
            string urlParameters = "?fields=staffOrganisationAssociations.addresses.street&fields=staffOrganisationAssociations.addresses.building&fields=staffOrganisationAssociations.organisationalUnit.names.value&locale=en_GB";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("api-key", APIkey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            DataObject dataObject = null;

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                dataObject = response.Content.ReadAsAsync<DataObject>().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            client.Dispose();

            if (dataObject != null)
            {
                if (dataObject.staffOrganisationAssociations[0].organisationalUnit != null)
                {
                    department = dataObject.staffOrganisationAssociations[0].organisationalUnit.names[0].value;
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
    }

    public class StaffOrganisationAssociations
    {
        public OrganisationalUnit organisationalUnit { get; set; }
        public List<Address> addresses { get; set; }
    }

    public class OrganisationalUnit
    {
        public List<Name> names { get; set; }
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