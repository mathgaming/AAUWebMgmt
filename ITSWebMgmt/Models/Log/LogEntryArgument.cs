using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models.Log
{
    public class LogEntryArgument
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public LogEntryArgument() { }

        public LogEntryArgument(string argument)
        {
            Value = argument;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
