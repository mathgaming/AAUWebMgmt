namespace ITSWebMgmt.Models.Log
{
    public class LogCountStatisticsModel
    {
        public string Name { get; set; }
        public string FromDate { get; set; }
        public int Count { get; set; }

        public LogCountStatisticsModel(string name, int count, string fromDate = "")
        {
            Name = name;
            Count = count;
            FromDate = fromDate;
        }
    }
}
