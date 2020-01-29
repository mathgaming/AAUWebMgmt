using ITSWebMgmt.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Text;
using System.Web;

namespace ITSWebMgmt.Helpers
{
    public class TableGenerator
    {
        public static string getPersonLinkFromADPath(string adpath)
        {
            if (adpath == "")
            {
                return "Not found";
            }
            var split = adpath.Split(',');
            var name = split[0].Replace("CN=", "");
            var domain = split.Where<string>(s => s.StartsWith("DC=")).ToArray<string>()[0].Replace("DC=", "");

            return string.Format("<a href=\"/User?username={0}%40{1}.aau.dk\">{0}@{1}.aau.dk</a><br/>", name, domain);
        }

        public static string buildRawTable(List<PropertyValueCollection> properties)
        {
            //builder.Append((result.Properties["aauStaffID"][0])).ToString();
            var builder = new StringBuilder();

            builder.Append("<table class=\"ui celled structured table\"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>");

            foreach (PropertyValueCollection a in properties)
            {
                builder.Append("<tr>");

                //Don't display admin password in raw data
                if (a != null && a.Count > 0 && a.PropertyName != "ms-Mcs-AdmPwd")
                {
                    builder.Append("<td rowspan=\"" + a.Count + "\">" + a.PropertyName + "</td>");
                    
                    string v = convertToStringWithCorrectFormatIfDate(a[0]);

                    if (a.Count == 1)
                    {
                        builder.Append("<td>" + v + "</td></tr>");
                    }
                    else
                    {
                        builder.Append("<td>" + v + "</td>");
                        for (int i = 1; i < a.Count; i++)
                        {
                            v = convertToStringWithCorrectFormatIfDate(a[i]);
                            builder.Append("<tr><td>" + v + "</td></tr>");
                        }
                    }
                }
                else
                {
                    builder.Append("<td></td></tr>");
                }
            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }

        public static string splitStingOnCommaToHtml(string v)
        {
            if (v != null && v.Length > 20)
            {
                return string.Join(",<br />", v.Split(","));
            }

            return v;
        }

        public static string convertToStringWithCorrectFormatIfDate(dynamic v)
        {
            if (v.GetType().Equals(typeof(DateTime)))
            {
                return DateTimeConverter.Convert((DateTime)v);
            }
            else if (v.GetType().ToString() == "System.__ComObject")
            {
                try
                {
                    int test = (Int32)v.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, v, null);
                    return DateTimeConverter.Convert(v);
                }
                catch (Exception) { }
            }
            return v.ToString();
        }

        public static string CreateRawFromDatabase(ManagementObjectCollection results, string errorMessage)
        {
            var builder = new StringBuilder();
            if (SCCM.HasValues(results))
            {
                builder.Append("<table class=\"ui celled structured table\"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>");
                foreach (ManagementObject o in results)
                {
                    foreach (var property in o.Properties)
                    {
                        string key = property.Name;
                        object value = property.Value;
                        int i = 0;
                        string[] arry = null;

                        if (value != null && value.GetType().IsArray)
                        {
                            if (value is string[])
                            {
                                arry = (string[])value;
                                if (arry.Length > 0)
                                {
                                    builder.Append("<tr><td rowspan=\"" + arry.Length + "\">" + key + "</td>");
                                }
                                else
                                {
                                    builder.Append("<tr><td rowspan=\"" + 1 + "\">" + key + "</td>");
                                    builder.Append("<td></td></tr>");
                                }
                            }
                            else
                            {
                                arry = new string[] { "none-string value" }; //XXX get the byte value
                            }
                            if (arry.Length > 0)
                            {
                                builder.Append("<td>" + arry[0] + "</td>");
                                foreach (string f in arry.Skip(1))
                                {
                                    builder.Append("<tr><td>" + f + "</td></tr>");
                                }
                            }
                        }
                        else
                        {
                            builder.Append("<tr><td rowspan=\"" + 1 + "\">" + key + "</td>");
                            builder.Append("<td>" + value + "</td></tr>");
                        }

                    }

                }
            }
            else
            {
                return errorMessage;
            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }
    }
}