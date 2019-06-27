using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.Computer
{
    public static class BasicInfo
    {
        public static ComputerModel Init(ComputerModel Model)
        {
            //Managed By
            Model.ManagedBy = "none";

            if (Model.ManagedByAD != null)
            {
                string managerVal = Model.ManagedByAD;

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    Model.ManagedBy = email;
                }
            }

            if (Model.AdminPasswordExpirationTime != null)
            {
                Model.ShowResultGetPassword = true;
            }

            return Model;
        }
    }
}
