using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class JamfGroupModel
    {
    }

    public class AdvancedComputerSearch
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class AdvancedComputerSearchList
    {
        public List<AdvancedComputerSearch> advanced_computer_searches { get; set; }
    }
}
