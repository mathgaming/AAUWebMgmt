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