namespace ITSWebMgmt.Models
{
    public class KnownIssue
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string OneNoteLink { get; set; }
        public string CaseLink { get; set; }
        public bool Active { get; set; } = true;
    }
}
