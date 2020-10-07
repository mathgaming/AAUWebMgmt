using System;
using System.Management;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace ITSWebMgmt.Helpers
{
    public static class SCCM
    {
        private static readonly string Username = Startup.Configuration["SCCMUsername"];
        private static readonly string Password = Startup.Configuration["SCCMPassword"];
        public static ManagementScope MS {get; set; }

        public static ManagementObjectCollection GetResults(WqlObjectQuery wqlq)
        {
            var searcher = new ManagementObjectSearcher(MS, wqlq);
            return searcher.Get();
        }

        public static bool HasValues(ManagementObjectCollection results)
        {
            try
            {
                if (results == null)
                {
                    return false;
                }

                var t2 = results.Count;
                return results.Count != 0;
            }
            catch (ManagementException) { }

            return false;
        }

        public static void Init()
        {
            MS = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1", GetConnectionOptions());
        }

        public static ConnectionOptions GetConnectionOptions()
        {
            if (Username == null)
            {
                Console.WriteLine();
            }
            ConnectionOptions con = new ConnectionOptions
            {
                Username = Username,
                Password = Password
            };
            return con;
        }

        public static bool AddComputerToCollection(string resourceID, string collectionId)
        {
            return RunScript($"Add-CMDeviceCollectionDirectMembershipRule -CollectionId {collectionId} -ResourceId {resourceID} -Force");
        }

        private static bool RunScript(string script)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                Arguments = @"cd $env:SMS_ADMIN_UI_PATH\..\;import-module .\ConfigurationManager.psd1;CD AA1:;" + script + ";cd C:"
            };
            //It is not possible to run a process as a diffent user. Therefore is the user set in IIS for the application pool for WebMgmt.

            Process p = Process.Start(psi);
            p.WaitForExit();
            string errOutput = p.StandardError.ReadToEnd();

            return !errOutput.ToLower().Contains("error");
        }

        public static dynamic GetProperty(this ManagementObjectCollection moc, string property)
        {
            return moc.OfType<ManagementObject>().FirstOrDefault()?.Properties[property]?.Value;
        }

        public static T GetPropertyAs<T>(this ManagementObjectCollection moc, string property)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T));
            var temp = GetPropertyAsString(moc, property);
            if (temp == "")
            {
                return default;
            }
            return (T)tc.ConvertFromInvariantString(temp);
        }

        public static int GetPropertyInGB(this ManagementObjectCollection moc, string property)
        {
            return GetPropertyAs<int>(moc, property) / 1024;
        }

        public static string GetPropertyAsString(this ManagementObjectCollection moc, string property)
        {
            if (moc == null)
            {
                return "Not found";
            }

            return GetPropertyAsString(moc.OfType<ManagementObject>().FirstOrDefault()?.Properties[property]);
        }

        public static string GetPropertyAsString(PropertyData property)
        {
            var value = property.Value;
            if (value != null)
            {
                if (value.GetType().Equals(typeof(string[])))
                {
                    return string.Join(", ", (string[])value);

                }
                else if (property.Type.ToString() == "DateTime")
                {
                    return DateTimeConverter.Convert(value.ToString());
                }
                else
                {
                    return value.ToString();
                }
            }

            return "not found";
        }
    }
}