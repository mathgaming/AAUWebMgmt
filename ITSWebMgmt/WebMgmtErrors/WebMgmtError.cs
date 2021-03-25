using System.Threading.Tasks;

namespace ITSWebMgmt.WebMgmtErrors
{
    public enum Severity { Error, Warning, Info}

    public abstract class WebMgmtError
    {
        public string Heading { get; set; }
        public string Description { get; set; }
        public async virtual Task<bool> HaveErrorAsync() => false;
        public virtual bool HaveError() => false;
        public Severity Severeness { get; set; }
    }
}
