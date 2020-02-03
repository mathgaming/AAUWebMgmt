using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Management;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.Extensions.Caching.Memory;
using ITSWebMgmt.WebMgmtErrors;
using ITSWebMgmt.Models.Log;
using ITSWebMgmt.Connectors;

namespace ITSWebMgmt.Controllers
{
    public class ComputerController : DynamicLoadingWebMgmtController
    {
        public IActionResult Index(string computername)
        {
            ComputerModel = getComputerModel(computername);

            if (computername != null)
            {
                if (ComputerModel.ComputerFound)
                {
                    if (ComputerModel.IsWindows)
                    {
                        new Logger(_context).Log(LogEntryType.ComputerLookup, HttpContext.User.Identity.Name, ComputerModel.Windows.adpath, true);
                    }
                    else
                    {
                        new Logger(_context).Log(LogEntryType.ComputerLookup, HttpContext.User.Identity.Name, ComputerModel.ComputerName, true); //TODO consider other value for mac
                    }
                }
                else
                {
                    new Logger(_context).Log(LogEntryType.ComputerLookup, HttpContext.User.Identity.Name, computername + " (Not found)", true);
                }
            }

            return View(ComputerModel);
        }

        private IMemoryCache _cache;
        public ComputerModel ComputerModel;

        public ComputerController(LogEntryContext context, IMemoryCache cache) : base(context)
        {
            _cache = cache;
        }

        private ComputerModel getComputerModel(string computerName)
        {
            if (computerName != null)
            {
                computerName = computerName.Trim().ToUpper();
                computerName = computerName.Substring(computerName.IndexOf('\\') + 1);
                if (!_cache.TryGetValue(computerName, out ComputerModel))
                {
                    ComputerModel = new ComputerModel(computerName);
                    ComputerModel.Windows = new WindowsComputerModel(ComputerModel);

                    if (ComputerModel.Windows.ComputerFound)
                    {
                        try
                        {
                            ComputerModel.IsWindows = true;
                            ComputerModel.Windows.setConfig();
                            ComputerModel.Windows.InitBasicInfo();
                            LoadWindowsWarnings();
                            ComputerModel.SetTabs();
                            if (!checkComputerOU(ComputerModel.Windows.adpath))
                            {
                                ComputerModel.Windows.ShowMoveComputerOUdiv = true;
                            }
                        }
                        catch (Exception e)
                        {
                            ComputerModel.Windows.ComputerFound = false;
                            ComputerModel.ResultError = "An error uccered while looking up the computer: " + e.Message;
                        }
                    }
                    else
                    {
                        ComputerModel.Mac = new MacComputerModel(ComputerModel);
                        if (ComputerModel.Mac.ComputerFound)
                        {
                            ComputerModel.IsWindows = false;
                            ComputerModel.SetTabs();
                            LoadMacWarnings();
                        }
                        else
                        {
                            try
                            {
                                ComputerModel.ResultError = INDBConnector.LookupComputer(computerName);
                            }
                            catch (Exception e)
                            {
                                HandleError(e);
                                ComputerModel.ResultError = "Computer not found";
                            }
                        }
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(ComputerModel.ComputerName, ComputerModel, cacheEntryOptions);
                }
            }
            else
            {
                ComputerModel = new ComputerModel(computerName);
            }

            return ComputerModel;
        }

        public override ActionResult LoadTab(string tabName, string name)
        {
            ComputerModel = getComputerModel(name);

            new Logger(_context).Log(LogEntryType.LoadedTabComputer, HttpContext.User.Identity.Name, new List<string>() { tabName, ComputerModel.Windows.adpath }, true);

            string viewName = tabName;
            switch (tabName)
            {
                case "basicinfo":
                    viewName = "Windows/BasicInfo";
                    ComputerModel.Windows.InitBasicInfo();
                    break;
                case "groups":
                    viewName = "Groups";
                    return PartialView(viewName, new PartialGroupModel(ComputerModel.Windows.ADcache, "memberOf"));
                case "tasks":
                    viewName = "Windows/Tasks";
                    break;
                case "warnings":
                    return PartialView("RawHTMLTab", new RawHTMLModel("Warnings", ComputerModel.ErrorMessages));
                case "sccminfo":
                    viewName = "Windows/SCCMInfo";
                    ComputerModel.Windows.InitSCCMInfo();
                    break;
                case "sccmcollections":
                    ComputerModel.Windows.InitSCCMCollections();
                    return PartialView("TableView", ComputerModel.Windows.SCCMCollections);
                case "sccmInventory":
                    ComputerModel.Windows.InitSCCMSoftware();
                    return PartialView("FilteredTableView", ComputerModel.Windows.SCCMSoftware);
                case "sccmAV":
                    ComputerModel.Windows.InitSCCMAV();
                    return PartialView("TableView", ComputerModel.Windows.SCCMAV);
                case "sccmHW":
                    viewName = "Windows/SCCMHW";
                    ComputerModel.Windows.InitSCCMHW();
                    break;
                case "rawdata":
                    return PartialView("RawTable", ComputerModel.Windows.ADcache.getAllProperties());
                case "rawdatasccm":
                    viewName = "Windows/SCCMRaw";
                    break;
                case "macbasicinfo":
                    return PartialView("TableView", ComputerModel.Mac.HTMLForBasicInfo);
                case "macHW":
                    return PartialView("TableView", ComputerModel.Mac.HTMLForHardware);
                case "macSW":
                    return PartialView("Software", ComputerModel.Mac);
                case "macgroups":
                    return PartialView("RawHTMLTab", new RawHTMLModel("Groups", ComputerModel.Mac.HTMLForGroups));
                case "macnetwork":
                    return PartialView("TableView", ComputerModel.Mac.HTMLForNetwork);
                case "macloaclaccounts":
                    return PartialView("TableView", ComputerModel.Mac.HTMLForLocalAccounts);
                case "macDisk":
                    return PartialView("TableView", ComputerModel.Mac.HTMLForDisk);
                case "purchase":
                    return PartialView("INDB", INDBConnector.getInfo(ComputerModel.ComputerName));
            }

            return PartialView(viewName, ComputerModel.Windows);
        }

        [HttpPost]
        public ActionResult MoveOU_Click([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            moveOU(HttpContext.User.Identity.Name, ComputerModel.Windows.adpath);
            return Success("OU moved for" + computername);
        }

        [HttpPost]
        public ActionResult AddToAAU10([FromBody]string computername)
        {
            return addComputerTocollection(computername, "AA1000BC", "AAU10 PC");
        }

        [HttpPost]
        public ActionResult AddToAdministrativ10([FromBody]string computername)
        {
            return addComputerTocollection(computername, "AA1001BD", "Administrativ10 PC");
        }

        private ActionResult addComputerTocollection(string computerName, string collectionId, string collectionName)
        {
            ComputerModel = getComputerModel(computerName);

            if (SCCM.AddComputerToCollection(ComputerModel.Windows.SCCMcache.ResourceID, collectionId))
            {
                new Logger(_context).Log(LogEntryType.FixPCConfig, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.adpath, collectionName });
                return Success("Computer added to " + collectionName);
            }

            return Error("Failed to add computer to group");
        }

        [HttpPost]
        public ActionResult AddToADAAU10([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            ADHelper.AddMemberToGroup(ComputerModel.Windows.adpath.Split("dk/")[1], "LDAP://CN=cm12_config_AAU10,OU=ConfigMgr,OU=Groups,DC=srv,DC=aau,DC=dk");
            new Logger(_context).Log(LogEntryType.AddedToADGroup, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.adpath, "cm12_config_AAU10" });
            return Success("Computer added to cm12_config_AAU10");
        }

        [HttpPost]
        public ActionResult AddToADAdministrativ10([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            ADHelper.AddMemberToGroup(ComputerModel.Windows.adpath.Split("dk/")[1], "LDAP://CN=cm12_config_Administrativ10,OU=ConfigMgr,OU=Groups,DC=srv,DC=aau,DC=dk");
            new Logger(_context).Log(LogEntryType.AddedToADGroup, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.adpath, "cm12_config_Administrativ10" });
            return Success("Computer added to cm12_config_Administrativ10");
        }

        [HttpPost]
        public ActionResult ResultGetPassword([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            new Logger(_context).Log(LogEntryType.ComputerAdminPassword, HttpContext.User.Identity.Name, ComputerModel.Windows.adpath, true);

            var passwordRetuned = getLocalAdminPassword(ComputerModel.Windows.adpath);

            if (string.IsNullOrEmpty(passwordRetuned))
            {
                return Error("Not found");
            }
            else
            {
                Func<string, string, string> appendColor = (string x, string color) => { return "<font color=\"" + color + "\">" + x + "</font>"; };

                string passwordWithColor = "";
                foreach (char c in passwordRetuned)
                {
                    var color = "green";
                    if (char.IsNumber(c))
                    {
                        color = "blue";
                    }

                    passwordWithColor += appendColor(c.ToString(), color);

                }

                return Success("<code>" + passwordWithColor + "</code><br /> Password will expire in 8 hours");
            }
        }
        
        
        internal bool computerIsInRightOU(string dn)
        {
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();

            //Check root is people
            if ((ou[0]).Equals("OU=Clients", StringComparison.CurrentCultureIgnoreCase))
            {
                //Computer should be in OU Clients
                return true;
            }

            return false;
        }

        public static string getLocalAdminPassword(string adpath)
        {
            if (string.IsNullOrEmpty(adpath))
            { //Error no session
                return null;
            }

            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(adpath); // srv\svc_webmgmt is used by the old one

            Console.WriteLine(de.Username);

            //XXX if expire time is smaller than 4 hours, you can use this to add time to the password (eg 3h to expire will become 4), never allow a password expire to be larger than the old value

            if (de.Properties.Contains("ms-Mcs-AdmPwd"))
            {
                var f = (de.Properties["ms-Mcs-AdmPwd"][0]).ToString();

                DateTime expiredate = (DateTime.Now).AddHours(8);
                string value = expiredate.ToFileTime().ToString();
                de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value = value;
                de.CommitChanges();

                return f;

            }
            else
            {
                return null;
            }
        }

        public void moveOU(string user, string adpath)
        {
            if (!checkComputerOU(adpath))
            {
                //OU is wrong lets calulate the right one
                string[] adpathsplit = adpath.ToLower().Replace("ldap://", "").Split('/');
                string protocol = "LDAP://";
                string Domain = adpathsplit[0];
                string[] dcpath = (adpathsplit[1].Split(',')).Where<string>(s => s.StartsWith("DC=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();

                string newOU = string.Format("OU=Clients");
                string newPath = string.Format("{0}{1}/{2},{3}", protocol, Domain, newOU, string.Join(",", dcpath));

                new Logger(_context).Log(LogEntryType.UserMoveOU, user, new List<string>() { newPath, adpath });
                moveComputerToOU(adpath, newPath);
            }
        }

        protected void moveComputerToOU(string adpath, string newOUpath)
        {
            //Important that LDAP:// is in upper case ! 
            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(adpath);
            var newLocaltion = DirectoryEntryCreator.CreateNewDirectoryEntry(newOUpath);
            de.MoveTo(newLocaltion);
            de.Close();
            newLocaltion.Close();
        }

        public bool checkComputerOU(string adpath)
        {
            //Check OU and fix it if wrong (only for clients sub folders or new clients)
            //Return true if in right ou (or we think its the right ou, or dont know)
            //Return false if we need to move the ou.

            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(adpath);

            string dn = (string)de.Properties["distinguishedName"][0];
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();
            int count = ou.Count();

            //Check if topou is clients (where is should be)
            if (ou[count - 1].Equals("OU=Clients", StringComparison.CurrentCultureIgnoreCase))
            {
                //XXX why not do this :p return count ==1
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    //Is in a sub ou for clients, we need to move it
                    return false;
                }
            }
            else
            {
                //Is now in Clients, is it in new computere => move to clients else we don't know where to place it (it might be a server)
                if (ou[count - 1].Equals("OU=New Computers", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteComputerFromAD([FromBody]string computername)
        {
            string onedriveMessage = "";
            try
            {
                ComputerModel = getComputerModel(computername);
                ComputerModel.Windows.ADcache.DeleteComputer();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

            new Logger(_context).Log(LogEntryType.ComputerDeletedFromAD, HttpContext.User.Identity.Name, ComputerModel.Windows.adpath);

            if (OneDriveHelper.ComputerUsesOneDrive(ComputerModel.Windows.ADcache))
            {
                onedriveMessage = "Computer used Onedrive before it was deleted. Remeber to add it to Onedrive, if the computer is added to AD again";
            }

            return Success(computername + " have been deleted from AD. " + onedriveMessage);
        }

        private void LoadWindowsWarnings()
        {
            List<WebMgmtError> warnings = new List<WebMgmtError>
            {
                new MissingDataFromSCCM(this),
                new DriveAlmostFull(this),
                new NotStandardComputerOU(this),
                new MissingPCConfig(this),
                new MissingPCADGroup(this),
                new IsWindows7(this),
                new ManagerAndComputerNotInSameDomain(this),
                new PasswordExpired(this),
                new MissingJavaLicense(this)
            };

            LoadWarnings(warnings);
        }

        private void LoadMacWarnings()
        {
            List<WebMgmtError> warnings = new List<WebMgmtError>
            {
                new NotAAUMac(this)
            };

            warnings.AddRange(GetAllMacWarnings());

            LoadWarnings(warnings);
        }

        public IEnumerable<MacWebMgmtError> GetAllMacWarnings()
        {
            foreach (var item in _context.MacErrors.Where(x => x.Active))
            {
                item.computer = this;
                yield return item;
            }
        }

        private void LoadWarnings(List<WebMgmtError> warnings)
        {
            var errorList = new WebMgmtErrorList(warnings);
            ComputerModel.ErrorCountMessage = errorList.getErrorCountMessage();
            ComputerModel.ErrorMessages = errorList.ErrorMessages;
        }

        public ActionResult AddToOneDrive([FromBody]string data)
        {
            string[] temp = data.Split('|');
            ComputerModel = getComputerModel(temp[0]);

            if (temp[1].Length == 0)
            {
                return Error("User email cannot be empty");
            }

            UserModel userModel = new UserModel(temp[1]);

            if (userModel.UserFound)
            {
                if (OneDriveHelper.doesUserUseOneDrive(userModel).Contains("True"))
                {
                    ADHelper.AddMemberToGroup(ComputerModel.Windows.DistinguishedName, "LDAP://CN=GPO_Computer_UseOnedriveStorage,OU=Group Policies,OU=Groups,DC=aau,DC=dk");

                    new Logger(_context).Log(LogEntryType.Onedrive, HttpContext.User.Identity.Name, new List<string>() { userModel.UserPrincipalName, ComputerModel.ComputerName, "Additional computer" });

                    return Success("Computer added to Onedrive group");
                }

                return Error("Could not add computer to Onedrive, because the user do not use Onedrive.\n The user can be added under task for the user");
            }
            else
            {
                return Error("User not found");
            }
        }
    }
}