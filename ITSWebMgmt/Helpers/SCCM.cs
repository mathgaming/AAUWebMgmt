using System;
using System.Management;
using System.Diagnostics;
using System.Security;
using System.IO;

namespace ITSWebMgmt.Helpers
{
    public class SCCM
    {
        private static string Username = Startup.Configuration["SCCMUsername"];
        private static string Password = Startup.Configuration["SCCMPassword"];
        public static ManagementScope ms {get; set; }

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
        }

        public static ConnectionOptions GetConnectionOptions()
        {
            if (Username == null)
            {
                Console.WriteLine();
            }
            ConnectionOptions con = new ConnectionOptions();
            con.Username = Username;
            con.Password = Password;
            return con;
        }

        public static bool AddComputerToCollection(string resourceID, string collectionId)
        {
            return runScript($"Add-CMDeviceCollectionDirectMembershipRule -CollectionId {collectionId} -ResourceId {resourceID} -Force");
        }

        private static bool runScript(string script)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            SecureString password = new SecureString();
            psi.FileName = "powershell";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            psi.Arguments = @"cd $env:SMS_ADMIN_UI_PATH\..\;import-module .\ConfigurationManager.psd1;CD AA1:;" + script + ";cd C:";
            //It is not possible to run a process as a diffent user. Therefore is the user set in IIS for the application pool for WebMGmt.
            /*foreach (char c in Password)
            {
                password.AppendChar(c);
            }
            psi.Password = password;
            psi.UserName = Username;*/

            Process p = Process.Start(psi);
            p.WaitForExit();
            string strOutput = p.StandardOutput.ReadToEnd();
            string errOutput = p.StandardError.ReadToEnd();

            return errOutput.Length == 0;
        }
    }
}