namespace ITSWebMgmt.Models
{
    public class RawHTMLModel
    {
        public RawHTMLModel(string title, string html)
        {
            Title = title;
            HTML = html;
        }
        public string Title { get; set; }
        public string HTML { get; set; }
    }
}
