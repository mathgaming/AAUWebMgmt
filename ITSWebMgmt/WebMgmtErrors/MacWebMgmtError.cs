using ITSWebMgmt.Controllers;
using ITSWebMgmt.WebMgmtErrors;
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

    public class IsTrashedMac : MacWebMgmtError
    {
        public IsTrashedMac(ComputerController computer) : base(computer)
        {
            Heading = "Computer is trashed in ØSS";
            Description = "The computer have been marked as trash in ØSS but was found in Jamf";
            Severeness = Severity.Error;
        }

        public async override Task<bool> HaveErrorAsync()
        {
            return await computer.ComputerModel.IsTrashedInØSSAsync();
        }
    }

    public class IsHalfTrashedMac : ComputerWebMgmtError
    {
        public IsHalfTrashedMac(ComputerController computer) : base(computer)
        {
            Heading = "Computer is trashed in WebMgmt, but not in ØSS";
            Description = "The computer have been marked as trash in WebMgmt but not in ØSS";
            Severeness = Severity.Error;
        }

        public override async Task<bool> HaveErrorAsync()
        {
            return computer.ComputerModel.IsTrashedInWebMgmt() && !await computer.ComputerModel.IsTrashedInØSSAsync();
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
