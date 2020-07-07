namespace ITSWebMgmt.Models
{
    public class ManagedByModel
    {
        public string ADPath { get; set; }
        public string ManagedByADPath { get; set; }
        public string ManagedByDomainAndName { get; set; }
        public ManagedByModel(string ADPath, string managedByPath, string managedByName)
        {
            this.ADPath = ADPath;
            ManagedByADPath = managedByPath;
            ManagedByDomainAndName = managedByName;
        }
    }
}
