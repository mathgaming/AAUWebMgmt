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
                    new Logger(_context).Log(LogEntryType.ComputerLookup, HttpContext.User.Identity.Name, ComputerModel.adpath, true);
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

        public ComputerController(LogEntryContext context,IMemoryCache cache) : base(context)
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
                    
                    if (ComputerModel.ComputerFound)
                    {
                        try
                        {
                            int testForSCCM = ComputerModel.Collection.Count;
                            ComputerModel.ShowResultDiv = true;
                            ComputerModel.InitSCCMInfo();
                            ComputerModel.InitBasicInfo();
                            LoadWarnings();
                            if (!checkComputerOU(ComputerModel.adpath))
                            {
                                ComputerModel.ShowMoveComputerOUdiv = true;
                            }
                        }
                        catch (Exception)
                        {
                            ComputerModel.ComputerFound = false;
                            ComputerModel.ShowResultDiv = false;
                            ComputerModel.ShowErrorDiv = true;
                            ComputerModel.ResultError = "Computer found in AD but not in SCCM";
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
            string viewName = tabName;
            switch (tabName)
            {
                case "basicinfo":
                    viewName = "BasicInfo";
                    ComputerModel.InitBasicInfo();
                    break;
                case "groups":
                    viewName = "Groups";
                    return PartialView(viewName, new PartialGroupModel(ComputerModel.ADcache, "memberOf"));
                case "tasks":
                    viewName = "Tasks";
                    break;
                case "warnings":
                    viewName = "Warnings";
                    break;
                case "sccminfo":
                    viewName = "SCCMInfo";
                    ComputerModel.InitSCCMInfo();
                    break;
                case "sccmInventory":
                    viewName = "SCCMInventory";
                    ComputerModel.InitSCCMInventory();
                    break;
                case "sccmAV":
                    viewName = "SCCMAV";
                    //DetectionID is required for UserName (SELECT * FROM SMS_G_System_Threats WHERE DetectionID='{04155F79-EB84-4828-9CEC-AC0749C4EDA6}' AND ResourceID=16787705)
                    //Only few computers with data, one them is AAU112782
                    ComputerModel.SCCMAV = TableGenerator.CreateTableFromDatabase(ComputerModel.Antivirus, new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" }, "Antivirus information not found");
                    break;
                case "sccmHW":
                    viewName = "SCCMHW";
                    ComputerModel.InitSCCMHW();
                    break;
                case "rawdata":
                    viewName = "Raw";
                    ComputerModel.Raw = TableGenerator.buildRawTable(ComputerModel.ADcache.getAllProperties());
                    break;
            }

            new Logger(_context).Log(LogEntryType.LoadedTabComputer, HttpContext.User.Identity.Name, new List<string>() { tabName, ComputerModel.adpath }, true);

            return PartialView(viewName, ComputerModel);
        }

        [HttpPost]
        public ActionResult MoveOU_Click([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            moveOU(HttpContext.User.Identity.Name, ComputerModel.adpath);
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

            if (addComputerToCollection(ComputerModel.SCCMcache.ResourceID, collectionId))
            {
                new Logger(_context).Log(LogEntryType.FixPCConfig, HttpContext.User.Identity.Name, new List<string>() { ComputerModel.adpath, collectionName });
                return Success("Computer added to " + collectionName);
            }

            return Error("Failed to add computer to group");
        }

        public void TestButton()
        {
            Console.WriteLine("Test button is in basic info");
        }

        [HttpPost]
        public ActionResult ResultGetPassword([FromBody]string computername)
        {
            ComputerModel = getComputerModel(computername);
            new Logger(_context).Log(LogEntryType.ComputerAdminPassword, HttpContext.User.Identity.Name, ComputerModel.adpath, true);

            var passwordRetuned = getLocalAdminPassword(ComputerModel.adpath);

            if (string.IsNullOrEmpty(passwordRetuned))
            {
                ComputerModel.Result = "Not found";
                return Error(ComputerModel.Result);
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

                ComputerModel.Result = "<code>" + passwordWithColor + "</code><br /> Password will expire in 8 hours";
                return Success(ComputerModel.Result);
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


            //DirectorySearcher search = new DirectorySearcher(de);
            //search.PropertiesToLoad.Add("ms-Mcs-AdmPwd");
            //SearchResult r = search.FindOne();

            //if (r != null && r.Properties.Contains("ms-Mcs-AdmPwd"))
            //{
            //    return r.Properties["ms-Mcs-AdmPwd"][0].ToString();

            //DirectoryEntry  = r.GetDirectoryEntry();
            //de


            //}
            //else { 
            //    return null;
            //}
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

            //return true;

        }

        protected bool addComputerToCollection(string resourceID, string collectionID)
        {
            var pathString = "\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_Collection.CollectionID=\"" + collectionID + "\"";
            ManagementPath path = new ManagementPath(pathString);
            ManagementObject obj = new ManagementObject(path);
            obj.Scope = new ManagementScope(pathString, SCCM.GetConnectionOptions());

            ManagementClass ruleClass = new ManagementClass("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1" + ":SMS_CollectionRuleDirect");

            ManagementObject rule = ruleClass.CreateInstance();

            rule["RuleName"] = "Static-" + resourceID;
            rule["ResourceClassName"] = "SMS_R_System";
            rule["ResourceID"] = resourceID;
            obj.InvokeMethod("AddMembershipRule", new object[] { rule });

            return true;
        }

        [HttpPost]
        public ActionResult DeleteComputerFromAD([FromBody]string computername)
        {
            try
            {
                ComputerModel = getComputerModel(computername);
                ComputerModel.ADcache.DeleteComputer();
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

            new Logger(_context).Log(LogEntryType.ComputerDeletedFromAD, HttpContext.User.Identity.Name, ComputerModel.adpath);

            return Success(computername + " have been deleted from AD");
        }

        private void LoadWarnings()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new DriveAlmostFull(this),
                new NotStandardComputerOU(this),
                new MissingPCConfig(this)
            };

            var errorList = new WebMgmtErrorList(errors);
            ComputerModel.ErrorCountMessage = errorList.getErrorCountMessage();
            ComputerModel.ErrorMessages = errorList.ErrorMessages;

            //Password is expired and warning before expire (same timeline as windows displays warning)
        }

    }
}