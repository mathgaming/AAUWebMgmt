using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ErrorCode
    {
        public int Id { get; set; }
        public string ErrorCodeName { get; set; }
        public string Description { get; set; }
        public string OneNoteLink { get; set; }
    }
}
