using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.Computer
{
    public static class BasicInfo
    {
        public static ComputerModel Init(ComputerModel model)
        {
            //Managed By
            model.ManagedBy = "none";

            if (model.ManagedByAD != null)
            {
                string managerVal = model.ManagedByAD;

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    model.ManagedBy = email;
                }
            }

            model.UsesOnedrive = OneDriveHelper.ComputerUsesOneDrive(model.ADcache);

            if (model.AdminPasswordExpirationTime != null)
            {
                model.ShowResultGetPassword = true;
            }

            return model;
        }
    }
}
