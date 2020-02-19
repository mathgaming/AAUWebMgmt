namespace ITSWebMgmt.Models
{
    public class CreateWorkItemModel
    {
        public string UserID { get; set; }
        public string AffectedUser { get; set; }
        public string Title { get; set; }
        public string SupportGroup { get; set; }
        public string Description { get; set; }
        public bool IsFeedback { get; set; }
    }
}
