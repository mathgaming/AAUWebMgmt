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
        public static UserModel Init(UserModel model)
        {
            model.ShowResultDiv = false;

            var loginscripthelper = new Loginscript();

            var script = loginscripthelper.getLoginScript(model.ScriptPath, model.ADcache.Path);

            if (script != null)
            {
                model.ShowResultDiv = true;
                model.Loginscript = loginscripthelper.parseAndDisplayLoginScript(script);
            }

            return model;
        }
    }
}
