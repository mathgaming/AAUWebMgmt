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

        public async Task<ØSSTableModel> LookUpByAAUNumberAsync(string id)
        {
            string assetNumber = await GetAssetNumberFromTagNumberAsync(id);
            return await GetØssTableAsync(assetNumber);
        }
        public async Task<List<string>> LookUpByEmployeeIDAsync(string id)
        {
            List<string> assetNumbers = await GetAssetNumbersFromEmployeeIDAsync(id);
            List<string> tagNumbers = new List<string>();
            foreach (var assetNumber in assetNumbers)
            {
                try
                {
                    tagNumbers.Add("AAU" + (await GetØSSInfoAsync(assetNumber)).TagNumber);
                }
                catch (Exception e)
                {
                    Console.WriteLine( e.Message);
                }
                
            }
            return tagNumbers;
        }

        public async Task<string> RunQueryAsync(string query, string outputKeyName)
        {
            string output = "";

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    output = reader[outputKeyName] as string;
                }
            }
            return output;
        }

        public async Task<List<string>> RunQueryMoreResultsAsync(string query, string outputKeyName)
        {
            List<string> output = new List<string>();

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    output.Add(reader[outputKeyName] as string);
                }
            }

            return output;
        }

        public async Task<List<string>> GetAssetNumbersFromEmployeeIDAsync(string id)
        {
            string query = $"select ASSET_NUMBER from FA_ASSET_DISTRIBUTION_V where EMPLOYEE_NUMBER like '{ id }'";
            return await RunQueryMoreResultsAsync(query, "ASSET_NUMBER");
        }

        public async Task<string> GetAssetNumberFromTagNumberAsync(string tagNumber)
        {
            if (tagNumber.Length <= 3)
            {
                return "";
            }
            tagNumber = tagNumber[3..];
            string query = $"select ASSET_NUMBER from FA_ADDITIONS_V where TAG_NUMBER like '{ tagNumber }'";
            return await RunQueryAsync(query, "ASSET_NUMBER");
        }

        public async Task<string> GetTagNumberFromAssetNumberAsync(string asssetNumber)
        {
            string query = $"select TAG_NUMBER from FA_ADDITIONS_V where ASSET_NUMBER like '{ asssetNumber }'";
            return await RunQueryAsync(query, "TAG_NUMBER");
        }

        public async Task<List<string>> GetAssetNumbersFromInvoiceNumberAsync(string number)
        {
            string query = $"select Distinct (ASSET_ID) from FA_ASSET_INVOICES_V where INVOICE_NUMBER like '{ number }'";

            List<string> output = new List<string>();

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = query;
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    output.Add((reader["ASSET_ID"] as long?).ToString());
                }
            }
            return output;
        }

        public async Task<string> GetAssetNumberFromInvoiceNumberAsync(string number)
        {
            return (await GetAssetNumbersFromInvoiceNumberAsync(number))[0];
        }

        public async Task<string> GetKeyFromSerialNumberAsync(string query, string serialNumber, string outputKeyName, bool tryAgain = true)
        {
            if (!tryAgain)
            {
                if (serialNumber[0] == 'S')
                {
                    serialNumber = serialNumber[1..];
                }
                else
                {
                    serialNumber = 'S' + serialNumber;
                }
            }

            string actualQuery = query + $" '{ serialNumber }'";
            string assetNumber = await RunQueryAsync(actualQuery, outputKeyName);

            if (assetNumber == "" && tryAgain)
            {
                return await GetKeyFromSerialNumberAsync(query, serialNumber, outputKeyName, false);
            }

            return assetNumber;
        }

        public async Task<string> GetAssetNumberFromSerialNumberAsync(string serialNumber)
        {
            string query = $"select ASSET_NUMBER from FA_ASSET_DISTRIBUTION_V where SERIAL_NUMBER like";
            return await GetKeyFromSerialNumberAsync(query, serialNumber, "ASSET_NUMBER");
        }

        public async Task<string> GetSegmentFromAssetNumberAsync(string assetNumber)
        {
            string query = $"select SEGMENT1 from FA_ADDITIONS_V "+
                            "join FA_ASSET_KEYWORDS on FA_ADDITIONS_V.ASSET_KEY_CCID = FA_ASSET_KEYWORDS.ASSET_KEY_CCID "+
                            $"where ASSET_NUMBER like '{ assetNumber }'";
            return await RunQueryAsync(query, "SEGMENT1");
        }

        public async Task<(string email, string first_name, string last_name)> GetResponsiblePersonAsync(string segment)
        {
            if (segment.Length == 0)
            {
                return ("", "", "");
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = "select EMAIL, FORNAVN, EFTERNAVN from PER_PERSON_ANALYSES " +
                                        "join PER_ANALYSIS_CRITERIA_KFV on PER_PERSON_ANALYSES.ANALYSIS_CRITERIA_ID = PER_ANALYSIS_CRITERIA_KFV.ANALYSIS_CRITERIA_ID " +
                                        "join AAU_HR_PERSON_IMPORT on AAU_HR_PERSON_IMPORT.PERSON_ID = PER_PERSON_ANALYSES.PERSON_ID " +
                                        $"where TIL_DATO IS NULL and PER_ANALYSIS_CRITERIA_KFV.SEGMENT1 <= '{segment}' and PER_ANALYSIS_CRITERIA_KFV.SEGMENT2 >= '{segment}'";

                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string email = reader["EMAIL"] as string;
                    string first_name = reader["FORNAVN"] as string;
                    string last_name = reader["EFTERNAVN"] as string;

                    return (email, first_name, last_name);
                }
            }

            return ("", "", "");
        }

        public async Task<TableModel> GetResponsiblePersonTableAsync(string segment)
        {
            TableModel table = new TableModel("No information found from ØSS", "Equipment manager");
            if (segment.Length == 0)
            {
                return table;
            }

            (string email, string first_name, string last_name) = await GetResponsiblePersonAsync(segment);


            if (email.Length != 0)
            {
                List<string[]> rows = new List<string[]> { new string[] { email, first_name, last_name } };
                table = new TableModel(new string[] { "Email", "First name", "Last name" }, rows, table.ViewHeading);
            }

            return table;
        }

        public async Task<ØSSInfo> GetØSSInfoAsync(string assetNumber)
        {
            if (assetNumber.Length == 0)
            {
                return new ØSSInfo();
            }

            ØSSInfo info = new ØSSInfo();
            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select IN_USE_FLAG, MANUFACTURER_NAME, MODEL_NUMBER, TAG_NUMBER from FA_ADDITIONS_V where ASSET_NUMBER like {assetNumber}";
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    info.InUseFlag = reader["In_USE_FLAG"] as string;
                    info.Manufacturer = reader["MANUFACTURER_NAME"] as string;
                    info.ModelNumber = reader["MODEL_NUMBER"] as string;
                    info.TagNumber = reader["TAG_NUMBER"] as string;
                }

                command.CommandText = $"select EMPLOYEE_NAME, EMPLOYEE_NUMBER, SERIAL_NUMBER, STATE from FA_ASSET_DISTRIBUTION_V where ASSET_NUMBER like {assetNumber}";
                reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    info.EmployeeName = reader["EMPLOYEE_NAME"] as string;
                    info.EmployeeNumber = reader["EMPLOYEE_NUMBER"] as string;
                    info.SerialNumber = reader["SERIAL_NUMBER"] as string;
                    info.State = reader["STATE"] as string;
                }
            }

            return info;
        }

        public async Task<(string OESSStatus, string OESSComment)> GetOESSStatusAsync(string assetNumber)
        {
            if (assetNumber.Length == 0)
            {
                return ("", "");
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select TRANSACTION_TYPE, COMMENTS from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber} order by DATE_EFFECTIVE desc fetch first 1 row only";
                var reader = await command.ExecuteReaderAsync();

                if(await reader.ReadAsync())
                {
                    return (reader["TRANSACTION_TYPE"] as string, reader["COMMENTS"] as string);
                }
            }

            return ("", "");
        }

        public async Task<bool> IsTrashedAsync(string assetNumber)
        {
            bool isTrashed = false;

            if (assetNumber.Length == 0)
            {
                return false;
            }

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select TRANSACTION_TYPE from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber}";
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
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

        public async Task<ØSSTableModel> GetØssTableAsync(string assetNumber)
        {
            ØSSTableModel tabels = new ØSSTableModel();
            string infoHeading = "ØSS info";
            string tranactionHeading = "Transaction Info";

            if (assetNumber.Length == 0)
            {
                tabels.InfoTable = new TableModel("No information found from ØSS", infoHeading);
                tabels.TransactionTable = new TableModel("No information found from ØSS", tranactionHeading);
                return tabels;
            }

            List<ØSSLine> lines = new List<ØSSLine>();
            ØSSInfo info = await GetØSSInfoAsync(assetNumber);

            using (OracleConnection conn = new OracleConnection(connectionString, credential))
            {
                await conn.OpenAsync();
                OracleCommand command = conn.CreateCommand();
                command.CommandText = $"select COMMENTS, DATE_EFFECTIVE, DESCRIPTION, TRANSACTION_DATE_ENTERED, TRANSACTION_TYPE from FA_TRANSACTION_HISTORY_TRX_V where ASSET_NUMBER like {assetNumber} order by DATE_EFFECTIVE asc";
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ØSSLine line = new ØSSLine
                    {
                        Comment = reader["COMMENTS"] as string,
                        DateEffective = reader["DATE_EFFECTIVE"] as DateTime?,
                        Description = reader["DESCRIPTION"] as string,
                        TransactionDateEntered = reader["TRANSACTION_DATE_ENTERED"] as DateTime?,
                        TransactionType = reader["TRANSACTION_TYPE"] as string
                    };

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


            if (rows.Count != 0)
            {
                tabels.InfoTable = new TableModel(new string[] { "IN_USE_FLAG", "MANUFACTURER_NAME", "MODEL_NUMBER", "TAG_NUMBER", "EMPLOYEE_NAME", "EMPLOYEE_NUMBER", "SERIAL_NUMBER", "STATE"}, row, infoHeading);
                tabels.TransactionTable = new TableModel(new string[] { "Comments", "Timestamp", "Description", "Transaction date", "Transaction type" }, rows, tranactionHeading);
            }

            return tabels;
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
