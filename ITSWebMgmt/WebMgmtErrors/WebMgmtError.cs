using ITSWebMgmt.Controllers;
using ITSWebMgmt.Caches;
using System.Management;
using System;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;

namespace ITSWebMgmt.WebMgmtErrors
{
    public static class Severity
    {
        public const int Info = 2;
        public const int Warning = 1;
        public const int Error = 0;
    }

    public abstract class WebMgmtError
    {
        public string Heading;
        public string Description;
        public abstract bool HaveError();
        public int Severeness;
    }

    public abstract class UserWebMgmtError : WebMgmtError
    {
        protected UserController user;

        public UserWebMgmtError(UserController user)
        {
            this.user = user;
        }
    }

    public abstract class ComputerWebMgmtError : WebMgmtError
    {
        protected ComputerController computer;

        public ComputerWebMgmtError(ComputerController computer)
        {
            this.computer = computer;
        }
    }
    public class MissingDataFromSCCM : ComputerWebMgmtError
    {
        public MissingDataFromSCCM(ComputerController computer) : base(computer)
        {
            Heading = "Computer missing data from SCCM";
            Description = "Some infomation cannot be shown due to missing information from SCCM";
            Severeness = Severity.Warning;
        }

        public override bool HaveError() => computer.ComputerModel.Windows.LogicalDisk.Count == 0;
    }

    public class DriveAlmostFull : ComputerWebMgmtError
    {
        public DriveAlmostFull(ComputerController computer) : base(computer)
        {
            Heading = "Less than 5 GB space avilable";
            Description = "Having an almost full drive might cause troubles";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            if (computer.ComputerModel.Windows.LogicalDisk.Count != 0)
            {
                int space = computer.ComputerModel.Windows.LogicalDisk.GetPropertyInGB("FreeSpace");
                if (space == 0) return false;
                return space <= 5;
            }

            return false;
        }
    }

    public class ManagerAndComputerNotInSameDomain : ComputerWebMgmtError
    {
        public ManagerAndComputerNotInSameDomain(ComputerController computer) : base(computer)
        {
            Heading = "The computer and the user in \"managed by\" are in different domains";
            Description = "This may or may not be an error, but user cannot have local administrator rights until this is corrected";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            ComputerModel compModel = computer.ComputerModel;
            if (compModel.IsWindows) {
                WindowsComputerModel winModel = compModel.Windows;
                string managerDomain = compModel.Windows.ManagedBy.Split('@')[1];
                //We are only interested in the bit after @, as that is the actual domain.
                //I am sure there is a prettier way of doing this, but this works too.
                //In case you don't know what is happening, I am using @ as a delimiter for the mail of the manager,
                //Which results in two indexes in the resulting array. As the domain is in the second index, we index by 1 to get it.
                return !winModel.Domain.Equals(managerDomain);
            }
            else
            {
                return false;
            }
        }
    }

    public class MissingPCConfig : ComputerWebMgmtError
    {
        public MissingPCConfig(ComputerController computer) : base(computer)
        {
            Heading = "The PC is missing config";
            Description = @"<p>The computer is not in the Administrativ10 PC or AAU10 PC group. Plase add it to one of them</p>
                            <p>It can take over 30 secounds before you get responce, that says if it succesfully was added, please be patient</p>
                            <button id=""AddToAdministrativ10"">Add computer to Administrativ10 PC</button>
                            <br />
                            <br />
                            <button id=""AddToAAU10"">Add computer to AAU10 PC</button>
                            <script>
                                $(""#AddToAdministrativ10"").click(function ()
                                {
                                    sendPostRequest(""AddToAdministrativ10"")
                                });

                                $(""#AddToAAU10"").click(function ()
                                {
                                    sendPostRequest(""AddToAAU10"")
                                });
                            </script>";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            DateTime temp = computer.ComputerModel.Windows.WhenCreated;
            return (computer.ComputerModel.Windows.ConfigPC == "Unknown" && computer.ComputerModel.Windows.WhenCreated > DateTime.Parse("2019-01-01"));
        }
    }

    public class NotStandardOU : UserWebMgmtError
    {
        public NotStandardOU(UserController user) : base(user)
        {
            Heading = "User is in a non standard OU";
            Description = "This might not be a problem. User can be affected by non-stadard group policy. User can be a service user or admin account.";
            Severeness = Severity.Warning;
        }

        public override bool HaveError() => !user.userIsInRightOU();
    }

    public class NotStandardComputerOU : ComputerWebMgmtError
    {
        public NotStandardComputerOU(ComputerController computer) : base(computer)
        {
            Heading = "Computer is in a wrong OU";
            Description = "The computer is getting wroung GPO settings. Fix by using task \"Move computer to OU Clients.\" ";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => !computer.computerIsInRightOU(computer.ComputerModel.Windows.DistinguishedName);
    }

    public class MissingAAUAttr : UserWebMgmtError
    {
        public MissingAAUAttr(UserController user) : base(user)
        {
            Heading = "User is missing AAU Attributes";
            Description = "The user is missing one or more of the AAU attributes. The user will not be able to login via login.aau.dk. Check CPR is correct in ADMdb.";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => user.UserModel.AAUUserClassification == null || user.UserModel.AAUUserStatus == null || (user.UserModel.AAUStaffID == null && user.UserModel.AAUStudentID == null);
    }

    public class AccountLocked : UserWebMgmtError
    {
        public AccountLocked(UserController user) : base(user)
        {
            Heading = "Account locked";
            Description = "The user account is locked, maybe due to too many failed password attempts.";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            const int UF_LOCKOUT = 0x0010;

            int userFlags = (int)user.UserModel.UserAccountControlComputed;

            return (userFlags & UF_LOCKOUT) == UF_LOCKOUT;
        }
    }
    public class ADFSLocked : UserWebMgmtError
    {
        public ADFSLocked(UserController user) : base(user)
        {
            Heading = "ADFS locked";
            Description = "The user account is ADFS locked, maybe due to too many failed password attempts.";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            return user.UserModel.BasicInfoADFSLocked == true.ToString();
        }
    }

    public class UserLockedDiv : UserWebMgmtError
    {
        public UserLockedDiv(UserController user) : base(user)
        {
            Heading = "User account is locked";
            Description = "The user account is locked, used tasks unlock account to unlock it.";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => user.UserModel.IsAccountLocked == true;
    }


    public class UserDisabled : UserWebMgmtError
    {
        public UserDisabled(UserController user) : base(user)
        {
            Heading = "User is diabled";
            Description = "The user is disabled in AD, user can't login. User is expired in AdmDB or disabled by a administrator, see <a href=\"onenote:https://docs.its.aau.dk/Documentation/Info%20til%20Service%20Desk/Disablet%20Users.one#Disabled%20users%20in%20AD&section-id={062F945F-AF8F-4E1C-8151-6C87AA1F134B}&page-id={86CE4A52-90A9-4A5C-A189-9402B9B6153B}&object-id={441C8DED-9C4E-4561-B184-186C63174D6D}&EB\">onenote</a>";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            const int ufAccountDisable = 0x0002;
            return (user.UserModel.UserAccountControl & ufAccountDisable) == ufAccountDisable;
        }
    }
}
