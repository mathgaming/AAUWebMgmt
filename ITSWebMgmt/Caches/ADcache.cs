using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;

namespace ITSWebMgmt.Caches
{
    public class Property
    {
        public string Name;
        public Type Type;
        public object Value;

        public Property(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    public abstract class ADCache
    {
        protected Dictionary<string, Property> Properties = new Dictionary<string, Property>();
        protected List<Property> PropertyNames;
        protected List<Property> PropertyToRefreshNames;
        public DirectoryEntry DE;
        public SearchResult result;
        public string Path { get => DE.Path; }
        public string ADPath;
        private readonly List<Type> types = new List<Type>();
        private List<PropertyValueCollection> AllProperties;
        private readonly Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> groupsTransitive = new Dictionary<string, List<string>>();
        private readonly object cacheLock = new object();
        public ADCache() { }

        public ADCache(string ADPath, List<Property> properties, List<Property> propertiesToRefresh)
        {
            this.ADPath = ADPath;
            PropertyNames = properties;
            PropertyToRefreshNames = propertiesToRefresh;
            DE = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);

            MakeCache();
        }

        public void MakeCache()
        {
            var search = new DirectorySearcher(DE);

            var tempProperties = PropertyNames;

            if (PropertyToRefreshNames != null)
            {
                List<string> propertiesNamesToRefresh = new List<string>();
                foreach (var p in PropertyToRefreshNames)
                {
                    propertiesNamesToRefresh.Add(p.Name);
                    tempProperties.Add(p);
                }

                DE.RefreshCache(propertiesNamesToRefresh.ToArray());
            }

            foreach (var p in tempProperties)
            {
                search.PropertiesToLoad.Add(p.Name);
            }

            result = search.FindOne();

            SaveCache(tempProperties, PropertyToRefreshNames);
        }

        protected void SaveCache(List<Property> properties, List<Property> propertiesToRefresh)
        {
            Properties = new Dictionary<string, Property>();

            foreach (var p in properties)
            {
                var value = DE.Properties[p.Name].Value;
                //TODO Handle all null values here and give then default values
                if (value == null)
                {
                    if (p.Type.Equals(typeof(bool)))
                        value = false;
                    else if (p.Type.Equals(typeof(int)))
                        value = -1;
                    else if (p.Type.Equals(typeof(string)))
                        value = "";
                    else if (p.Type.Equals(typeof(object[])))
                        value = new string[] { "" };
                }
                else
                {
                    //Print the type
                    Debug.WriteLine($"{p.Name}: {value.GetType()}, value: {value}");

                    //Handle special types
                    if (value.GetType().Equals(typeof(object[])))
                    {
                        value = ((object[])value).Cast<string>().ToArray();
                    }
                    if (value.GetType().ToString() == "System.__ComObject")
                    {
                        value = DateTimeConverter.Convert(value);
                    }
                }

                p.Value = value;
                AddProperty(p.Name, p);
            }
        }

        public List<PropertyValueCollection> GetAllProperties()
        {
            if (AllProperties == null)
            {
                AllProperties = new List<PropertyValueCollection>();
                foreach (string k in DE.Properties.PropertyNames)
                {
                    AllProperties.Add(DE.Properties[k]);
                }
            }
            return AllProperties;
        }

        public dynamic GetProperty(string property)
        {
            if (Properties.ContainsKey(property))
            {
                return Properties[property].Value;
            }

            return null;
        }

        public void SaveProperty(string property, dynamic value)
        {
            if (Properties[property].Type.Equals(value.GetType()))
            {
                DE.Properties[property].Value = value;
                DE.CommitChanges();
            }
            else
                Debug.WriteLine($"Not saved due to wrong type");
        }

        public void ClearProperty(string property)
        {
            DE.Properties[property].Clear();
            DE.CommitChanges();
        }

        public void AddProperty(string property, Property value)
        {
            Properties[property] = value;
        }

        public List<string> GetGroups(string name)
        {
            if (!groups.ContainsKey(name))
            {
                groups.Add(name, result.Properties[name].Cast<string>().ToList());
            }
            return groups[name];
        }

        public List<string> GetGroupsTransitive(string name)
        {
            string attName = $"msds-{name}Transitive";
            lock (cacheLock)
            {
                if (!groupsTransitive.ContainsKey(attName))
                {
                    DE.RefreshCache(attName.Split(','));
                    groupsTransitive.Add(attName, DE.Properties[attName].Cast<string>().ToList());
                }
            }
            return groupsTransitive[attName];
        }
    }
}