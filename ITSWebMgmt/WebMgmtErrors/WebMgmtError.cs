namespace ITSWebMgmt.WebMgmtErrors
{
    public enum Severity { Error, Warning, Info}

    public abstract class WebMgmtError
    {
        public string Heading { get; set; }
        public string Description { get; set; }
        public abstract bool HaveError();
        public Severity Severeness { get; set; }
    }
}
