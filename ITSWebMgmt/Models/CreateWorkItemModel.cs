namespace ITSWebMgmt.Models
{
    public class CreateWorkItemModel
    {
        public string UserID { get; set; }
        public string AffectedUser { get; set; }
        public string Title { get; set; }
        public string Desription { get; set; }
        public bool IsFeedback { get; set; }
    }
}
