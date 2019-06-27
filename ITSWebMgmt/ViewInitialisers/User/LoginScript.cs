using ITSWebMgmt.Functions;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class LoginScript
    {
        public static UserModel Init(UserModel Model)
        {
            Model.ShowResultDiv = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(Model.ScriptPath, Model.ADcache.Path);

            if (script != null)
            {
                Model.ShowResultDiv = true;
                Model.Loginscript = loginscripthelper.parseAndDisplayLoginScript(script);
            }

            return Model;
        }
    }
}
