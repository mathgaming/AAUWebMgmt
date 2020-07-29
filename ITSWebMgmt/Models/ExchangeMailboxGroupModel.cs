using System;
using System.Linq;

namespace ITSWebMgmt.Models
{
    public class ExchangeMailboxGroupModel
    {
        public string RawValue { get; }
        public string Type { get; }
        public string Domain { get; }
        public string Name { get; }
        public string Access { get; }

        public ExchangeMailboxGroupModel(string group)
        {
            RawValue = group;
            if (group.StartsWith("CN=MBX_"))
            {
                var ADPathsplit = group.Split(',');
                var nameSplit = ADPathsplit[0].Split('_');

                if (nameSplit.Length == 5)
                {
                    //A normal exchange resource group
                    Type = nameSplit[2];
                    Domain = nameSplit[1];
                    Name = nameSplit[3];
                    Access = nameSplit[4];
                } //XXX: if Length == 4 this a all resources group
                else if (nameSplit.Length == 4)
                {
                    Type = nameSplit[2];
                    Domain = nameSplit[1];
                    Name = nameSplit[2];
                    Access = nameSplit[3];
                }
                else if (nameSplit.Length > 5)
                {
                    var len = nameSplit.Length;
                    Type = nameSplit[2];
                    Domain = nameSplit[1];
                    Name = string.Join("_", nameSplit.Skip(3).Reverse().Skip(1).Reverse());
                    Access = nameSplit[len - 1];
                }
                else
                {
                    Type = "not implemented support for MBX group with less than 4 sections: " + group;
                }
            }
            else
            {
                throw new FormatException("Mbx group must start with \"MBX_\"");
            }
        }

    }
}