using ITSWebMgmt.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    public class ØSSConnector
    {
        private OracleConnection conn;

        public ØSSConnector()
        {
            Connect();
            Disconnect();
        }

        void Connect()
        {
            string user = Startup.Configuration["cred:jamf:username"];
            string pass = Startup.Configuration["cred:jamf:password"];
            string oradb = "Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = ora-oss-stdb.srv.aau.dk)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = OSSPSB)));"
                + $"User Id={user};Password={pass}; Connection Timeout=60;";

            conn = new OracleConnection(oradb);
            conn.Open();

            Console.WriteLine();
        }

        void LookupUser(string name) //UserModel.DisplayName
        {
            string[] parts = name.Split(" ");
            string formattedName = parts[parts.Length - 1] + ",";
            for (int i = 0; i < parts.Length - 1; i++)
            {
                formattedName += " " + parts[i];
            }

            string query = $"select * from webmgmt_assets where employee_name like '{formattedName}'";

            OracleCommand command = conn.CreateCommand();
            command.CommandText = query;

            OracleDataReader reader = command.ExecuteReader();

            List<ØSSLine> lines = new List<ØSSLine>();

            while (reader.Read())
            {
                ØSSLine line = new ØSSLine();
                line.TagNumber = reader["TAG_NUMBER"] as string;
                line.AssetNumber = reader["ASSET_NUMBER"] as string;
                line.Description = reader["DESCRIPTION"] as string;
                line.SerialNumber = reader["SERIAL_NUMBER"] as string;
                line.EmployeeName = reader["EMPLOYEE_NANE"] as string;
                line.EmployeeNUmber = reader["EMPLOYEE_NUMBER"] as string;

                lines.Add(line);
            }

            // do something with the lines
        }

        void Disconnect()
        {
            conn.Close();
            conn.Dispose();
            Console.Write("Disconnected");
        }
    }

    class ØSSLine
    {
        public string TagNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; }
        public string EmployeeName { get; set; }

        public string EmployeeNUmber { get; set; }
    }
}
