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
        public static UserModel Init(UserModel model, HttpContext context)
        {
            //lblbasicInfoOfficePDS
            if (model.AAUStaffID != null)
            {
                string empID = model.AAUStaffID;

                var pds = new PureConnector(empID);
                model.BasicInfoDepartmentPDS = pds.Department;
                model.BasicInfoOfficePDS = pds.OfficeAddress;
            }

            //Other fileds
            var attrDisplayName = "UserName, AAU-ID, AAU-UUID, UserStatus, StaffID, StudentID, UserClassification, Telephone, LastLogon (approx.)";
            var attrArry = model.getUserInfo();
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
            foreach (string s in model.ProxyAddresses)
            {
                if (s.StartsWith("SMTP:", StringComparison.CurrentCultureIgnoreCase))
                {
                    var tmp2 = s.ToLower().Replace("smtp:", "");
                    email += string.Format("<a href=\"mailto:{0}\">{0}</a><br/>", tmp2);
                }
            }
            sb.Append($"<tr><td>E-mails</td><td>{email}</td></tr>");

            const int UF_LOCKOUT = 0x0010;
            int userFlags = model.UserAccountControlComputed;

            model.BasicInfoPasswordExpired = "False";

            if ((userFlags & UF_LOCKOUT) == UF_LOCKOUT)
            {
                model.BasicInfoPasswordExpired = "True";
            }

            if (model.UserPasswordExpiryTimeComputed == "")
            {
                model.BasicInfoPasswordExpireDate = "Never";
            }
            else
            {
                model.BasicInfoPasswordExpireDate = model.UserPasswordExpiryTimeComputed;
            }

            model.UsesOnedrive = OneDriveHelper.doesUserUseOneDrive(context, model);

            //OneDrive
            sb.Append($"<tr><td>Uses OneDrive?</td><td>{model.UsesOnedrive}</td></tr>");

            model.BasicInfoTable = sb.ToString();

            var admdb = new ADMdbConnector();

            string upn = model.UserPrincipalName;

            string firstName = model.GivenName;
            string lastName = model.SN;

            var tmp = upn.Split('@');
            var domain = tmp[1].Split('.')[0];

            //Make lookup in ADMdb
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //basicInfoAdmDBExpireDate.Text = await admdb.loadUserExpiredate(domain, tmp[0], firstName, lastName);
            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine("ADMdb Lookup took: " + watch.ElapsedMilliseconds);

            //Has roaming
            model.BasicInfoRomaing = "false";
            if (model.Profilepath != null)
            {
                model.BasicInfoRomaing = "true";
            }

            //Password Expire date "PasswordExpirationDate"
            return model;
        }
    }
}
