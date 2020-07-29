namespace ITSWebMgmt.Models
{
    public class PrintModel
    {
        public bool Found { get; set; }
        public string AAUCardXerox { get; set; }
        public string AAUCardKonica { get; set; }
        public string DepartmentThing { get; set; }
        public string DepartmentName { get; set; }
        public string EquitracDisabled { get; set; } = "";
        public string ConectionError { get; set; }
        public string CredentialError { get; set; }
        public decimal Free { get; set; }
        public decimal Balance { get; set; }
        public decimal Paid => Balance - Free;
    }
}
