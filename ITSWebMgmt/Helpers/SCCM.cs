using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using System;
using System.Management;

namespace ITSWebMgmt.Helpers
{
    public class SCCM
    {
        private static string Username = Startup.Configuration["SCCMUsername"];
        private static string Password = Startup.Configuration["SCCMPassword"];
        public static ManagementScope ms {get; set; }
        public static WqlConnectionManager cm { get; set; }

        public static ManagementObjectCollection getResults(WqlObjectQuery wqlq)
        {
            var searcher = new ManagementObjectSearcher(ms, wqlq);
            return searcher.Get();
        }

        public static bool HasValues(ManagementObjectCollection results)
        {
            try
            {
                var t2 = results.Count;
                return results.Count != 0;
            }
            catch (ManagementException) { }

            return false;
        }

        public static void Init()
        {
            ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1", GetConnectionOptions());
            Connect();
        }

        private static void Connect()
        {
            try
            {
                SmsNamedValuesDictionary namedValues = new SmsNamedValuesDictionary();
                WqlConnectionManager connection = new WqlConnectionManager(namedValues);

                connection.Connect("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1", Username, Password);

                cm = connection;
            }
            catch (SmsException e)
            {
                Console.WriteLine("Failed to Connect. Error: " + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Failed to authenticate. Error:" + e.Message);
            }
        }

        public static ConnectionOptions GetConnectionOptions()
        {
            if (Username == null)
            {
                System.Console.WriteLine();
            }
            ConnectionOptions con = new ConnectionOptions();
            con.Username = Username;
            con.Password = Password;
            return con;
        }
    }
}