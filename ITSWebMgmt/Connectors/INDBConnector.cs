using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
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
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE UDSTYRS_REGISTRERINGS_NR LIKE {computerName.Substring(3)} OR UDSTYRS_REGISTRERINGS_NR LIKE {computerName}";

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
                            string value = reader.GetValue(i).ToString();
                            if (value == "")
                            {
                                tableHelper.AddRow(new string[] { reader.GetName(i), "Date not found" });
                            }
                            else
                            {
                                tableHelper.AddRow(new string[] { reader.GetName(i), DateTime.Parse(value).ToString("yyyy-MM-dd") });
                            }
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

        public static string GetAllMacs()
        {
            var connection = tryConnect();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;

            IDbCommand command = conn.CreateCommand();
            command.CommandText = $"SELECT " +
                $"SERIENR," +
                $"BESTILLINGS_DATO," +
                $"TYPEN," +
                $"FABRIKAT," +
                $"MODELLEN," +
                $"UDSTYRS_REGISTRERINGS_NR," +
                $"SLUTBRUGER," +
                $"KOMMENTAR," +
                $"PROJEKT_NR," +
                $"SRNR," +
                $"BESTILLER" +
                $" FROM ITSINDKOEB.INDKOEBSOVERSIGT_V WHERE" +
                $"(TYPEN LIKE 'Bærbar' OR TYPEN LIKE 'PC' OR TYPEN LIKE 'PC, Bærbar')" +
                $"AND FABRIKAT LIKE 'Apple'" +
                $"AND MODELLEN NOT LIKE '%iPad%' AND MODELLEN NOT LIKE '%ipad%'AND MODELLEN NOT LIKE '%Ipad%'" +
                $"AND UDSTYRS_REGISTRERINGS_NR IS NOT NULL";

            HTMLTableHelper tableHelper = new HTMLTableHelper(new string[] { "SERIENR", "BESTILLINGS_DATO", "TYPEN", "FABRIKAT", "MODELLEN", "UDSTYRS_REGISTRERINGS_NR", "SLUTBRUGER", "KOMMENTAR", "PROJEKT_NR", "SRNR", "BESTILLER" });

            int count = 0;

            List<string> macsInJamf = new List<string>() { "C02L29LAF8J5", "FVFX61FKHV2J", "C02T129HH03M", "FVFXQ0E1HV2J", "FVFXP2XEHV2D", "FVFX91HNHV2J", "C02GP5XFDV14", "C02KV19RFFRP", "C02J68PFDKQ5", "C02M2D1JFD57", "C02H55T0DHJP", "C02S902WFVH5", "C02XG16BJHD5", "C02Q270FG8WN", "C02WG01LHV2V", "C02KWBDTF6T6", "C02Q510NFWW4", "C02QF86QFVH5", "C02PJ2D8FVH7", "C02QL1USFVH8", "C02NT0PPG3QR", "C02NW186G3QT", "C02NN562G3QK", "C02P422LG3QR", "C02P14ENG3QJ", "C02P3082G3QK", "C1MPT5V0G944", "C02PT12KFVH6", "C02PW2GKFVH6", "F5KPD04MF9VN", "C02PH0NRG3QP", "C02PD7TUG3QC", "C07PM1NYG1J1", "C02X833VJGH6", "C02Q50U8FVH8", "C02Q93W1FVH7", "C02KC6WDDNMP", "C02K9133DNMP", "C02KD0LJFFT1", "C02L490CF5V7", "C02MG8QXF8J4", "C02MG5UKF8J4", "C02MG61DF8J4", "C02MG61NF8J4", "C02LR3JSFD59", "DGKJW03ADNMM", "DGKJW02VDNMM", "DGKJW038DNMM", "DGKJW035DNMM", "DGKJW034DNMM", "DGKJW02TDNMM", "DGKJW02XDNMM", "DGKJW0ADDNMM", "DGKJW03BDNMM", "DGKJW02ZDNMM", "DGKJW039DNMM", "DGKJW032DNMM", "DGKJW02YDNMM", "DGKJW02WDNMM", "DGKJW031DNMM", "DGKJW030DNMM", "C02M207EFH05", "C02M439DFH00", "C02MMAEFFD57", "C02N176SG086", "C02NC0QEG3QJ", "C02NT1T0G5RN", "C02LV0D8FH04", "C02Q274LG8WN", "C02PTJHRFVH5", "C02QP0NTFVH8", "C02QC87NFVH5", "C02R61V8FVH8", "C02PTJCUFVH5", "C02QL1UXFVH8", "C02RWC43G8WN", "C02QN2J1FVH8", "C02PF176FY11", "C02VR63ZHV2D", "C02PTJHUFVH5", "C02RT0VTFVH7", "C02QF86SFVH5", "C1MR41N4G944", "C02R85XDFVH8", "C02R90U0G8WL", "C02RC2EVG8WL", "C02R1074G8WP", "C02RJ0ZCG8WL", "C02R21DMG8WN", "C02RG1JWFVH8", "C02RH09JG8WP", "C02RK23RFVH8", "C02RL27BFVH8", "C02RL2C5FVH8", "C02RN2X7FVH8", "C02RL2C4FVH8", "C02RL2KMG8WL", "C02Q73XVFVH8", "C02RV5NVFVH5", "C1MS511TH3QJ", "C02RWF8GFVH5", "C02RL27CFVH8", "C02RT0WPFVH7", "C02SC0XNFVH5", "C02SQ37QFVH4", "C02T10UKFVH4", "C02T61CYGTDY", "C02TH2B3FVH4", "C02Y60KPJHD5", "C02XQ440JGH5", "C02T32SQHF1Q", "C02T34RQHF1Q", "C02SGMVAFVH5", "C02SQ17WG8WL", "C02SLML8G8WN", "C02SL9DMGY25", "C1MS1Z51H3QF", "C02T61HVFVH4", "C02T61HUFVH4", "C02T30E4G8WL", "C02ST2SZFVH4", "C02SWUE6GTF1", "C02SW37ZG8WL", "C02SW4D8FVH4", "C02SW52GHF1P", "C02T40JNH03M", "C02TD5F9FVH4", "C02ST2SXFVH4", "C02TM0JKHF1Q", "C02TP41DFVH4", "C02TM0K9HF1Q", "C02SH272FVH8", "C02KJ2SXFFRP", "C02TV033HTD8", "C02TR1GLHTD7", "C02V30VJHV2J", "C02W922JHV2D", "C02WF073HV2J", "C02WF074HV2J", "C02W200PHV2D", "C02VX086HTD7", "C02WL09KG8WN", "C02V420XHH23", "FVFWH0U5HV2J", "C02VW02VHTD7", "FVFWK0E8HV2J", "C02VX084HTD7", "FVFWW2RRHV2D", "FVFWV1H0HV2D", "FVFX61FLHV2J", "C02Y20G9JHD3", "C02V50SNHH23", "FVFX255RHV27", "C02XG68JJGH6", "C02XD1BTJG5M", "FVFX667MHV2D", "FVFX666XHV2D", "FVFX669FHV2D", "FVFX665HHV2D", "C02X1865JG5L", "FVFX90HXHV2J", "C02WX15DJG5M", "C02XD10LJGH7", "C02WX600JG5J", "C02XG68HJGH6", "C02XH23FJHCF", "C02XD1D8JG5M", "FVFXK05SHV2J", "FVFXK05UHV2J", "C02XK05NJHD5", "FVFXK15QHV2D", "C02T34RHHF1Q", "C02V30VHHV2J", "C02SL07GGTHY", "C02V9198HV2J", "C02TR1HFHTD7", "C02VD7M3HTD6", "C02W80WQHV2J", "FVFWK0Q3HV2J", "FVFWH40ZHV29", "FVFX56CBHV2D", "FVFXC1A1HV2J", "FVFXC1A3HV2J", "FVFXC19XHV2J", "FVFX74YHHV2D", "C02XG24UJGH7", "FVFXN0C3HV2J", "FVFXN0XZHV2J", "C02XL4M2JHCD", "C02TG56VFVH4", "C02TG5PAFVH4", "C02TN6P3FVH4", "C02TX0B7HV2D", "C02V30VLHV2J", "C02V30VKHV2J", "C02TR18XHV2P", "C02V317UG8WL", "C02TX0B2HV2D", "C02V80S7HV2J", "C02TX1SRHV2D", "C02TR20QHTD8", "C02V90A3HTD8", "C02VF0UEHV2J", "C02VF0UEHV2J", "C02V14R4HV2D", "C02VD01EHV2P", "C02VD82EHV2D", "C02VM0QDHV2J", "C02VJ02HHV2P", "C02VD3MBHV2N", "C02VF06PHV2D", "C02VQ0NSG8WL", "C02W21AKHV2V", "C02V75MUJ1GJ", "C02VD17YHV2P", "C02VR62KHV2D", "C02W31G1G8WL", "C02W30MFG8WL", "C02W80WRHV2J", "C02W80THG8WL", "C02XF266JG5M", "C02Y12QKJHD3", "C02XK05YJGH7", "C02XH05TJGH7", "C02XJ7MAJHCD", "C02XW0DSJGH7", "C02Y60AQJHD5", "FVFXK05QHV2J", "FVFY1151HV2J", "FVFXQ0XQHV2J", "FVFY1157HV2J", "FVFXQ0E2HV2J", "FVFY115HHV2J", "FVFXK05PHV2J", "FVFY114ZHV2J", "C02Z3059LVDT", "C07Z40ZSJYVY", "C02Z40E0LVDN", "C02YX2RNLVDH", "C02YNAQMLVCJ", "C02YX11BL412", "C02V40DDHH24", "C02Y90ZPJG5M", "DGKYMHLYJV3Y", "C02Y10QRJHD5", "C02YT2GXL412", "C02Z35L3L410", "FVFXK05LHV2J", "FVFXK05NHV2J", "C17YN9VZLVCF", "C02ZD065LVDN", "C02Z86FLLVDL", "C17YN9Z2LVCF", "C02Z35KQL410", "C02Z96H9LVDQ", "C02VJ037HH24", "C02XQ0HXJGH7", "C02QK1Z2FWW4", "C02TH21YFVH4", "YM8374PYZE4", "C02QN1WJFWW3", "W89239F5642", "C07YP2CLJYW0", "C02NF3ENG3QC", "W8025E6UDAS", "C02T912XG8WL", "C2VGFGZ7DV13", "C17H8K2GDV13", "C02FF3DFDHJN", "C02GD68GDJWV", "C02GFKPHDJWT", "C07F20UDDD6H", "C02GY1PKDHJQ", "C02NGE1LG3QJ", "C02NLKQMG3QC", "C02G52WHDHJQ", "C02HFHXQDV7L", "C02GV5BCDJWV", "DGKJC02ADHJV", "C02NQ4WSG5RN", "C02NW2A0G5RP", "C02NW7QLG3QJ", "C02NFJ72G3QJ", "C02K53BEFFRP", "C02HK2GSDV33", "C07H881UDJD0", "C02JG118F1G3", "C02J1323DV33", "C02JXKG0DNCV", "C02K514PF1G4", "C1ML37GYDTY3", "C02J75BXDTY4", "C07J109ADJD3", "C17SM5UQGY25", "C02JF7ESDRVC", "C02V14R1HV2D", "C02N20UYFH05", "VMgj7slrw902", "VMipkjhtq085", "VMipkjhtq086", "VM94gokvtn2d", "VMjpai8ow94t", "VMgj7slrw900", "VMgj7slrw901", "VM9fhFfmrxa0", "VMipkjhtq100", "VMipkjhtq090", "C02H52LLDJWT", "C02HTDXMDRVD", "C02HWJDHDRV7", "C02MMHA4FH00", "C02JD15QF1G3", "C02HN081DV14", "C02VD3MBHV2N", "C02PTJCAFVH5", "C02T32S6HF1Q", "C02V14QUHV2D", "C02TV3L4G8WN", "C02FK1D3DDR4", "FVFY115GHV2J", "C02Q5AK3G8WN", "W80476SKAGV", "C02HQJNGDTY4", "C02XD1BZJHD5", "C02X906AJGH7", "VMipkjhtq084", "VM08hzaiyl94", "C02V41XLHH23", "C02XC0P7JGH7", "C02YL0X7LVCJ", "C02GV5GXDJWV", "FVFXJ0J2HV2J", "C02Y60APJHD5", "C02N89WYG3QJ", "C02X7566JGH6", "C02XG26XJG5L", "C02SGQLTFVH5", "C02QK277FVH8", "C02XF0SHJHCF", "C02XQ0H5JGH7", "C02ST1U6HF1Q", "FVFY1156HV2J", "C02MCLP4FH00", "C02ST2SYFVH4", "C02QP086G8WL", "FVFXR15DHV2J", "C02VF1TMG8WL", "C02V50WWHV2N", "FVFX70KCHV2D", "C02FG0VYDF93", "C02XL4BVJGH6", "C02XW35XJGH6", "FVFXK05THV2J", "C02S22Q3GG7N", "VM02022V5RU", "FVFX90J0HV2J", "DGKK90BRDNMN", "C02FM15TDDR1", "C02QT4ACFVH5", "C02XN0CCJGH7", "C02HX5MXDKQ1", "C02V40DDHH24", "DGKVFHP0J1GP", "C02XG3WFJHD3", "C02HV058DKQ4", "C02X90BXJGH7", "C02XG13LJHD5", "C02XH030JGH7", "C02XD1BSJG5M", "C02XF0L7JG5M", "C02Y612EJHD3", "C02XK05QJHD5", "C02XD22SJGH6", "FVFXG19BHV2J", "C02Y748WJGH6", "FVFXK05MHV2J", "C02XM0M5JHD5", "FVFXQ06GHV2J", "FVFX56BDHV2D", "C02XF3R5JHCD", "C02Y10S8JHD5", "C17FVQZNDH2G", "C02X70ZJJHD5", "C02X92PDJG5M", "C02PLB0QFVH7", "C02Y10R3JHD5", "C02Y60AMJHD5", "FVFXC1A2HV2J", "C02NF3P1G3QC", "C02Y32CQJHCD", "C02VT0Q2HV2J", "C02Y60KNJHD5", "C02Y206QJGH7", "FVFX6670HV2D", "DGKRD062GG78", "FVFYX140HV2J", "C02XK05PJHD5", "FVFX80Z2HV2J", "C02Y10QZJHD5", "C02P9ADJFVH5", "C02XK05XJGH7", "C02YD0RJJHD5", "C02XF3QJJHCD", "C02ML7BMFH00", "C02X833VJGH6", "FVFXQ06HHV2J", "C02XH23CJHCF", "C02XF1B0JG5M", "C02VG1JUHV2J", "C02L64CHF6T6", "C02M69E1FH00" };

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> entry = new List<string>();
                    for (int i = 0; i < 11; i++)
                    {
                        entry.Add(reader.GetValue(i).ToString());
                    }

                    var name = reader.GetValue(0).ToString();
                    if (name.StartsWith("S"))
                    {
                        name = name.Substring(1);
                    }

                    var projectNr = reader.GetValue(8).ToString();
                    if (projectNr.Length > 7)
                    {
                        int n = 0;
                        string id = projectNr.Substring(0, 7);

                        bool isNumeric = int.TryParse(id , out n);
                        if (!macsInJamf.Contains(name) && isNumeric)
                        {
                            tableHelper.AddRow(entry.ToArray());
                            count++;

                            string a = ADHelper.AAUIdSearch(id);
                            Console.WriteLine(a);
                        }
                    }
                }
            }

            conn.Close();

            return count + "    " + tableHelper.GetTable();
        }

        public static string LookupComputer(string computerName)
        {
            var connection = tryConnect();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;

            if (!computerName.StartsWith("AAU"))
            {
                return "Computer not found";
            }

            var computerNameDegits = computerName.Substring(3);
            int val = 0;
            if (!int.TryParse(computerNameDegits, out val))
            {
                return "Computer not found";
            }

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
                return "Computer not found";
            }
            else
            {
                return $"Computer has not been registrered in AD or Jamf, but was found in INDB. The manufacturer is {manifacturer} and the model is {model}";
            }
        }

        //Only used for test
        public static string GetFullInfo(string computerName)
        {
            var connection = tryConnect();
            if (connection.conn == null)
                return connection.error;

            var conn = connection.conn;

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
