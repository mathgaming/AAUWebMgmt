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
        public List<TabModel> Tabs = new List<TabModel>();
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
        public string Manager { get => ADcache.getProperty("manager"); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }
        public ManagementObjectCollection getUserMachineRelationshipFromUserName(string userName) => SCCMcache.getUserMachineRelationshipFromUserName(userName);
        public List<WindowsComputerModel> getManagedWindowsComputers() {
            List<WindowsComputerModel> managedComputerList = new List<WindowsComputerModel>();

            if (UserPrincipalName != "")
            {
                string[] upnsplit = UserPrincipalName.Split('@');
                string domain = upnsplit[1].Split('.')[0];

                string formattedName = string.Format("{0}\\\\{1}", domain, upnsplit[0]);

                foreach (ManagementObject o in getUserMachineRelationshipFromUserName(formattedName))
                {
                    string machineName = o.Properties["ResourceName"].Value.ToString();
                    WindowsComputerModel model = new WindowsComputerModel(machineName);
                    if (!model.ComputerFound)
                    {
                        model = new WindowsComputerModel(model.ComputerName);
                    }
                    managedComputerList.Add(model);
                }
            }
            
            return managedComputerList;
        }

        public bool UserFound = false;

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

        public string AdmDBExpireDate { get; set; }
        public string BasicInfoDepartmentPDS { get; set; }
        public string BasicInfoOfficePDS { get; set; }
        public string BasicInfoADFSLocked { get; set; }
        public string BasicInfoLocked { get; set; }
        public string BasicInfoPasswordExpireDate { get; set; }
        public string BasicInfoTable { get; set; }
        public string BasicInfoRomaing
        {
            get
            {
                return (Profilepath != null).ToString();
            }
        }
        public string CalAgendaStatus { get; set; }
        public string ServiceManager {
            get { return ServiceManager; }
            set { ServiceManager = value; }
        }
        public string ErrorMessages { get; set; }
        public string ResultError { get; set; }
        public string UserName { get; set; } = "mhsv16@its.aau.dk";
        public string ErrorCountMessage { get; set; }
        public string SCSMUserID { get; set; }
        public string Windows7to10 { get; set; }
        public bool ShowFixUserOU { get; set; } = false;
        public bool ShowLoginScript { get; set; } = false;
        public string UsesOnedrive { get; set; } = "False";

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
                UserFound = true;
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
            }
        }

        public void SetTabs()
        {
            Tabs.Add(new TabModel("servicemanager", "Service Manager", true));
            Tabs.Add(new TabModel("calAgenda", "Calendar, Currently: " + CalAgendaStatus));
            Tabs.Add(new TabModel("computerInformation", "Computer Information"));
            Tabs.Add(new TabModel("win7to10", "Windows 7 to 10 upgrade", true));
            Tabs.Add(new TabModel("groups", "Groups"));
            Tabs.Add(new TabModel("fileshares", "Fileshares"));
            Tabs.Add(new TabModel("exchange", "Exchange Resources"));
            Tabs.Add(new TabModel("print", "Print"));
            Tabs.Add(new TabModel("rawdata", "Raw Data"));
            Tabs.Add(new TabModel("tasks", "Tasks"));
            Tabs.Add(new TabModel("warnings", "Warnings"));
        }

        #region loading data
        /*
         * Remember the disclaimer in the documentation about some guy that wrote the legacy code in the weirdest possible manner?
         * This is one of those cases where it applies. How anyone thought it would be a good idea to aquire data, trim and prepare it,
         * as well as actually serving it in the same function is beyond my understanding. Why make clearly defined functions and fields,
         * when you can make super compact code that is impossible to extend? If anything good is to be said about the following pieces of code,
         * it would probably be, that it is a prime example of what NOT to do when making a well-typed program.
         * 
         * Should any of you brave souls that get the duty of maintaining this abomination feel the need to change this, please do.
         * It should be wiped off any harddrive that has seen the ugly underbelly of the software business. If you are not up to the task,
         * I don't blame you. It makes me ill just looking at it too.
             */
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
            List<string> emails = getUserMails();
            string emailString = "";
            foreach (string mailAddress in emails)
            {
                emailString += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", mailAddress);
            }
            sb.Append($"<tr><td>E-mails</td><td>{emailString}</td></tr>");
            
            const int UF_LOCKOUT = 0x0010;
            int userFlags = UserAccountControlComputed;

            BasicInfoLocked = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
                BasicInfoLocked = "True";
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
        public List<string> getUserMails(){
            List<string> emails = new List<string>();
            foreach (string s in ProxyAddresses)
            {
                if (isAnEmail(s))
                {
                    emails.Add(s.ToLower().Replace("smtp:", ""));
                }
            }
            return emails;
        }
        private bool isAnEmail(string s)
        {
            return s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase);
        }

        public string InitCalendarAgenda()
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

            return sb.ToString();
        }

        public static async Task<GetUserAvailabilityResults> getFreeBusyResultsAsync(UserModel UserModel)
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
            service.UseDefaultCredentials = true; // Use domain account for connecting 
            //service.Credentials = new WebCredentials("user1@contoso.com", "password"); // used if we need to enter a password, but for now we are using domain credentials
            //service.AutodiscoverUrl("kyrke@its.aau.dk");  //XXX we should use the service user for webmgmt!
            service.Url = new Uri("https://mail.aau.dk/EWS/exchange.asmx");

            List<AttendeeInfo> attendees = new List<AttendeeInfo>();

            string address = UserModel.UserPrincipalName;
            if (UserModel.UserPrincipalName == "")
            {
                address = UserModel.UserName;
            }

            attendees.Add(new AttendeeInfo()
            {
                SmtpAddress = address,
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

        public string InitComputerInformation()
        {
            try
            {
                string windows = "<h3>Windows computers</h3>";
                var windowsComputers = getManagedWindowsComputers();

                if (windowsComputers.Count != 0)
                {
                    var helper = new HTMLTableHelper(new string[] { "Computer name", "AAU Fjernsupport" });

                    foreach (WindowsComputerModel m in windowsComputers)
                    {
                        string OnedriveWarning = "";
                        try
                        {
                            if (UsesOnedrive.Contains("True") && !OneDriveHelper.ComputerUsesOneDrive(m.ADcache))
                            {
                                OnedriveWarning = "<font color=\"red\"> (Not using Onedrive!)</font>";
                            }
                        }
                        catch (System.Runtime.InteropServices.COMException e)
                        {
                            // Does get an unknown error (0x80005000) when computer is found in SCCM, but not in AD
                        }

                        var name = "<a href=\"/Computer?computername=" + m.ComputerName + "\">" + m.ComputerName + "</a>" + OnedriveWarning + "<br />";
                        var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + m.ComputerName + "\">Start</a>";
                        helper.AddRow(new string[] { name, fjernsupport });
                    }

                    windows += helper.GetTable();
                }
                else
                {
                    windows += "User do not have any Windows computer";
                }

                string mac = "<h3>Mac computers</h3>";
                JamfConnector jamf = new JamfConnector();
                var macComputers = jamf.getComputerNamesForUser(UserPrincipalName);

                if (macComputers.Count != 0)
                {
                    var macHelper = new HTMLTableHelper(new string[] { "Computer name" });

                    foreach (string computername in macComputers)
                    {
                        var name = "<a href=\"/Computer?computername=" + computername + "\">" + computername + "</a>" + "<br />";
                        macHelper.AddRow(new string[] { name });
                    }

                    mac += macHelper.GetTable();
                }
                else
                {
                    mac += "User do not have any Mac computer";
                }

                return $"<h4>Links to computerinfo can be to computers in the wrong domain, because the domain was not found</h4>{windows}{mac}";
            }
            catch (UnauthorizedAccessException)
            {
                return "Service user does not have SCCM access.";
            }
        }

        public PartialGroupModel InitExchange()
        {
            PartialGroupModel model = new PartialGroupModel(ADcache, "memberOf");
            string transitiv = "";

            var members = model.getGroupsTransitive(model.AttributeName);
            if (members.Count == 0)
            {
                transitiv = "<h3>The list only shoes direct members, could not find the full list for the Controller or domain</h3>";
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
                transitiv = "<h3>The list only shoes direct members, could not find the full list for the Controller or domain</h3>";
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

        public string InitLoginScript()
        {
            ShowLoginScript = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(ScriptPath, ADcache.Path);

            if (script != null)
            {
                ShowLoginScript = true;
                return loginscripthelper.parseAndDisplayLoginScript(script);
            }

            return null;
        }

        public void InitWin7to10()
        {
            bool haveWindows7 = false;
            var helper = new HTMLTableHelper(new string[] { "Computername", "Windows 7 to 10 upgrade" });

            foreach (WindowsComputerModel m in getManagedWindowsComputers())
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
