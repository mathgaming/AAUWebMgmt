using ITSWebMgmt.Models;
using System;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class ComputerInformation
    {
        public static UserModel Init(UserModel model)
        {
            try
            {
                var helper = new HTMLTableHelper(new string[] { "Computername", "AAU Fjernsupport" });

                foreach (ComputerModel m in model.getManagedComputers())
                {
                    var name = "<a href=\"/Computer?computername=" + m.ComputerName + "\">" + m.ComputerName + "</a> <br />";
                    var fjernsupport = "<a href=\"https://support.its.aau.dk/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&client.hostname=" + m.ComputerName + "\">Start</a>";
                    helper.AddRow(new string[] { name, fjernsupport });
                }
                model.ComputerInformation = "<h4>Links til computerinfo kan være til maskiner i et forkert domæne, da info omkring computer domæne ikke er tilgængelig i denne søgning</h4>" + helper.GetTable();
            }
            catch (UnauthorizedAccessException e)
            {
                model.ComputerInformation = "Service user does not have SCCM access.";
            }

            return model;
        }
    }
}
