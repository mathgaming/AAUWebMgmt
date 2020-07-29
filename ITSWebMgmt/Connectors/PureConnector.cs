using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ITSWebMgmt.Connectors
{
    public class PureConnector
    {
        private readonly string APIkey = Startup.Configuration["PureApiKey"];
        private readonly string street = null;
        private readonly string building = null;

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

            if (name != null && name != "")
            {
                PureDataObject dataObject = GetRequest($"?q={name}&size=30&fields=staffOrganisationAssociations.person.name.text.value&fields=staffOrganisationAssociations.emails.value.value");

                if (dataObject != null)
                {
                    foreach (var item in dataObject.items)
                    {
                        if (item.staffOrganisationAssociations != null)
                        {
                            var user = item.staffOrganisationAssociations[0];
                            if (user?.emails != null && user?.person?.name != null)
                            {
                                emails.Add(user.person.name.text[0].value + " (" + user.emails[0].value.value + ")");
                            }
                        }
                    }
                }
            }

            return emails;
        }

        private PureDataObject GetRequest(string urlParameters, string subSite = "")
        {
            string url = "https://vbn.aau.dk/ws/api/518/persons" + subSite;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Add("api-key", APIkey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            client.Dispose();

            PureDataObject dataObject = null;

            if (response.IsSuccessStatusCode)
            {
                dataObject = response.Content.ReadAsAsync<PureDataObject>().Result;
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
            PureDataObject dataObject = GetRequest("?fields=staffOrganisationAssociations.addresses.street&fields=staffOrganisationAssociations.addresses.building&fields=staffOrganisationAssociations.organisationalUnit.name.text.value&locale=en_GB", "/" + empID);

            if (dataObject != null)
            {
                if (dataObject.staffOrganisationAssociations[0].organisationalUnit != null)
                {
                    Department = dataObject.staffOrganisationAssociations[0].organisationalUnit.name.text[0].value;
                }
                if (dataObject.staffOrganisationAssociations[0].addresses != null)
                {
                    street = dataObject.staffOrganisationAssociations[0].addresses[0].street;
                    building = dataObject.staffOrganisationAssociations[0].addresses[0].building;
                }
            }
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    public class PureDataObject
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
        public Name name { get; set; }
    }

    public class OrganisationalUnit
    {
        public Name name { get; set; }
    }
    public class EMail
    {
        public Value value { get; set; }
    }
    public class Value
    {
        public string value { get; set; }
    }

    public class Name
    {
        public List<Text> text { get; set; }
    }

    public class Text
    {
        public string value { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string building { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}