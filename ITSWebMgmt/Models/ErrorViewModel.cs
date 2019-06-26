using Microsoft.AspNetCore.Diagnostics;
using System;

namespace ITSWebMgmt.Models
{
    public class ErrorViewModel
    {
        public Exception Error { get; set; }
        public string AffectedUser { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string Url { get => Path + QueryString; }
        public string ErrorMessage { get => Error.Message; }
        public string Stacktrace { get => Error.StackTrace; }
    }
}