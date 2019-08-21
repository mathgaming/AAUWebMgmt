using ITSWebMgmt.Caches;
using ITSWebMgmt.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using ITSWebMgmt.Helpers;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.AspNetCore.Http;
using ITSWebMgmt.Connectors;

namespace ITSWebMgmt.Models
{
    public class UserModel : WebMgmtModel<UserADcache>
    {
        public string Guid { get => new Guid((byte[])(ADcache.getProperty("objectGUID"))).ToString(); }
        public string UserPrincipalName { get => ADcache.getProperty("userPrincipalName"); }
        public string DisplayName { get => ADcache.getProperty("displayName"); }
        public string[] ProxyAddresses
        {
            get
            {
                var temp = ADcache.getProperty("proxyAddresses");
                return temp.GetType().Equals(typeof(string)) ? (new string[] { temp }) : temp;
            }
        }
        public int UserAccountControlComputed { get => ADcache.getProperty("msDS-User-Account-Control-Computed"); }
        public int UserAccountControl { get => ADcache.getProperty("userAccountControl"); }
        public string UserPasswordExpiryTimeComputed { get => ADcache.getProperty("msDS-UserPasswordExpiryTimeComputed"); }
        public string GivenName { get => ADcache.getProperty("givenName"); }
        public string SN { get => ADcache.getProperty("sn"); }
        public string AAUStaffID { get => ADcache.getProperty("aauStaffID").ToString(); }
        public string AAUStudentID { get => ADcache.getProperty("aauStudentID").ToString(); }
        public object Profilepath { get => ADcache.getProperty("profilepath"); }
        public string AAUUserClassification { get => ADcache.getProperty("aauUserClassification"); }
        public string AAUUserStatus { get => ADcache.getProperty("aauUserStatus").ToString(); }
        public string ScriptPath { get => ADcache.getProperty("scriptPath"); }
        public bool IsAccountLocked { get => ADcache.getProperty("IsAccountLocked"); }
        public string AAUAAUID { get => ADcache.getProperty("aauAAUID"); }
        public string AAUUUID { get => ADcache.getProperty("aauUUID"); }
        public string TelephoneNumber { get => ADcache.getProperty("telephoneNumber"); set => ADcache.saveProperty("telephoneNumber", value); }
        public string LastLogon { get => ADcache.getProperty("lastLogon"); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }
        public ManagementObjectCollection getUserMachineRelationshipFromUserName(string userName) => SCCMcache.getUserMachineRelationshipFromUserName(userName);
        public List<ComputerModel> getManagedComputers() {
            string[] upnsplit = UserPrincipalName.Split('@');
            string domain = upnsplit[1].Split('.')[0];

            string formattedName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

            List<ComputerModel> managedComputerList = new List<ComputerModel>();

            foreach (ManagementObject o in getUserMachineRelationshipFromUserName(formattedName))
            {
                string machineName = o.Properties["ResourceName"].Value.ToString();
                ComputerModel model = new ComputerModel(machineName);
                if (!model.ComputerFound)
                {
                    model = new ComputerModel(model.ComputerName);
                }
                managedComputerList.Add(model);
            }
            return managedComputerList;
        }

        public string[] getUserInfo()
        {
            return new string[]
            {
                UserPrincipalName,
                AAUAAUID,
                AAUUUID,
                AAUUserStatus,
                AAUStaffID,
                AAUStudentID,
                AAUUserClassification,
                TelephoneNumber,
                LastLogon
            };
        }

        public string AdmDBExpireDate;
        public string BasicInfoDepartmentPDS;
        public string BasicInfoOfficePDS;
        public string BasicInfoPasswordExpired;
        public string BasicInfoPasswordExpireDate;
        public string BasicInfoTable;
        public string BasicInfoRomaing
        {
            get
            {
                return (Profilepath != null).ToString();
            }
        }
        public string Print;
        public string CalAgenda;
        public string CalAgendaStatus;
        public string ServiceManager;
        public string ComputerInformation;
        public string Loginscript;
        public string Rawdata;
        public string ErrorMessages;
        public string ResultError;
        public string UserName = "mhsv16@its.aau.dk";
        public string ErrorCountMessage;
        public string SCSMUserID;
        public string Windows7to10;
        public bool ShowResultDiv = false;
        public bool ShowErrorDiv = false;
        public bool ShowFixUserOU = false;
        public bool ShowLoginScript = false;
        public bool UsesOnedrive = false;

        public UserModel(string username, bool loadDataInbackground = true)
        {
            string adpath = null;
            if (username != null)
            {
                UserName = username;
                adpath = ADHelper.GetADPath(username);
            }
            if (adpath != null)
            {
                ADcache = new UserADcache(adpath);
                SCCMcache = new SCCMcache();
                ShowResultDiv = true;
                ShowErrorDiv = false;
                if (loadDataInbackground)
                {
                    LoadDataInbackground();
                }
            }
            else
            {
                if (username != null)
                {
                    ResultError = $"User ({username}) Not found";
                }
                ShowResultDiv = false;
                ShowErrorDiv = true;
            }
        }

        #region loading data
        public void InitBasicInfo()
        {
            //lblbasicInfoOfficePDS
            if (AAUStaffID != null)
            {
                string empID = AAUStaffID;

                var pds = new PureConnector(empID);
                BasicInfoDepartmentPDS = pds.Department;
                BasicInfoOfficePDS = pds.OfficeAddress;
            }

            //Other fileds
            var attrDisplayName = "UserName, AAU-ID, AAU-UUID, UserStatus, StaffID, StudentID, UserClassification, Telephone, LastLogon (approx.)";
            var attrArry = getUserInfo();
            var dispArry = attrDisplayName.Split(',');
            string[] dateFields = { "lastLogon", "badPasswordTime" };

            var sb = new StringBuilder();
            for (int i = 0; i < attrArry.Length; i++)
            {
                string k = attrArry[i];
                sb.Append("<tr>");

                sb.Append(string.Format("<td>{0}</td>", dispArry[i].Trim()));

                if (k != null)
                {
                    sb.Append(string.Format("<td>{0}</td>", k));
                }
                else
                {
                    sb.Append("<td></td>");
                }

                sb.Append("</tr>");
            }

            //Email
            string email = "";
            foreach (string s in ProxyAddresses)
            {
                if (s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase))
                {
                    var tmp2 = s.ToLower().Replace("smtp:", "");
                    email += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", tmp2);
                }
            }
            sb.Append($"<tr><td>E-mails</td><td>{email}</td></tr>");

            const int UF_LOCKOUT = 0x0010;
            int userFlags = UserAccountControlComputed;

            BasicInfoPasswordExpired = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
                BasicInfoPasswordExpired = "True";
            }

            if (UserPasswordExpiryTimeComputed == "")
            {
                BasicInfoPasswordExpireDate = "Never";
            }
            else
            {
                BasicInfoPasswordExpireDate = UserPasswordExpiryTimeComputed;
            }

            UsesOnedrive = OneDriveHelper.doesUserUseOneDrive(this);

            //OneDrive
            sb.Append($"<tr><td>Uses OneDrive?</td><td>{UsesOnedrive}</td></tr>");

            BasicInfoTable = sb.ToString();

            /*var admdb = new ADMdbConnector();

            string upn = UserPrincipalName;

            string firstName = GivenName;
            string lastName = SN;

            var tmp = upn.Split('@');
            var domain = tmp[1].Split('.')[0];

            //Make lookup in ADMdb
            AdmDBExpireDate = admdb.loadUserExpiredate(domain, tmp[0], firstName, lastName).Result;*/
        }

        public void InitCalendarAgenda()
        {
            CalAgendaStatus = "Free";
            var sb = new StringBuilder();
            // Display available meeting times.

            var temp = getFreeBusyResultsAsync(this).Result;

            DateTime now = DateTime.Now;
            foreach (AttendeeAvailability availability in temp.AttendeesAvailability)
            {
                foreach (CalendarEvent calendarItem in availability.CalendarEvents)
                {
                    if (calendarItem.FreeBusyStatus != LegacyFreeBusyStatus.Free)
                    {
                        bool isNow = false;
                        if (now > calendarItem.StartTime && calendarItem.EndTime > now)
                        {
                            sb.Append("<b>");
                            isNow = true;
                            CalAgendaStatus = calendarItem.FreeBusyStatus.ToString();
                        }
                        sb.Append(string.Format("{0}-{1}: {2}<br/>", calendarItem.StartTime.ToString("HH:mm"), calendarItem.EndTime.ToString("HH:mm"), calendarItem.FreeBusyStatus));

                        if (isNow)
                        {
                            sb.Append("</b>");
                        }
                    }
                }
            }

            CalAgenda = sb.ToString();
        }

        public static async Task<GetUserAvailabilityResults> getFreeBusyResultsAsync(UserModel UserModel)
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
            service.UseDefaultCredentials = true; // Use domain account for connecting 
            //service.Credentials = new WebCredentials("user1@contoso.com", "password"); // used if we need to enter a password, but for now we are using domain credentials
            //service.AutodiscoverUrl("kyrke@its.aau.dk");  //XXX we should use the service user for webmgmt!
            service.Url = new Uri("https://mail.aau.dk/EWS/exchange.asmx");

            List<AttendeeInfo> attendees = new List<AttendeeInfo>();

            attendees.Add(new AttendeeInfo()
            {
                SmtpAddress = UserModel.UserPrincipalName,
                AttendeeType = MeetingAttendeeType.Organizer
            });

            // Specify availability options.
            AvailabilityOptions myOptions = new AvailabilityOptions();

            myOptions.MeetingDuration = 30;
            myOptions.RequestedFreeBusyView = FreeBusyViewType.FreeBusy;

            // Return a set of free/busy times.
            DateTime dayBegin = DateTime.Now.Date;
            var window = new TimeWindow(dayBegin, dayBegin.AddDays(1));
            return await service.GetUserAvailability(attendees, window, AvailabilityData.FreeBusy, myOptions);
        }

        public void InitComputerInformation()
        {
            try
            {
                var helper = new HTMLTableHelper(new string[] { "Computername", "AAU Fjernsupport" });

                foreach (ComputerModel m in getManagedComputers())
                {
                    string OnedriveWarning = "";
                    if (UsesOnedrive && !OneDriveHelper.ComputerUsesOneDrive(m.ADcache))
                    {
                        OnedriveWarning = "<font color=\"red\"> (Not using Onedrive!)</font>";
                    }
                    var name = "<a href=\"/Computer?computername=" + m.ComputerName + "\">" + m.ComputerName + "</a>" + OnedriveWarning + "<br />";
                    var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + m.ComputerName + "\">Start</a>";
                    helper.AddRow(new string[] { name, fjernsupport });
                }
                ComputerInformation = "<h4>Links til computerinfo kan være til maskiner i et forkert domæne, da info omkring computer domæne ikke er tilgængelig i denne søgning</h4>" + helper.GetTable();
            }
            catch (UnauthorizedAccessException e)
            {
                ComputerInformation = "Service user does not have SCCM access.";
            }
        }

        public PartialGroupModel InitExchange()
        {
            PartialGroupModel model = new PartialGroupModel(ADcache, "memberOf");
            string transitiv = "";

            var members = model.getGroupsTransitive(model.AttributeName);
            if (members.Count == 0)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
                members = model.getGroups(model.AttributeName);
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Select Exchange groups and convert to list of ExchangeMailboxGroup
            var exchangeMailboxGroupList = members.Where<string>(group => (group.StartsWith("CN=MBX_"))).Select(x => new ExchangeMailboxGroupModel(x));

            foreach (ExchangeMailboxGroupModel e in exchangeMailboxGroupList)
            {
                var type = e.Type;
                var domain = e.Domain;
                var nameFormated = string.Format("<a href=\"/Group?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + e.RawValue), e.Name);
                var access = e.Access;
                helper.AddRow(new string[] { type, domain, nameFormated, access });
            }

            model.Data = transitiv + helper.GetTable();

            return model;
        }

        public PartialGroupModel InitFileshares()
        {
            PartialGroupModel model = new PartialGroupModel(ADcache, "memberOf");
            string transitiv = "";
            var members = model.getGroupsTransitive(model.AttributeName);

            if (members.Count == 0)
            {
                transitiv = "<h3>NB: Listen viser kun direkte medlemsskaber, kunne ikke finde fuld liste på denne Domain Controller eller domæne</h3>";
                members = model.getGroups(model.AttributeName);
            }

            var helper = new HTMLTableHelper(new string[] { "Type", "Domain", "Name", "Access" });

            //Filter fileshare groups and convert to Fileshare Objects
            var fileshareList = members.Where((string value) =>
            {
                return GroupController.isFileShare(value);
            }).Select(x => new FileshareModel(x));

            foreach (FileshareModel f in fileshareList)
            {
                var nameWithLink = string.Format("<a href=\"/Group?grouppath={0}\">{1}</a><br/>", HttpUtility.UrlEncode("LDAP://" + f.Fileshareraw), f.Name);
                helper.AddRow(new string[] { f.Type, f.Domain, nameWithLink, f.Access });
            }

            model.Data = transitiv + helper.GetTable();

            return model;
        }

        public void InitLoginScript()
        {
            ShowLoginScript = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(ScriptPath, ADcache.Path);

            if (script != null)
            {
                ShowLoginScript = true;
                Loginscript = loginscripthelper.parseAndDisplayLoginScript(script);
            }
        }

        public void InitWin7to10()
        {
            bool haveWindows7 = false;
            var helper = new HTMLTableHelper(new string[] { "Computername", "Windows 7 to 10 upgrade" });

            foreach (ComputerModel m in getManagedComputers())
            {
                m.setConfig();
                string upgradeButton = "";
                if (m.ConfigPC.Equals("AAU7 PC") || m.ConfigPC.Equals("Administrativ7 PC"))
                {
                    var name = "<a href=\"/Computer?computername=" + m.ComputerName + "\">" + m.ComputerName + "</a><br/>";
                    upgradeButton = "<input type=\"button\" value=\"Create Win7 to 10 SR\" onclick=\"submitform('" + m.ComputerName + "');\"/>";
                    helper.AddRow(new string[] { name, upgradeButton });
                    haveWindows7 = true;
                }
            }

            if (haveWindows7)
            {
                var scsm = new SCSMConnector();
                _ = scsm.getUUID(UserPrincipalName).Result;
                SCSMUserID = scsm.userID;

                Windows7to10 = helper.GetTable();
            }
            else
            {
                Windows7to10 = "User do not have any Windows 7 PCs";
            }
        }

        #endregion loading data
    }
}
