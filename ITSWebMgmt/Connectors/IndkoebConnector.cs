using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text;

namespace ITSWebMgmt.Connectors
{
    public class IndkoebConnector
    {
        public static string getInfo(string computerName)
        {
            string username = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            if (username == null || password == null)
            {
                return "Invalid creds for indkoeb";
            }

            Test();

            /*string directoryServer = "sqlnet.adm.aau.dk:389,sqlnet2.adm.aau.dk:389";
            string defaultAdminContext = "dc=adm,dc=aau,dc=dk";
            string oracleHostEntryPath = string.Format("LDAP://{0}/,{1}", directoryServer, defaultAdminContext);

            var directoryEntry = new DirectoryEntry(oracleHostEntryPath, username, password);
            //search.Filter = string.Format("(&(objectClass=computer)(cn={0}))", ComputerName);

            var directorySearcher = new DirectorySearcher(directoryEntry, $"(&(objectclass=AAUF)(cn={computerName}))");

            var test = directorySearcher.FindOne();*/
            //string oracleNetDescription = Encoding.Default.GetString(directorySearcher.FindOne().Properties["orclnetdescstring"][0] as byte[]);


            //var credentials = new UserCredentials(username, password);
            //return Impersonation.RunAsUser(credentials, LogonType.NewCredentials, () =>
            //OracleConnection myConnection = new OracleConnection($"Data Source = adm39.adm.aau.dk:8099; User Id={username};Password={password};");
            /*OracleConnection myConnection = new OracleConnection($"Data Source = adm39.adm.aau.dk:8099");
            try
                {
                    myConnection.Open();
                }
                catch (SqlException)
                {
                    return "Error connection to indkoeb database.";
                }

                string sqlcommand = @"SELECT 
                    SRNR as SRNR,
                    PROJEKT_NR as projectNumber,
                    KOMMENTAR as comment,
                    INDKOEBER as buyer,
                    FROM indkoebsoversigt_v
                    WHERE ?? LIKE " + computerName;

                /*var command = new SqlCommand(sqlcommand, myConnection);

                HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tableHelper.AddRow(new string[] { "Buyer", reader["buyer"] as string });
                    }
                }

                myConnection.Close();

                return tableHelper.GetTable();*/
            //});*/
            return "test";
        }
        private static void Test()
        {
            string directoryServer = "sqlnet.adm.aau.dk:389";
            string defaultAdminContext = "dc=adm,dc=aau,dc=dk";
            string serviceName = "AAUF";
            string userId = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            using (IDbConnection connection = GetConnection(directoryServer, defaultAdminContext, serviceName, userId,
                    password))
            {
                var temp = connection.Database;

                connection.Open();

                connection.Close();
            }

        }

        private static IDbConnection GetConnection(string directoryServer, string defaultAdminContext,
            string serviceName, string userId, string password)
        {
            string descriptor = ConnectionDescriptor(directoryServer, defaultAdminContext, serviceName);
            // Connect to Oracle
            string connectionString = $"Data Source={descriptor}; User Id={userId}; Password={password};";

            OracleConnection con = new OracleConnection(connectionString);
            
            return con;
        }

        private static string ConnectionDescriptor(string directoryServer, string defaultAdminContext,
            string serviceName)
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