using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class DefendpointChallengeResponseModel
    {
        public string Data { get; set; }
        public DefendpointChallengeResponseModel(string result)
        {
            Data = result;
        }
    }
}
