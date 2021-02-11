using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
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
            ComputerModel = GetComputerModel(computername);

            if (computername != null)
            {
                if (ComputerModel.ComputerFound)
                {
                    if (ComputerModel.IsWindows)
                    {
                        new Logger(_context).Log(LogEntryType.ComputerLookup, HttpContext.User.Identity.Name, ComputerModel.Windows.ADPath, true);
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

        private readonly IMemoryCache _cache;
        public ComputerModel ComputerModel;

        public ComputerController(WebMgmtContext context, IMemoryCache cache) : base(context)
        {
            _cache = cache;
        }

        private ComputerModel GetComputerModel(string computerName)
        {
            if (computerName != null)
            {
                computerName = computerName.Trim().ToUpper();
                computerName = computerName.Substring(computerName.IndexOf('\\') + 1);
                if (!_cache.TryGetValue(computerName, out ComputerModel))
                {
                    ComputerModel = new ComputerModel(computerName, GetTrashRequest(computerName));
                    ComputerModel.Windows = new WindowsComputerModel(ComputerModel);

                    if (ComputerModel.IsWindows)
                    {
                        try
                        {
                            ComputerModel.Windows.SetConfig();
                            ComputerModel.Windows.InitBasicInfo();
                            LoadWindowsWarnings();
                            ComputerModel.SetTabs();
                            if (!CheckComputerOU(ComputerModel.Windows.ADPath))
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

                        string sn = computerName;
                        if (sn.Length > 0 && sn[0] != 'S' && sn[0] != 's')
                        {
                            sn = "S" + sn;
                        }
                        MacCSVInfo info = _context.MacCSVInfos.FirstOrDefault(x => x.SerialNumber == sn);

                        if (ComputerModel.Mac.ComputerFound)
                        {
                            if (info != null && !info.OESSAssetNumber.Contains(','))
                            {
                                ComputerModel.SetØSSAssetnumber(info.OESSAssetNumber);
                            }
                            ComputerModel.SetTabs();
                            LoadMacWarnings();
                        }
                        else
                        {
                            if (computerName.StartsWith("AAU") && int.TryParse(computerName.Substring(3), out _))
                            {
                                MacCSVInfo i = _context.MacCSVInfos.FirstOrDefault(x => x.AAUNumber == computerName.Substring(3));
                                if (i != null && i.SerialNumber != null && i.SerialNumber != "")
                                {
                                    string sn2 = i.SerialNumber;
                                    if (i.SerialNumber.StartsWith("S"))
                                    {
                                        sn2 = sn2.Substring(1);
                                    }
                                    ComputerModel.ComputerName = sn2;
                                    
                                    ComputerModel.Mac = new MacComputerModel(ComputerModel);
                                    if (ComputerModel.Mac.ComputerFound)
                                    {
                                        ComputerModel.SetØSSAssetnumber(i.OESSAssetNumber);
                                        ComputerModel.SetTabs();
                                        LoadMacWarnings();
                                    }
                                }
                            }
                            if (!ComputerModel.Mac.ComputerFound)
                            {
                                ComputerModel.ComputerName = computerName;
                                if (info != null)
                                {
                                    ComputerModel.MacCSVInfo = info;
                                    ComputerModel.OESSTables = new ØSSConnector().GetØssTable(info.OESSAssetNumber);
                                    ComputerModel.OESSResponsiblePersonTable = new ØSSConnector().GetResponsiblePersonTable(new ØSSConnector().GetSegmentFromAssetNumber(info.OESSAssetNumber));
                                    ComputerModel.OnlyFoundInOESS = true;
                                }
                                else
                                {
                                    ComputerModel.InitØSSInfo(computerName);
                                    if (ComputerModel.OESSTables.InfoTable.ErrorMessage == null)
                                    {
                                        ComputerModel.OnlyFoundInOESS = true;
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
                            }
                        }
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(ComputerModel.ComputerName, ComputerModel, cacheEntryOptions);
                }
            }
            else
            {
                ComputerModel = new ComputerModel(computerName, GetTrashRequest(computerName));
            }

            return ComputerModel;
        }

        private TrashRequest GetTrashRequest(string computerName)
        {
            return _context.TrashRequests.FirstOrDefault(x => x.ComputerName == computerName);
        }

        public override ActionResult LoadTab(string tabName, string name)
        {
            ComputerModel = GetComputerModel(name);

            new Logger(_context).Log(LogEntryType.LoadedTabComputer, HttpContext.User.Identity.Name, new List<string>() { tabName, ComputerModel.Windows.ADPath }, true);

            string viewName = tabName;
            switch (tabName)
            {
                case "basicinfo":
                    viewName = "Windows/BasicInfo";
                    ComputerModel.Windows.InitBasicInfo();
                    break;
                case "groups":
                    viewName = "Groups";
                    return PartialView(viewName, new PartialGroupModel(ComputerModel.Windows.ADCache, "memberOf"));
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
                    return PartialView("RawTable", ComputerModel.Windows.ADCache.GetAllProperties());
                case "rawdatasccm":
                    viewName = "Windows/SCCMRaw";
                    break;
                case "macbasicinfo":
                    return PartialView("TableView", ComputerModel.Mac.BasicInfoTable);
                case "macHW":
                    return PartialView("TableView", ComputerModel.Mac.HardwareTable);
                case "macSW":
                    return PartialView("Mac/Software", ComputerModel.Mac);
                case "macgroups":
                    return PartialView("TableView", ComputerModel.Mac.GroupsTable);
                case "macnetwork":
                    return PartialView("TableView", ComputerModel.Mac.NetworkTable);
                case "macloaclaccounts":
                    return PartialView("TableView", ComputerModel.Mac.LocalAccountsTable);
                case "macDisk":
                    return PartialView("TableView", ComputerModel.Mac.DiskTable);
                case "purchase":
                    return PartialView("INDB", INDBConnector.GetInfo(ComputerModel.ComputerName));
                case "øss":
                    ComputerModel.InitØSSInfo();
                    return PartialView("OESS", ComputerModel);
            }

            return PartialView(viewName, ComputerModel.Windows);
        }

        [HttpPost]
        public ActionResult MoveOU_Click([FromBody]string computername)
        {
            ComputerModel = GetComputerModel(computername);
            MoveOU(HttpContext.User.Identity.Name, ComputerModel.Windows.ADPath);
            return Success("OU moved for" + computername);
        }

        [HttpPost]
        public ActionResult AddToAAU10([FromBody]string computername)
        {
            return AddComputerToCollection(computername, "AA1000BC", "AAU10 PC");
        }

        [HttpPost]
        public ActionResult AddToAdministrativ10([FromBody]string computername)
        {
            return AddComputerToCollection(computername, "AA1001BD", "Administrativ10 PC");
        }

        private ActionResult AddComputerToCollection(string computerName, string collectionId, string collectionName)
        {
            ComputerModel = GetComputerModel(computerName);

            if (SCCM.AddComputerToCollection(ComputerModel.Windows.SCCMCache.ResourceID, collectionId))
            {
                new Logger(_context).Log(LogEntryType.FixPCConfig, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.ADPath, collectionName });
                return Success("Computer added to " + collectionName);
            }

            return Error("Failed to add computer to group");
        }

        [HttpPost]
        public ActionResult AddToADAAU10([FromBody]string computername)
        {
            ComputerModel = GetComputerModel(computername);
            ADHelper.AddMemberToGroup(ComputerModel.Windows.ADPath.Split("dk/")[1], "LDAP://CN=cm12_config_AAU10,OU=ConfigMgr,OU=Groups,DC=srv,DC=aau,DC=dk");
            new Logger(_context).Log(LogEntryType.AddedToADGroup, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.ADPath, "cm12_config_AAU10" });
            return Success("Computer added to cm12_config_AAU10");
        }

        [HttpPost]
        public ActionResult AddToADAdministrativ10([FromBody]string computername)
        {
            ComputerModel = GetComputerModel(computername);
            ADHelper.AddMemberToGroup(ComputerModel.Windows.ADPath.Split("dk/")[1], "LDAP://CN=cm12_config_Administrativ10,OU=ConfigMgr,OU=Groups,DC=srv,DC=aau,DC=dk");
            new Logger(_context).Log(LogEntryType.AddedToADGroup, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.Windows.ADPath, "cm12_config_Administrativ10" });
            return Success("Computer added to cm12_config_Administrativ10");
        }

        [HttpPost]
        public ActionResult ResultGetPassword([FromBody]string computername)
        {
            ComputerModel = GetComputerModel(computername);
            new Logger(_context).Log(LogEntryType.ComputerAdminPassword, HttpContext.User.Identity.Name, ComputerModel.Windows.ADPath, true);

            var passwordReturned = GetLocalAdminPassword(ComputerModel.Windows.ADPath);

            if (string.IsNullOrEmpty(passwordReturned))
            {
                return Error("Not found");
            }
            else
            {
                return Success(passwordReturned);
            }
        }
        
        
        internal bool ComputerIsInRightOU(string dn)
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

        public static string GetLocalAdminPassword(string ADPath)
        {
            if (string.IsNullOrEmpty(ADPath))
            { //Error no session
                return null;
            }

            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath); // srv\svc_webmgmt is used by the old one

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

        public void MoveOU(string user, string ADPath)
        {
            if (!CheckComputerOU(ADPath))
            {
                //OU is wrong lets calulate the right one
                string[] ADPathsplit = ADPath.ToLower().Replace("ldap://", "").Split('/');
                string protocol = "LDAP://";
                string Domain = ADPathsplit[0];
                string[] dcpath = (ADPathsplit[1].Split(',')).Where<string>(s => s.StartsWith("DC=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();

                string newOU = string.Format("OU=Clients");
                string newPath = string.Format("{0}{1}/{2},{3}", protocol, Domain, newOU, string.Join(",", dcpath));

                new Logger(_context).Log(LogEntryType.UserMoveOU, user, new List<string>() { newPath, ADPath });
                MoveComputerToOU(ADPath, newPath);
            }
        }

        protected void MoveComputerToOU(string ADPath, string newOUpath)
        {
            //Important that LDAP:// is in upper case ! 
            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);
            var newLocaltion = DirectoryEntryCreator.CreateNewDirectoryEntry(newOUpath);
            de.MoveTo(newLocaltion);
            de.Close();
            newLocaltion.Close();
        }

        public bool CheckComputerOU(string ADPath)
        {
            //Check OU and fix it if wrong (only for clients sub folders or new clients)
            //Return true if in right ou (or we think its the right ou, or dont know)
            //Return false if we need to move the ou.

            DirectoryEntry de = DirectoryEntryCreator.CreateNewDirectoryEntry(ADPath);

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
                ComputerModel = GetComputerModel(computername);
                ComputerModel.Windows.ADCache.DeleteComputer();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

            new Logger(_context).Log(LogEntryType.ComputerDeletedFromAD, HttpContext.User.Identity.Name, ComputerModel.Windows.ADPath);

            if (OneDriveHelper.ComputerUsesOneDrive(ComputerModel.Windows.ADCache))
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
                new MissingJavaLicense(this),
                new HaveVirus(this),
                new IsTrashed(this),
                new IsHalfTrashed(this)
            };

            LoadWarnings(warnings);
        }

        private void LoadMacWarnings()
        {
            List<WebMgmtError> warnings = new List<WebMgmtError>
            {
                new NotAAUMac(this),
                new IsTrashedMac(this),
                new IsHalfTrashedMac(this)
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
            ComputerModel.ErrorCountMessage = errorList.GetErrorCountMessage();
            ComputerModel.ErrorMessages = errorList.ErrorMessages;
        }

        public ActionResult AddToOneDrive([FromBody]string data)
        {
            string[] temp = data.Split('|');
            ComputerModel = GetComputerModel(temp[0]);

            if (temp[1].Length == 0)
            {
                return Error("User email cannot be empty");
            }

            UserModel userModel = new UserModel(temp[1]);

            if (userModel.UserFound)
            {
                if (OneDriveHelper.DoesUserUseOneDrive(userModel).Contains("True"))
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

        private void sendTrashComputerEmail(TrashRequest trashRequest)
        {
            string subject = $"Kvittering for afleveringsnummer {trashRequest.Id}";
            string body = $"(Brugers navn) har i dag afleveret {trashRequest.Desciption} med AAU-nummer {trashRequest.ComputerName} til {trashRequest.CreatedBy} hos ITS.\n\n" +
                          $"Kvittering er sendt til {trashRequest.EquipmentManager}({trashRequest.EquipmentManagerEmail})\n\n" +
                          $"Ekstra kvittering er sendt til {trashRequest.RequestedBy}";

            // Hilken anden email skal den sendes til?
            //EmailHelper.SendEmail(subject, body, trashRequest.RequestedBy);
        }
    }
}