using ITSWebMgmt.Controllers;
using ITSWebMgmt.WebMgmtErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public abstract class MacWebMgmtError : WebMgmtError
    {
        public ComputerController computer;
   
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string CaseLink { get; set; }
        public bool Active { get; set; } = true;
        public MacWebMgmtError(ComputerController computer)
        {
            this.computer = computer;
        }
    }

    public class NotAAUMac : MacWebMgmtError
    {
        public NotAAUMac(ComputerController computer) : base(computer)
        {
            Heading = "This is not an AAU-Mac";
            Description = "The computer is not an AAU-Mac";
            Severeness = Severity.Error;
            GroupName = "AAU Mac";
        }

        public override bool HaveError()
        {
            return !computer.ComputerModel.Mac.Groups.Contains(GroupName);
        }
    }

    public class SecurityUpdateAvailable : MacWebMgmtError
    {
        public SecurityUpdateAvailable(ComputerController computer) : base(computer)
        {
            Heading = "There is security updates available";
            Description = "There is security updates available";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            return false;
        }
    }

    public class MissingEmail : MacWebMgmtError
    {
        public MissingEmail(ComputerController computer) : base(computer)
        {
            Heading = "The email is not set";
            Description = "The email is not set";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            return false;
        }
    }

    public class MissingGroup : MacWebMgmtError
    {
        public MissingGroup() : base(null) { }
        public MissingGroup(ComputerController computer) : base(computer)
        {
        }

        public override bool HaveError()
        {
            return computer.ComputerModel.Mac.Groups.Contains(GroupName);
        }
    }
}
