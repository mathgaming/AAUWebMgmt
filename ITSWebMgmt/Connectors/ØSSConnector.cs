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
            return GetTableFromQuery(id.Substring(3));
        }

        public TableModel LookUpByEmployeeID(string id)
        {
            string query = $"select * from webmgmt_assets where employee_number like '{id}'"+
                " and (LOWER(DESCRIPTION) like '%pc%' or LOWER(DESCRIPTION) like '%bærbar%' or LOWER(DESCRIPTION) like '%computer%' or LOWER(DESCRIPTION) like '%mac%')";
            return GetTableFromQuery(query);
        }

        private string getAssetNumberFromTagNumber(string tagNumber)
        {
            OracleCommand command = conn.CreateCommand();
            command.CommandText = $"select ASSET_NUMBER from FA_ADDITIONS_V where TAG_NUMBER like { tagNumber }";
            OracleDataReader reader = command.ExecuteReader();

            reader.Read();
            return reader["ASSET_NUMBER"] as string;
        }

        public TableModel GetTableFromQuery(string computerNumber)
        {
            Connect();
            OracleCommand command = conn.CreateCommand();
            string assetNumber = getAssetNumberFromTagNumber(computerNumber);
            if (assetNumber.Length == 0)
            {
                return new TableModel("No information found from ØSS"); 
            }

            command.CommandText =  $"select IN_USE_FLAG, MANUFACTURER_NAME, MODEL_NUMBER, TAG_NUMBER, " + /*fra FA_ADDITIONS_V*/
                                    "COMMENTS, DATE_EFFECTIVE, DESCRIPTION, TRANSACTION_DATE_ENTERED, TRANSACTION_TYPE, " + /*fra FA_TRANSACTION_HISTORY_TRX_V */
                                    "EMPLOYEE_NAME, EMPLOYEE_NUMBER, SERIAL_NUMBER, STATE " + /*fra FA_ADDITIONS_V*/
                                    "from FA_ADDITIONS_V " +
                                    "full join FA_TRANSACTION_HISTORY_TRX_V on FA_ADDITIONS_V.ASSET_NUMBER = FA_TRANSACTION_HISTORY_TRX_V.ASSET_NUMBER " +
                                    "full join FA_ASSET_DISTRIBUTION_V on FA_ADDITIONS_V.ASSET_NUMBER = FA_ASSET_DISTRIBUTION_V.ASSET_NUMBER " +
                                    $"where FA_ADDITIONS_V.ASSET_NUMBER like {assetNumber}";

            OracleDataReader reader = command.ExecuteReader();

            List<ØSSLine> lines = new List<ØSSLine>();

            while (reader.Read())
            {
                ØSSLine line = new ØSSLine();
                line.InUseFlag = reader["In_USE_FLAG"] as string;
                line.Manufacturer = reader["MANUFACTURER_NAME"] as string;
                line.ModelNumber = reader["MODEL_NUMBER"] as string;
                line.TagNumber = reader["TAG_NUMBER"] as string;
                line.Comment = reader["COMMENTS"] as string;
                line.DateEffective = reader["DATE_EFFECTIVE"] as DateTime?;
                line.Description = reader["DESCRIPTION"] as string;
                line.TransactionDateEntered = reader["TRANSACTION_DATE_ENTERED"] as DateTime?;
                line.TransactionType = reader["TRANSACTION_TYPE"] as string;
                line.EmployeeName = reader["EMPLOYEE_NAME"] as string;
                line.EmployeeNumber = reader["EMPLOYEE_NUMBER"] as string;
                line.SerialNumber = reader["SERIAL_NUMBER"] as string;
                line.State = reader["STATE"] as string;

                lines.Add(line);
            }

            Disconnect();

            List<string[]> rows = new List<string[]>();

            foreach (var line in lines)
            {
                rows.Add(new string[] { line.InUseFlag,
                                        line.Manufacturer,
                                        line.ModelNumber,
                                        line.TagNumber,
                                        line.Comment,
                                        DateTimeConverter.Convert(line.DateEffective),
                                        line.Description,
                                        DateTimeConverter.Convert(line.TransactionDateEntered),
                                        line.TransactionType,
                                        line.EmployeeName,
                                        line.EmployeeNumber,
                                        line.SerialNumber,
                                        line.State} );
            }

            TableModel table;
            if (rows.Count == 0)
            {
                table = new TableModel("No information found from ØSS");
            }
            else
            {
                table = new TableModel(new string[] { "IN_USE_FLAG", "MANUFACTURER_NAME", "MODEL_NUMBER", "TAG_NUMBER", "COMMENTS", "DATE_EFFECTIVE", "DESCRIPTION", "TRANSACTION_DATE_ENTERED", "TRANSACTION_TYPE", "EMPLOYEE_NAME", "EMPLOYEE_NUMBER", "SERIAL_NUMBER", "STATE" }, rows);
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
        public string InUseFlag { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string TagNumber { get; set; }
        public string Comment { get; set; }
        public DateTime? DateEffective { get; set; }
        public string Description { get; set; }
        public DateTime? TransactionDateEntered { get; set; }
        public string TransactionType { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string SerialNumber { get; set; }
        public string State { get; set; }
    }
}
