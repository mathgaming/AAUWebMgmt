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
        private readonly SecureString s = new SecureString();
        private readonly OracleCredential credential;
        private readonly string connectionString = "Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = ora-oss-stdb.srv.aau.dk)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = OSSPSB)));Pooling = false;";

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

        public ØSSTableModel LookUpByAAUNumber(string id)
        {
            string assetNumber = GetAssetNumberFromTagNumber(id);
            return GetØssTable(assetNumber);
        }
        public List<string> LookUpByEmployeeID(string id)
        {
            List<string> assetNumbers = GetAssetNumbersFromEmployeeID(id);
            List<string> tagNumbers = new List<string>();
            foreach (var assetNumber in assetNumbers)
            {
                try
                {
                    tagNumbers.Add("AAU" + GetØSSInfo(assetNumber).TagNumber);
                }
                catch (Exception e)
                {
                    Console.WriteLine( e.Message);
                }
                
            }
            return tagNumbers;
        }

        public string RunQuery(string query, string outputKeyName)
        {
            string output = "";

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                OracleDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    output = reader[outputKeyName] as string;
                }
            }
            return output;
        }

        public List<string> RunQueryMoreResults(string query, string outputKeyName)
        {
            List<string> output = new List<string>();

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                OracleDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {
                    output.Add(reader[outputKeyName] as string);
                }
            }

            return output;
        }

        public List<string> GetAssetNumbersFromEmployeeID(string id)
        {
            string query = $"select ASSET_NUMBER from FA_ASSET_DISTRIBUTION_V where EMPLOYEE_NUMBER like '{ id }'";
            return RunQueryMoreResults(query, "ASSET_NUMBER");
        }

        public string GetAssetNumberFromTagNumber(string tagNumber)
        {
            if (tagNumber.Length <= 3)
            {
                return "";
            }
            tagNumber = tagNumber.Substring(3);
            string query = $"select ASSET_NUMBER from FA_ADDITIONS_V where TAG_NUMBER like '{ tagNumber }'";
            return RunQuery(query, "ASSET_NUMBER");
        }

        public string GetTagNumberFromAssetNumber(string asssetNumber)
        {
            string query = $"select TAG_NUMBER from FA_ADDITIONS_V where ASSET_NUMBER like '{ asssetNumber }'";
            return RunQuery(query, "TAG_NUMBER");
        }

        public List<string> GetAssetNumbersFromInvoiceNumber(string number)
        {
            string query = $"select Distinct (ASSET_ID) from FA_ASSET_INVOICES_V where INVOICE_NUMBER like '{ number }'";

            List<string> output = new List<string>();

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                OracleDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    output.Add((reader["ASSET_ID"] as long?).ToString());
                }
            }
            return output;
        }

        public string GetAssetNumberFromInvoiceNumber(string number)
        {
            return GetAssetNumbersFromInvoiceNumber(number)[0];
        }

        public string GetKeyFromSerialNumber(string query, string serialNumber, string outputKeyName, bool tryAgain = true)
        {
            if (!tryAgain)
            {
                if (serialNumber[0] == 'S')
                {
                    serialNumber = serialNumber.Substring(1);
                }
                else
                {
                    serialNumber = 'S' + serialNumber;
                }
            }

            string actualQuery = query + $" '{ serialNumber }'";
            string assetNumber = RunQuery(actualQuery, outputKeyName);

            if (assetNumber == "" && tryAgain)
            {
                return GetKeyFromSerialNumber(query, serialNumber, outputKeyName, false);
            }

            return assetNumber;
        }

        public string GetAssetNumberFromSerialNumber(string serialNumber)
        {
            string query = $"select ASSET_NUMBER from FA_ASSET_DISTRIBUTION_V where SERIAL_NUMBER like";
            return GetKeyFromSerialNumber(query, serialNumber, "ASSET_NUMBER");
        }

        public string GetSegmentFromAssetNumber(string assetNumber)
        {
            string query = $"select SEGMENT1 from FA_ADDITIONS_V "+
                            "join FA_ASSET_KEYWORDS on FA_ADDITIONS_V.ASSET_KEY_CCID = FA_ASSET_KEYWORDS.ASSET_KEY_CCID "+
                            $"where ASSET_NUMBER like '{ assetNumber }'";
            return RunQuery(query, "SEGMENT1");
        }

        public (string email, string first_name, string last_name) GetResponsiblePerson(string segment)
        {
            if (segment.Length == 0)
            {
                return ("", "", "");
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = "select EMAIL, FORNAVN, EFTERNAVN from PER_PERSON_ANALYSES " +
                                        "join PER_ANALYSIS_CRITERIA_KFV on PER_PERSON_ANALYSES.ANALYSIS_CRITERIA_ID = PER_ANALYSIS_CRITERIA_KFV.ANALYSIS_CRITERIA_ID " +
                                        "join AAU_HR_PERSON_IMPORT on AAU_HR_PERSON_IMPORT.PERSON_ID = PER_PERSON_ANALYSES.PERSON_ID " +
                                        $"where TIL_DATO IS NULL and PER_ANALYSIS_CRITERIA_KFV.SEGMENT1 <= '{segment}' and PER_ANALYSIS_CRITERIA_KFV.SEGMENT2 >= '{segment}'";

                OracleDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string email = reader["EMAIL"] as string;
                    string first_name = reader["FORNAVN"] as string;
                    string last_name = reader["EFTERNAVN"] as string;

                    return (email, first_name, last_name);
                }
            }

            return ("", "", "");
        }

        public TableModel GetResponsiblePersonTable(string segment)
        {
            if (segment.Length == 0)
            {
                return new TableModel("No information found from ØSS");
            }

            (string email, string first_name, string last_name) = GetResponsiblePerson(segment);

            TableModel table;
            if (email.Length == 0)
            {
                table = new TableModel("No information found from ØSS");
            }
            else
            {
                List<string[]> rows = new List<string[]> { new string[] { email, first_name, last_name } };
                table = new TableModel(new string[] { "Email", "First name", "Last name" }, rows);
            }

            table.ViewHeading = "Equipment manager";

            return table;
        }

        public ØSSInfo GetØSSInfo(string assetNumber)
        {
            if (assetNumber.Length == 0)
            {
                return new ØSSInfo();
            }

            ØSSInfo info = new ØSSInfo();
            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select IN_USE_FLAG, MANUFACTURER_NAME, MODEL_NUMBER, TAG_NUMBER from FA_ADDITIONS_V where ASSET_NUMBER like {assetNumber}";
                OracleDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    info.InUseFlag = reader["In_USE_FLAG"] as string;
                    info.Manufacturer = reader["MANUFACTURER_NAME"] as string;
                    info.ModelNumber = reader["MODEL_NUMBER"] as string;
                    info.TagNumber = reader["TAG_NUMBER"] as string;
                }

                command.CommandText = $"select EMPLOYEE_NAME, EMPLOYEE_NUMBER, SERIAL_NUMBER, STATE from FA_ASSET_DISTRIBUTION_V where ASSET_NUMBER like {assetNumber}";
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    info.EmployeeName = reader["EMPLOYEE_NAME"] as string;
                    info.EmployeeNumber = reader["EMPLOYEE_NUMBER"] as string;
                    info.SerialNumber = reader["SERIAL_NUMBER"] as string;
                    info.State = reader["STATE"] as string;
                }
            }

            return info;
        }

        public (string OESSStatus, string OESSComment) GetOESSStatus(string assetNumber)
        {
            if (assetNumber.Length == 0)
            {
                return ("", "");
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select TRANSACTION_TYPE, COMMENTS from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber} order by DATE_EFFECTIVE desc fetch first 1 row only";
                OracleDataReader reader = command.ExecuteReader();

                if(reader.Read())
                {
                    return (reader["TRANSACTION_TYPE"] as string, reader["COMMENTS"] as string);
                }
            }

            return ("", "");
        }

        public bool IsTrashed(string assetNumber)
        {
            bool isTrashed = false;

            if (assetNumber.Length == 0)
            {
                return false;
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select TRANSACTION_TYPE from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber}";
                OracleDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["TRANSACTION_TYPE"] as string == "Fuldstændig afgang")
                    {
                        isTrashed = true;
                        break;
                    }
                }
            }

            return isTrashed;
        }

        public ØSSTableModel GetØssTable(string assetNumber)
        {
            ØSSTableModel tables = new ØSSTableModel();

            if (assetNumber.Length == 0)
            {
                return tables;
            }

            List<ØSSLine> lines = new List<ØSSLine>();
            ØSSInfo info = GetØSSInfo(assetNumber);

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                conn.Open();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select COMMENTS, DATE_EFFECTIVE, DESCRIPTION, TRANSACTION_DATE_ENTERED, TRANSACTION_TYPE from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber} order by DATE_EFFECTIVE asc";
                OracleDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ØSSLine line = new ØSSLine();
                    line.Comment = reader["COMMENTS"] as string;
                    line.DateEffective = reader["DATE_EFFECTIVE"] as DateTime?;
                    line.Description = reader["DESCRIPTION"] as string;
                    line.TransactionDateEntered = reader["TRANSACTION_DATE_ENTERED"] as DateTime?;
                    line.TransactionType = reader["TRANSACTION_TYPE"] as string;

                    lines.Add(line);
                }
            }

            List<string[]> rows = new List<string[]>();

            foreach (var line in lines)
            {
                rows.Add(new string[] { line.Comment,
                                        DateTimeConverter.Convert(line.DateEffective),
                                        line.Description,
                                        DateTimeConverter.Convert(line.TransactionDateEntered),
                                        line.TransactionType} );
            }

            List<string[]> row = new List<string[]>  {new string[] {  
                info.InUseFlag,
                info.Manufacturer,
                info.ModelNumber,
                info.TagNumber,
                info.EmployeeName,
                info.EmployeeNumber,
                info.SerialNumber,
                info.State} };

            TableModel transactionTable;
            TableModel infoTable;
            if (rows.Count == 0)
            {
                transactionTable = new TableModel("No information found from ØSS");
                infoTable = new TableModel("No information found from ØSS");
            }
            else
            {
                infoTable = new TableModel(new string[] { "IN_USE_FLAG", "MANUFACTURER_NAME", "MODEL_NUMBER", "TAG_NUMBER", "EMPLOYEE_NAME", "EMPLOYEE_NUMBER", "SERIAL_NUMBER", "STATE"}, row);
                transactionTable = new TableModel(new string[] { "Comments", "Timestamp", "Description", "Transaction date", "Transaction type" }, rows);
            }

            infoTable.ViewHeading = "ØSS info";
            transactionTable.ViewHeading = "Transaction Info";

            tables.InfoTable = infoTable;
            tables.TransactionTable = transactionTable;

            return tables;
        }
    }

    public class ØSSInfo
    {
        public string InUseFlag { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string TagNumber { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public string SerialNumber { get; set; }
        public string State { get; set; }
    }

    class ØSSLine
    {
        public string Comment { get; set; }
        public DateTime? DateEffective { get; set; }
        public string Description { get; set; }
        public DateTime? TransactionDateEntered { get; set; }
        public string TransactionType { get; set; }
    }
}
