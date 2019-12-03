using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ServiceManagerModel
    {
        public string userID;
        private List<Case> userCases;
        public List<Case> openCases { get => _getSortedCasesInStatuses("Active", "Pending", "Pending 3rd party", "Awaiting user response", "Pending Group Approval", "New", "Submitted", "In Progress", "On Hold"); }
        public List<Case> closedCases { get => userCases.Except(openCases).ToList(); }


        public ServiceManagerModel(string userID, List<Case> userCases)
        {
            this.userCases = userCases;
            if (userCases == null)
            {
                this.userCases = new List<Case>();
            }
            this.userID = userID;
        }
        private List<Case> _getListOfCasesWithCertainStatus(string statusToSortBy)
        {
            return userCases.Where(x => x.Status.Equals(statusToSortBy)).ToList();
        }
        private List<Case> _getSortedCasesInStatuses(params string[] statuses)
        {
            List<Case> outputList = new List<Case>();
            foreach (string s in statuses)
            {
                outputList = outputList.Concat<Case>(_getListOfCasesWithCertainStatus(s)).ToList();
            }
            return outputList.OrderByDescending(x => x.LastModified).ToList();
        }
    }
    
    public class Case
    {
        private static readonly string idForConvertedToSR = "d283d1f2-5660-d28e-f0a3-225f621394a9";
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Status { get; private set; }
        public DateTime LastModified { get; private set; }
        public bool IsIR { get; private set; }
        public Case(string Id, string Title, string Status, string LastModified)
        {
            this.Id = Id;
            this.Title = Title;
            this.Status = Status;
            this.LastModified = Convert.ToDateTime(LastModified);
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
