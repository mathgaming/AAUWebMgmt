using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ServiceManagerModel
    {
        public string userID;
        public List<Case> userCases;

        public ServiceManagerModel(string userID, List<Case> userCases)
        {
            this.userCases = userCases;
            this.userID = userID;
        }
    }
    
    public class Case
    {
        static readonly string idForConvertedToSR = "d283d1f2-5660-d28e-f0a3-225f621394a9";
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Status { get; private set; }
        public string LastModified { get; private set; }
        public bool IsIR { get; private set; }
        public Case(string Id, string Title, string Status, string LastModified)
        {
            this.Id = Id;
            this.Title = Title;
            this.Status = Status;
            this.LastModified = Convert.ToDateTime(LastModified).ToString("yyyy-MM-dd HH:mm");
            if (Id.StartsWith("IR"))
            {
                IsIR = true;
            }
            else
            {
                IsIR = false;
            }
        }
    }
}
