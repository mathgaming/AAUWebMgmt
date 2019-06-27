using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSWebMgmt.ViewInitialisers.User
{
    public static class BasicInfo
    {
        public static UserModel Init(UserModel Model, HttpContext context)
        {
            //lblbasicInfoOfficePDS
            if (Model.AAUStaffID != null)
            {
                string empID = Model.AAUStaffID;

                var pds = new PureConnector(empID);
                Model.BasicInfoDepartmentPDS = pds.Department;
                Model.BasicInfoOfficePDS = pds.OfficeAddress;
            }

            //Other fileds
            var attrDisplayName = "UserName, AAU-ID, AAU-UUID, UserStatus, StaffID, StudentID, UserClassification, Telephone, LastLogon (approx.)";
            var attrArry = Model.getUserInfo();
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
            string email = "";
            foreach (string s in Model.ProxyAddresses)
            {
                if (s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase))
                {
                    var tmp2 = s.ToLower().Replace("smtp:", "");
                    email += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", tmp2);
                }
            }
            sb.Append($"<tr><td>E-mails</td><td>{email}</td></tr>");

            const int UF_LOCKOUT = 0x0010;
            int userFlags = Model.UserAccountControlComputed;

            Model.BasicInfoPasswordExpired = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
                Model.BasicInfoPasswordExpired = "True";
            }

            if (Model.UserPasswordExpiryTimeComputed == "")
            {
                Model.BasicInfoPasswordExpireDate = "Never";
            }
            else
            {
                Model.BasicInfoPasswordExpireDate = Model.UserPasswordExpiryTimeComputed;
            }

            Model.UsesOnedrive = OneDriveHelper.doesUserUseOneDrive(context, Model);

            //OneDrive
            sb.Append($"<tr><td>Uses OneDrive?</td><td>{Model.UsesOnedrive}</td></tr>");

            Model.BasicInfoTable = sb.ToString();

            var admdb = new ADMdbConnector();

            string upn = Model.UserPrincipalName;

            string firstName = Model.GivenName;
            string lastName = Model.SN;

            var tmp = upn.Split('@');
            var domain = tmp[1].Split('.')[0];

            //Make lookup in ADMdb
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //basicInfoAdmDBExpireDate.Text = await admdb.loadUserExpiredate(domain, tmp[0], firstName, lastName);
            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine("ADMdb Lookup took: " + watch.ElapsedMilliseconds);

            //Has roaming
            Model.BasicInfoRomaing = "false";
            if (Model.Profilepath != null)
            {
                Model.BasicInfoRomaing = "true";
            }

            //Password Expire date "PasswordExpirationDate"
            return Model;
        }
    }
}
