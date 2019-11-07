using ITSWebMgmt.Controllers;
using ITSWebMgmt.Caches;
using System.Management;
using System;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;

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
