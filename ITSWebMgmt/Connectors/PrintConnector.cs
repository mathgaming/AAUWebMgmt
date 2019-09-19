using SimpleImpersonation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ITSWebMgmt.Connectors
{
    public class PrintConnector
    {

        string userGuid = "";

        public PrintConnector(string guid)
        {
            this.userGuid = guid;
        }


        public string doStuff()
        {

            StringBuilder sb = new StringBuilder();

            string domain = Startup.Configuration["cred:equitrac:domain"];
            string username = Startup.Configuration["cred:equitrac:username"];
            string secret = Startup.Configuration["cred:equitrac:password"];

            if (domain == null || username == null || secret == null)
            {
                return "No valid creds for Equitrac";
            }

            var credentials = new UserCredentials(domain, username, secret);
            Impersonation.RunAsUser(credentials, LogonType.NewCredentials, () =>
            {
                SqlConnection myConnection = new SqlConnection("Data Source = AD-SQL2-MISC.AAU.DK; Database = eqcas; Integrated Security=SSPI; MultipleActiveResultSets=true");

                bool SQLSuccess = true;                

                try
                {
                    myConnection.Open();
                }
                catch (SqlException)
                {
                    sb.Append("Error connection to equitrac database.");
                    SQLSuccess = false;
                }

                if (SQLSuccess)
                {
                    string adguid = "AD:{" + userGuid + "}";

                    string sqlcommand = @"
                    	SELECT 
                    		account.creation, 
                    		account.lastmodified, 
                    		account.state,
                    		account.freemoney,
                    		account.balance, 
		                    account.primarypin as AAUCardXerox,
		                    altp.primarypin as AAUCardKonica, 
                            dept.valtype as departmentThing,
                            dept.name as departmentName
                    
                    	FROM cat_validation account
                    		LEFT OUTER JOIN cas_val_assoc      ass  ON ass.associd= account.id
                    		LEFT OUTER JOIN cat_validation     dept ON dept.id    = ass.mainid and dept.valtype = 'dpt'
                    		LEFT OUTER JOIN cas_user_ext       ur   ON ur.x_id    = account.id
                    		LEFT OUTER JOIN cas_user_class     uc   ON uc.id      = classid
                    		LEFT OUTER JOIN cas_primarypin_ext altp ON altp.x_id  = account.id
                    		LEFT JOIN cas_location             loc  ON loc.id     = account.locationid
                    		Where syncidentifier = @adguid;
                   ";

                    var command = new SqlCommand(sqlcommand, myConnection);
                    command.Parameters.AddWithValue("@adguid", adguid);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string AAUCardXerox = reader["AAUCardXerox"] as string;
                            string AAUCardKonica = reader["AAUCardKonica"] as string;
                            string departmentThing = reader["departmentThing"] as string;
                            string departmentName = reader["departmentname"] as string;
                            int state = reader.GetByte(reader.GetOrdinal("state"));

                            decimal free = reader.GetDecimal(reader.GetOrdinal("freemoney"));
                            decimal balance = reader.GetDecimal(reader.GetOrdinal("balance"));
                            decimal paid = balance - free;

                            bool cardok = true;

                            if (state != 1)
                            {
                                sb.Append("Error! Users is disabled in Equitrac<br/>");
                                cardok = false;
                            }
                            if (String.IsNullOrWhiteSpace(AAUCardKonica))
                            {
                                sb.Append("Error! Users is missing AAUCard information in Konica format <br/>");
                                cardok = false;
                            }
                            else
                            {
                                sb.Append("AAUCard Konica: " + AAUCardKonica + "<br/>");
                            }

                            if (String.IsNullOrWhiteSpace(AAUCardXerox))
                            {
                                sb.Append("Error! Users is missing AAUCard information in Xerox format <br/>");
                                cardok = false;
                            }
                            else
                            {
                                sb.Append("AAUCard Xerox: " + AAUCardXerox + "<br/>");
                            }

                            if (cardok)
                            {
                                sb.Append("AAU Card OK <br/>");
                            }

                            if (String.IsNullOrEmpty(departmentThing))
                            {

                                sb.Append("<br/>");
                                sb.Append("Free Credits: " + free);
                                sb.Append("<br/>");
                                sb.Append("Paid Credits: " + paid);
                                sb.Append("<br/>");
                                sb.Append("Remaning Credits: " + balance);

                            }
                            else
                            {
                                sb.Append("Department: " + departmentName);
                                sb.Append("<br/>");
                                sb.Append("User has \"free print\"");
                            }
                        }
                    }

                    myConnection.Close();
                }
            });

            return sb.ToString();
        }

        Dictionary<string, List<string>> GetAllColumnNames(SqlConnection connection)
        {
            Dictionary<string, List<string>> tables = new Dictionary<string, List<string>>();

            foreach (var item in GetAllTables(connection))
            {
                string[] restrictions = new string[4] { null, null, item, null };
                var columnList = connection.GetSchema("Columns", restrictions).AsEnumerable().Select(s => s.Field<String>("Column_Name")).ToList();
                tables.Add(item, columnList);
            }

            return tables;
        }

        string[] GetAllTables(SqlConnection connection)
        {
            List<string> result = new List<string>();
            SqlCommand cmd = new SqlCommand("SELECT name FROM sys.Tables", connection);
            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader["name"].ToString());
            return result.ToArray();
        }
    }
}
