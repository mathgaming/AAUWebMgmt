using ITSWebMgmt.Helpers;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text;

namespace ITSWebMgmt.Connectors
{
    public class INDBConnector
    {
        public static string getInfo(string computerName)
        {
            var connection = tryConnect();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;

            IDbCommand command = conn.CreateCommand();
            command.CommandText = $"SELECT " +
                $"BESTILLINGS_DATO," +
                $"FABRIKAT," +
                $"MODELLEN," +
                $"MODTAGELSESDATO," +
                $"SERIENR," +
                $"SLUTBRUGER" +
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE UDSTYRS_REGISTRERINGS_NR LIKE {computerName.Substring(3)}";

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });
            List<string> tables = new List<string>();
            int results = 0;
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results++;
                    tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });
                    for (int i = 0; i < 6; i++)
                    {
                        if (reader.GetName(i) == "BESTILLINGS_DATO" || reader.GetName(i) == "MODTAGELSESDATO")
                        {
                            tableHelper.AddRow(new string[] { reader.GetName(i), DateTime.Parse(reader.GetValue(i).ToString()).ToString("yyyy-MM-dd") });
                        }
                        else
                        {
                            tableHelper.AddRow(new string[] { reader.GetName(i), reader.GetValue(i).ToString() });
                        }
                    }
                    tables.Add(tableHelper.GetTable());
                }
            }

            conn.Close();

            if (results == 0)
            {
                return "AAU number not found in purchase database";
            }
            else if (results > 1)
            {
                return "<h2>Multiple results found! All are listed below</h2>" + string.Join("<br/>", tables);
            }
            else
            {
                return tableHelper.GetTable();
            }
        }

        public static string LookupComputer(string computerName)
        {
            var connection = tryConnect();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;

            IDbCommand command = conn.CreateCommand();
            command.CommandText = $"SELECT " +
                $"FABRIKAT," +
                $"MODELLEN " +
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE UDSTYRS_REGISTRERINGS_NR LIKE {computerName.Substring(3)}";

            string manifacturer = "";
            string model = "";
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    manifacturer = reader.GetValue(0).ToString();
                    model = reader.GetValue(1).ToString();
                }
            }

            conn.Close();

            if (manifacturer.Length == 0)
            {
                return "Conputer not found";
            }
            else
            {
                return $"Computer has not been registrered in AD or Jamf, but was found in INDB. The manufacturer is {manifacturer} and the model is {model}";
            }
        }

        //Only used for test
        public static string getFullInfo(IDbConnection conn, string computerName)
        {
            IDbCommand command = conn.CreateCommand();
            command.CommandText = $"SELECT * FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE UDSTYRS_REGISTRERINGS_NR LIKE {computerName.Substring(3)}";

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });
            List<string> tables = new List<string>();
            
            int results = 0;
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results++;
                    tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });
                    for (int i = 0; i < 31; i++)
                    {
                        tableHelper.AddRow(new string[] { reader.GetName(i), reader.GetValue(i).ToString() });
                    }
                    tables.Add(tableHelper.GetTable());
                }
            }

            conn.Close();

            if (results == 0)
            {
                return "AAU number not found in purchase database";
            }
            else if (results > 1)
            {
                return "<h2>Multiple results found! All are listed below</h2>" + string.Join("<br/>", tables);
            }
            else
            {
                return tableHelper.GetTable();
            }
        }


        private static (IDbConnection conn, string error) tryConnect()
        {
            string username = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            if (username == null || password == null)
            {
                return (null, "Invalid creds for indkoeb");
            }

            IDbConnection conn = GetDatabaseConnection();

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                return (null, "Error connection to indkoeb database.");
            }

            return (conn, null);
        }

        private static IDbConnection GetDatabaseConnection()
        {
            string directoryServer = "sqlnet.adm.aau.dk:389";
            string defaultAdminContext = "dc=adm,dc=aau,dc=dk";
            string serviceName = "AAUF";
            string userId = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            return GetConnection(directoryServer, defaultAdminContext, serviceName, userId, password);
        }

        private static IDbConnection GetConnection(string directoryServer, string defaultAdminContext, string serviceName, string userId, string password)
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
            byte[] value = searchResult.Properties["orclnetdescstring"][0] as byte[];

            if (value != null)
            {
                string descriptor = Encoding.Default.GetString(value);
                return descriptor;
            }

            throw new Exception("Error querying LDAP");
        }
    }
}
