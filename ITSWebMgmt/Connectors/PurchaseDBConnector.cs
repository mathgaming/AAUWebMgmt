using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text;

namespace ITSWebMgmt.Connectors
{
    public class PurchaseDBConnector
    {
        public static string getInfo(string computerName)
        {
            string username = Startup.Configuration["cred:indkoeb:username"];
            string password = Startup.Configuration["cred:indkoeb:password"];

            if (username == null || password == null)
            {
                return "Invalid creds for indkoeb";
            }

            IDbConnection conn = GetDatabaseConnection();

            try
            {
                conn.Open();
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
                FROM ITSINDKOEB.INDKOEBSOVERSIGT_V
                WHERE ?? LIKE " + computerName.Substring(3);

            //sqlcommand = @"SELECT * from table_privileges";
            /*sqlcommand = @"SELECT
                  table_name,
                  column_name,
                  data_type
             FROM all_tab_cols
            WHERE table_name = 'INDKOEBSOVERSIGT_V'";*/

            sqlcommand = $"SELECT * FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE SRNR LIKE {computerName.Substring(3)}";

            IDbCommand command = conn.CreateCommand();
            command.CommandText = sqlcommand;

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "Property", "Value" });
            List<string> tables = new List<string>();
            int results = 0;
            int id = int.Parse(computerName.Substring(3));
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results++;
                    for (int i = 0; i < 31; i++)
                    {
                        var value1 = reader.GetValue(i).ToString();
                        int a;
                        if (int.TryParse(value1, out a))
                        {
                            if (id == a)
                            {
                                Console.WriteLine("Yeah");
                            }
                            var tet = reader.GetName(i);
                            Console.WriteLine(tet);
                        }
                        tableHelper.AddRow(new string[] { reader.GetName(i), reader.GetValue(i).ToString() });
                    }
                }
            }

            var tablessergterher = tableHelper.GetTable();

            conn.Close();

            if (results == 0)
            {
                return "AAU number not found in purchase database";
            }
            else if (results > 1)
            {
                return "<h2>Multiple results found! All are listed below</h2>" + tableHelper.GetTable();
            }
            else
            {
                return tableHelper.GetTable();
            }
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