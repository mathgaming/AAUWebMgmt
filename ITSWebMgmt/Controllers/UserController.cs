using ITSWebMgmt.Caches;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Functions;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using ITSWebMgmt.ViewInitialisers.User;
using ITSWebMgmt.WebMgmtErrors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ITSWebMgmt.Controllers
{
    public class UserController : DynamicLoadingWebMgmtController
    {
        public IActionResult Index(string username)
        {
            UserModel = getUserModel(username);
            return View(UserModel);
        }

        private IMemoryCache _cache;
        public UserModel UserModel;

        public UserController(IMemoryCache cache)
        {
            _cache = cache;
        }

        private UserModel getUserModel(string username)
        {
            if (username != null)
            {
                if (!_cache.TryGetValue(username, out UserModel))
                {
                    username = username.Trim();
                    UserModel = new UserModel(username, lookupUser(username));
                    UserModel = BasicInfo.Init(UserModel, HttpContext);
                    LoadWarnings();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(username, UserModel, cacheEntryOptions);
                }
            }
            else
            {
                UserModel = new UserModel(null, null);
            }

            return UserModel;
        }

        protected string lookupUser(string username)
        {
            int val;
            if (username.Length == 4 && int.TryParse(username, out val))
            {
                return doPhoneSearch(username);
            }
            else
            {
                return getADPathFromUsername(username);
            }
        }

        public string globalSearch(string email)
        {
            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            string filter = string.Format("(|(proxyaddresses=SMTP:{0})(userPrincipalName={0}))", email);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            SearchResult r = search.FindOne();

            if (r != null)
            {
                //return r.Properties["userPrincipalName"][0].ToString(); //XXX handle if result is 0 (null exception)
                string adpath = r.Properties["ADsPath"][0].ToString();
                return adpath.Replace("aau.dk/", "").Replace("GC:", "LDAP:");
            }
            else
            {
                return null;
            }
        }

        //Searhces on a phone numer (internal or external), and returns a upn (later ADsPath) to a use or null if not found
        public string doPhoneSearch(string numberIn)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string number = numberIn;
            //If number is a shot internal number, expand it :)
            if (number.Length == 4)
            {
                // format is 3452
                number = string.Format("+459940{0}", number);

            }
            else if (number.Length == 6)
            {
                //format is +453452 
                number = string.Format("+459940{0}", number.Replace("+45", ""));

            }
            else if (number.Length == 8)
            {
                //format is 99403442
                number = string.Format("+45{0}", number);

            } // else format is ok

            DirectoryEntry de = new DirectoryEntry("GC://aau.dk");
            //string filter = string.Format("(&(objectCategory=person)(telephoneNumber={0}))", number);
            string filter = string.Format("(&(objectCategory=person)(objectClass=user)(telephoneNumber={0}))", number);

            //logger.Debug(filter);

            DirectorySearcher search = new DirectorySearcher(de, filter);
            search.PropertiesToLoad.Add("userPrincipalName"); //Load something to speed up the object get?
            SearchResult r = search.FindOne();

            watch.Stop();
            System.Diagnostics.Debug.WriteLine("phonesearch took: " + watch.ElapsedMilliseconds);

            if (r != null)
            {
                //return r.Properties["ADsPath"][0].ToString(); //XXX handle if result is 0 (null exception)
                return r.Properties["ADsPath"][0].ToString().Replace("GC:", "LDAP:").Replace("aau.dk/", "");
            }
            else
            {
                return null;
            }
        }

        public bool userIsInRightOU()
        {
            string dn = UserModel.DistinguishedName;
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();
            if (count < 2)
            {
                return false;
            }
            //Check root is people
            if (!(ou[count - 1]).Equals("ou=people", StringComparison.CurrentCultureIgnoreCase))
            {
                //Error user is not placed in people!!!!! Cant move the user (might not be a real user or admin or computer)
                return false;
            }
            string[] okplaces = new string[3] { "ou=staff", "ou=guests", "ou=students" };
            if (!okplaces.Contains(ou[count - 2], StringComparer.OrdinalIgnoreCase))
            {
                //Error user is not in out staff, people or student, what is gowing on here?
                return false;
            }
            if (count > 2)
            {
                return false;
            }
            return true;
        }

        public ActionResult FixUserOu([FromBody]string username)
        {
            UserModel = getUserModel(username);
            if (userIsInRightOU()) { return Error(); }

            //See if it can be fixed!
            string dn = UserModel.DistinguishedName;
            string[] dnarray = dn.Split(',');

            string[] ou = dnarray.Where(x => x.StartsWith("ou=", StringComparison.CurrentCultureIgnoreCase)).ToArray();

            int count = ou.Count();

            if (count < 2)
            {
                return Error("This cant be in people/{staff,student,guest}");
            }
            //Check root is people
            if (!(ou[count - 1]).Equals("ou=people", StringComparison.CurrentCultureIgnoreCase))
            {
                return Error("Error user is not placed in people!!!!! Cant move the user (might not be a real user or admin or computer)");
            }
            string[] okplaces = new string[3] { "ou=staff", "ou=guests", "ou=students" };
            if (!okplaces.Contains(ou[count - 2], StringComparer.OrdinalIgnoreCase))
            {
                return Error("Error user is not in out staff, people or student, what is gowing on here?");
            }
            if (count > 2)
            {
                //User is not placed in people/{staff,student,guest}, but in a sub ou, we need to do somthing!
                //from above check we know the path is people/{staff,student,guest}, lets generate new OU

                //Format ldap://DOMAIN/pathtoOU
                //return false; //XX Return false here?

                string[] adpathsplit = UserModel.adpath.ToLower().Replace("ldap://", "").Split('/');
                string protocol = "LDAP://";
                string domain = adpathsplit[0];
                string[] dcpath = (adpathsplit[0].Split(',')).Where<string>(s => s.StartsWith("dc=", StringComparison.CurrentCultureIgnoreCase)).ToArray<string>();

                string newOU = string.Format("{0},{1}", ou[count - 2], ou[count - 1]);
                string newPath = string.Format("{0}{1}/{2},{3}", protocol, string.Join(".", dcpath).Replace("dc=", ""), newOU, string.Join(",", dcpath));

                logger.Info("user " + ControllerContext.HttpContext.User.Identity.Name + " changed OU on user to: " + newPath + " from " + UserModel.adpath + ".");

                var newLocaltion = new DirectoryEntry(newPath);
                UserModel.ADcache.DE.MoveTo(newLocaltion);

                return Success();
            }
            //We don't need to do anything, user is placed in the right ou! (we think, can still be in wrong ou fx a guest changed to staff, we cant check that here) 
            logger.Debug("no need to change user {0} out, all is good", UserModel.adpath);
            return Success();
        }


        public string getADPathFromUsername(string username)
        {
            //XXX, this is a use input, might not be save us use in log 
            logger.Info("User {0} lookedup user {1}", ControllerContext.HttpContext.User.Identity.Name, username);

            if (username.Contains("\\"))
            {
                //Form is domain\useranme
                var tmp = username.Split('\\');
                if (!tmp[0].Equals("AAU", StringComparison.CurrentCultureIgnoreCase))
                {
                    username = string.Format("{0}@{1}.aau.dk", tmp[1], tmp[0]);
                }
                else //IS AAU domain 
                {
                    username = string.Format("{0}@{1}.dk", tmp[1], tmp[0]);
                }
            }

            var adpath = globalSearch(username);
            if (adpath == null)
            {
                //Show user not found
                return null;
            }
            else
            {
                //We got ADPATH lets build the GUI
                return adpath;
            }
        }


        public ActionResult UnlockUserAccount([FromBody]string username)
        {
            UserModel = getUserModel(username);
            logger.Info("User {0} unlocked useraccont {1}", ControllerContext.HttpContext.User.Identity.Name, UserModel.adpath);

            try
            {
                UserModel.ADcache.DE.Properties["LockOutTime"].Value = 0; //unlock account
                UserModel.ADcache.DE.CommitChanges(); //may not be needed but adding it anyways
                UserModel.ADcache.DE.Close();
            }
            catch (UnauthorizedAccessException e)
            {
                return Error(e.Message);
            }

            return Success();
        }

        public ActionResult ToggleUserprofile([FromBody]string username)
        {
            UserModel = getUserModel(username);
            //XXX log what the new value of profile is :)
            logger.Info("User {0} toggled romaing profile for user  {1}", ControllerContext.HttpContext.User.Identity.Name, UserModel.adpath);

            //string profilepath = (string)(ADcache.DE.Properties["profilePath"])[0];

            try
            {
                if (UserModel.ADcache.DE.Properties.Contains("profilepath"))
                {
                    UserModel.ADcache.DE.Properties["profilePath"].Clear();
                    UserModel.ADcache.DE.CommitChanges();
                }
                else
                {
                    string upn = UserModel.UserPrincipalName;
                    var tmp = upn.Split('@');

                    string path = string.Format("\\\\{0}\\profiles\\{1}", tmp[1], tmp[0]);

                    UserModel.ADcache.DE.Properties["profilePath"].Add(path);
                    UserModel.ADcache.DE.CommitChanges();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return Error(e.Message);
            }

            //Set value
            //DirectoryEntry de = result.GetDirectoryEntry();
            //de.Properties["TelephoneNumber"].Clear();
            //de.Properties["employeeNumber"].Value = "123456789";
            //de.CommitChanges();

            return Success();
        }

        [HttpPost]
        public void CreateNewIRSR(string displayname)
        {
            Response.Redirect("/CreateWorkItem?userDisplayName=" + displayname);
        }

        public ActionResult Error(string message = "Error")
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new { success = false, errorMessage = message });
        }

        public ActionResult Success(string Message = "Success")
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new { success = true, message = Message });
        }

        private void LoadWarnings()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new UserDisabled(this),
                new UserLockedDiv(this),
                new PasswordExpired(this),
                new MissingAAUAttr(this),
                new NotStandardOU(this)
            };

            var errorList = new WebMgmtErrorList(errors);
            UserModel.ErrorCountMessage = errorList.getErrorCountMessage();
            UserModel.ErrorMessages = errorList.ErrorMessages;

            if (this.userIsInRightOU())
            {
                UserModel.ShowFixUserOU = false;
            }
            //Password is expired and warning before expire (same timeline as windows displays warning)
        }

        public override ActionResult LoadTab(string tabName, string name)
        {
            UserModel = getUserModel(name);

            PartialGroupModel model = null;

            string viewName = tabName;
            switch (tabName)
            {
                case "basicinfo":
                    viewName = "BasicInfo";
                    UserModel = BasicInfo.Init(UserModel, HttpContext);
                    break;
                case "groups":
                    viewName = "Groups";
                    model = new PartialGroupModel(UserModel.ADcache, "memberOf");
                    break;
                case "tasks":
                    viewName = "Tasks";
                    break;
                case "warnings":
                    viewName = "Warnings";
                    break;
                case "fileshares":
                    viewName = "Fileshares";
                    model = new PartialGroupModel(UserModel.ADcache, "memberOf");
                    model = Fileshares.Init(model);
                    break;
                case "calAgenda":
                    viewName = "CalendarAgenda";
                    UserModel = CalendarAgenda.Init(UserModel);
                    break;
                case "exchange":
                    viewName = "Exchange";
                    model = new PartialGroupModel(UserModel.ADcache, "memberOf");
                    model = Exchange.Init(model);
                    break;
                case "servicemanager":
                    viewName = "ServiceManager";
                    break;
                case "computerInformation":
                    viewName = "ComputerInformation";
                    UserModel = ComputerInformation.Init(UserModel);
                    break;
                case "loginscript":
                    viewName = "Loginscript";
                    UserModel = LoginScript.Init(UserModel);
                    break;
                case "print":
                    viewName = "Print";
                    UserModel.Print = new PrintConnector(UserModel.Guid.ToString()).doStuff();
                    break;
                case "rawdata":
                    viewName = "Raw";
                    UserModel.Rawdata = TableGenerator.buildRawTable(UserModel.ADcache.getAllProperties());
                    break;
            }
            return model != null ? PartialView(viewName, model) : PartialView(viewName, UserModel);
        }
    }
}
