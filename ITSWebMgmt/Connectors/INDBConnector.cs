using ITSWebMgmt.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Text;
using System.Threading.Tasks;

namespace ITSWebMgmt.Connectors
{
    public class INDBConnector
    {
        public static async Task<List<TableModel>> GetInfoAsync(string computerName)
        {
            if (await LookupComputerAsync(computerName) == "Computer not found")
            {
                return new List<TableModel> { new TableModel("Not found in purchase database") };
            }

            var connection = await TryConnectAsync();
            if (connection.conn == null)
                return new List<TableModel>{ new TableModel(connection.error)};

            var conn = connection.conn;

            string where = getWhere(computerName);

            var command = conn.CreateCommand();
            command.CommandText = $"SELECT " +
                $"BESTILLINGS_DATO," +
                $"FABRIKAT," +
                $"MODELLEN," +
                $"MODTAGELSESDATO," +
                $"SERIENR," +
                $"SLUTBRUGER" +
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE {where}";

            List<string[]> rows = new List<string[]>();
            List<TableModel> tables = new List<TableModel>();
            int results = 0;
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results++;
                    for (int i = 0; i < 6; i++)
                    {
                        if (reader.GetName(i) == "BESTILLINGS_DATO" || reader.GetName(i) == "MODTAGELSESDATO")
                        {
                            string value = reader.GetValue(i).ToString();
                            if (value == "")
                            {
                                rows.Add(new string[] { reader.GetName(i), "Date not found" });
                            }
                            else
                            {
                                rows.Add(new string[] { reader.GetName(i), DateTime.Parse(value).ToString("yyyy-MM-dd") });
                            }
                        }
                        else
                        {
                            rows.Add(new string[] { reader.GetName(i), reader.GetValue(i).ToString() });
                        }
                    }
                    tables.Add(new TableModel(new string[] { "Property", "Value" }, rows));
                    rows = new List<string[]>();
                }
            }

            conn.Close();

            if (tables.Count == 0)
            {
                return new List<TableModel> { new TableModel("AAU number not found in purchase database") };
            }
            else
            {
                return tables;
            }
        }

        private static string getWhere(string computerName)
        {
            string where = "";

            var computerNameDegits = computerName[3..];
            if (computerName.StartsWith("AAU") && int.TryParse(computerNameDegits, out _))
            {
                where = $"UDSTYRS_REGISTRERINGS_NR LIKE { computerName[3..]}";
            }
            else
            {
                if (!computerName.ToLower().StartsWith("s"))
                {
                    computerName = 'S' + computerName;
                }

                where = $"SERIENR like '{computerName}'";
            }

            return where;
        }

        public static async Task<string> LookupComputerAsync(string computerName)
        {
            var connection = await TryConnectAsync();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;
            string where = getWhere(computerName);

            var command = conn.CreateCommand();
            command.CommandText = $"SELECT " +
                $"FABRIKAT," +
                $"MODELLEN " +
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE {where}";

            string manifacturer = "";
            string model = "";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    manifacturer = reader.GetValue(0).ToString();
                    model = reader.GetValue(1).ToString();
                }
            }

            conn.Close();

            if (manifacturer.Length == 0)
            {
                return "Computer not found";
            }
            else
            {
                return $"Computer has not been registrered in AD or Jamf, but was found in INDB. The manufacturer is {manifacturer} and the model is {model}";
            }
        }

        private static async Task<(OracleConnection conn, string error)> TryConnectAsync()
        {
            string username = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            if (username == null || password == null)
            {
                return (null, "Invalid creds for indkoeb");
            }

            OracleConnection conn = GetDatabaseConnection();

            try
            {
                await conn.OpenAsync();
            }
            catch (Exception)
            {
                return (null, "Error connection to indkoeb database.");
            }

            return (conn, null);
        }

        private static OracleConnection GetDatabaseConnection()
        {
            string directoryServer = "sqlnet.adm.aau.dk:389";
            string defaultAdminContext = "dc=adm,dc=aau,dc=dk";
            string serviceName = "AAUF";
            string userId = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            return GetConnection(directoryServer, defaultAdminContext, serviceName, userId, password);
        }

        private static OracleConnection GetConnection(string directoryServer, string defaultAdminContext, string serviceName, string userId, string password)
        {
            string descriptor = ConnectionDescriptor(directoryServer, defaultAdminContext, serviceName);
            string connectionString = $"Data Source={descriptor}; User Id={userId}; Password={password};";
            OracleConnection con = new OracleConnection(connectionString);
            
            return con;
        }

        private static string ConnectionDescriptor(string directoryServer, string defaultAdminContext, string serviceName)
        {
            string ldapAdress = $"LDAP://{directoryServer}/{defaultAdminContext}";
            string query = $"(&(objectclass=orclNetService)(cn={serviceName}))";

            DirectoryEntry directoryEntry = new DirectoryEntry(ldapAdress, null, null, AuthenticationTypes.Anonymous);
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, query, new[] { "orclnetdescstring" }, SearchScope.Subtree);

            SearchResult searchResult = directorySearcher.FindOne();

            if (searchResult.Properties["orclnetdescstring"][0] is byte[] value)
            {
                string descriptor = Encoding.Default.GetString(value);
                return descriptor;
            }

            throw new Exception("Error querying LDAP");
        }
    }
}
