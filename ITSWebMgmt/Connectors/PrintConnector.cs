using ITSWebMgmt.Models;
using SimpleImpersonation;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ITSWebMgmt.Connectors
{
    public class PrintConnector
    {
        readonly string userGuid = "";

        public PrintConnector(string guid)
        {
            userGuid = guid;
        }

        public PrintModel GetData()
        {
            PrintModel model = new PrintModel();

            string domain = Startup.Configuration["cred:equitrac:domain"];
            string username = Startup.Configuration["cred:equitrac:username"];
            string secret = Startup.Configuration["cred:equitrac:password"];

            if (domain == null || username == null || secret == null)
            {
                model.CredentialError = "No valid creds for Equitrac";
            }

            var credentials = new UserCredentials(domain, username, secret);
            Impersonation.RunAsUser(credentials, LogonType.NewCredentials, () =>
            {
                SqlConnection myConnection = new SqlConnection("Data Source = AD-SQL2-MISC.AAU.DK; Database = eqcas; Integrated Security=SSPI; MultipleActiveResultSets=true");
                

                try
                {
                    myConnection.Open();
                }
                catch (SqlException)
                {
                    model.ConectionError = "Error connection to equitrac database.";
                }

                if (model.ConectionError == null)
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
                            model.Found = true;
                            string AAUCardXerox = reader["AAUCardXerox"] as string;
                            string AAUCardKonica = reader["AAUCardKonica"] as string;
                            model.DepartmentThing = reader["departmentThing"] as string;
                            model.DepartmentName = reader["departmentname"] as string;
                            int state = reader.GetByte(reader.GetOrdinal("state"));

                            if (state != 1)
                            {
                                model.EquitracDisabled = "Error! Users is disabled in Equitrac";
                            }
                            if (string.IsNullOrWhiteSpace(AAUCardKonica))
                            {
                                model.AAUCardKonica = "Error! Users is missing AAUCard information in Konica format";
                            }
                            else
                            {
                                model.AAUCardKonica = "AAUCard Konica: " + AAUCardKonica;
                            }

                            if (string.IsNullOrWhiteSpace(AAUCardXerox))
                            {
                                model.AAUCardXerox = "Error! Users is missing AAUCard information in Xerox format";
                            }
                            else
                            {
                                model.AAUCardXerox = "AAUCard Xerox: " + AAUCardXerox;
                            }

                            model.Free = reader.GetDecimal(reader.GetOrdinal("freemoney"));
                            model.Balance = reader.GetDecimal(reader.GetOrdinal("balance"));
                        }
                        else
                        {
                            model.Found = false;
                        }
                    }

                    myConnection.Close();
                }
            });

            return model;
        }

        Dictionary<string, List<string>> GetAllColumnNames(SqlConnection connection)
        {
            Dictionary<string, List<string>> tables = new Dictionary<string, List<string>>();

            foreach (var item in GetAllTables(connection))
            {
                string[] restrictions = new string[4] { null, null, item, null };
                var columnList = connection.GetSchema("Columns", restrictions).AsEnumerable().Select(s => s.Field<string>("Column_Name")).ToList();
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
