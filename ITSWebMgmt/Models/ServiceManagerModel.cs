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

        }
    }
    
    public class Case
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Status { get; private set; }
        public string LastModified { get; private set; }
        public Case(string Id, string Title, string Status, string LastModified)
        {
            this.Id = Id; ;
            this.Title = Title;
            this.Status = Status;
            this.LastModified = Convert.ToDateTime(LastModified).ToString("yyyy-MM-dd HH:mm");
        }
    }
}
