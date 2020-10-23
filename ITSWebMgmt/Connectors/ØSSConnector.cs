using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    public class ØSSConnector
    {
        private OracleConnection conn;
        private readonly SecureString s = new SecureString();
        private readonly OracleCredential credential;

        public ØSSConnector()
        {
            string user = Startup.Configuration["cred:oss:username"];
            string pass = Startup.Configuration["cred:oss:password"];

            foreach (var c in pass)
            {
                s.AppendChar(c);
            }

            s.MakeReadOnly();

            credential = new OracleCredential(user, s);
        }

        private void Connect()
        {
            string oradb = "Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = ora-oss-stdb.srv.aau.dk)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = OSSPSB)));";

            conn = new OracleConnection(oradb, credential);
            conn.Open();

            Console.WriteLine();
        }

        public TableModel LookUpByAAUNumber(string id)
        {
            id = id.Substring(3);
            string query = $"select * from webmgmt_assets where tag_number like '{id}'";
            return GetTableFromQuery(query);
        }

        public TableModel LookUpByEmployeeID(string id)
        {
            string query = $"select * from webmgmt_assets where employee_number like '{id}'"+
                " and (LOWER(DESCRIPTION) like '%pc%' or LOWER(DESCRIPTION) like '%bærbar%' or LOWER(DESCRIPTION) like '%computer%' or LOWER(DESCRIPTION) like '%mac%')";
            return GetTableFromQuery(query);
        }

        public TableModel GetTableFromQuery(string query)
        {
            Connect();
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
                line.EmployeeName = reader["EMPLOYEE_NAME"] as string;
                line.EmployeeNumber = reader["EMPLOYEE_NUMBER"] as string;
                line.TransactionType = reader["TRANSACTION_TYPE_CODE"] as string;
                line.LastUpdateDate = reader["LAST_UPDATE_DATE"] as DateTime?;

                lines.Add(line);
            }

            Disconnect();

            List<string[]> rows = new List<string[]>();

            foreach (var line in lines)
            {
                rows.Add(new string[] { line.TagNumber, line.Description, line.EmployeeName, line.TransactionType, DateTimeConverter.Convert(line.LastUpdateDate) });
            }

            TableModel table;
            if (rows.Count == 0)
            {
                table = new TableModel("No information found from ØSS");
            }
            else
            {
                table = new TableModel(new string[] { "Tag/AAU number", "Description", "Employee name", "Transaction type", "Last update date" }, rows);
            }

            table.ViewHeading = "ØSS info";

            return table;
        }

        private void Disconnect()
        {
            conn.Close();
            conn.Dispose();
        }
    }

    class ØSSLine
    {
        public string TagNumber { get; set; }
        public string AssetNumber { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string TransactionType { get; set; }
        public DateTime? LastUpdateDate { get; set; } 
    }
}
