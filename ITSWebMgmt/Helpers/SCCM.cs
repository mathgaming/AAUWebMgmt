using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using System;
using System.Management;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;

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
                Console.WriteLine();
            }
            ConnectionOptions con = new ConnectionOptions();
            con.Username = Username;
            con.Password = Password;
            return con;
        }


        private static void runScript(string script)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            psi.Arguments = @"cd $env:SMS_ADMIN_UI_PATH\..\;import-module .\ConfigurationManager.psd1;CD AA1:;Get-CMSite;" + script;
            Process p = Process.Start(psi);
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(strOutput);

            //The correct way of doing it, but it does not work with dotnet core 2.2, but might work in 3.0
            /* using (PowerShell PowerShellInstance = PowerShell.Create())
             {
                 PowerShellInstance.AddScript(
                     "Set-ExecutionPolicy RemoteSigned\n" +
                     //"[System.Reflection.Assembly]::LoadWithPartialName(\"System.Windows.Forms\")\n" +
                     @"cd ""C:\Program Files (x86)\Microsoft Configuration Manager\AdminConsole\bin""" + "\n" +
                     "import-module .\\ConfigurationManager.psd1\n" +
                     "CD AA1:\n" +
                     "Get-CMSite\n" +
                     "get-date\n");

                 Collection <PSObject> PSOutput = PowerShellInstance.Invoke();

                 Console.WriteLine(PowerShellInstance.Streams.Error);

                 foreach (var error in PowerShellInstance.Streams.Error)
                 {
                     Console.WriteLine(error.Exception.Message);
                 }

                 foreach (PSObject outputItem in PSOutput)
                 {
                     if (outputItem != null)
                     {
                         Console.WriteLine(outputItem);
                     }
                 }
             }*/
        }
    }
}