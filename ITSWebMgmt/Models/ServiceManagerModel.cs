using System;
using System.Collections.Generic;
using System.Linq;

namespace ITSWebMgmt.Models
{
    public class ServiceManagerModel
    {
        public string userID;
        private readonly List<Case> userCases;
        public List<Case> OpenCases { get => GetSortedCasesInStatuses("Active", "Pending", "Pending 3rd party", "Awaiting user response", "Pending Group Approval", "New", "Submitted", "In Progress", "On Hold"); }
        public List<Case> ClosedCases { get => userCases.Except(OpenCases).ToList(); }

        public ServiceManagerModel(string userID, List<Case> userCases)
        {
            this.userCases = userCases;
            if (userCases == null)
            {
                this.userCases = new List<Case>();
            }
            this.userID = userID;
        }
        private List<Case> GetListOfCasesWithCertainStatus(string statusToSortBy)
        {
            return userCases.Where(x => x.Status.Equals(statusToSortBy)).ToList();
        }
        private List<Case> GetSortedCasesInStatuses(params string[] statuses)
        {
            List<Case> outputList = new List<Case>();
            foreach (string s in statuses)
            {
                outputList = outputList.Concat<Case>(GetListOfCasesWithCertainStatus(s)).ToList();
            }
            return outputList.OrderByDescending(x => x.LastModified).ToList();
        }
    }
    
    public class Case
    {
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
