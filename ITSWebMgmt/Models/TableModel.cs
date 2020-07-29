using System.Collections.Generic;

namespace ITSWebMgmt.Models
{
    public class TableModel
    {
        public string[] Headings { get; set; }
        public List<string[]> Rows { get; set; }
        public int[] LinkColumns { get; set; }
        public List<string[]> LinkRows { get; set; }
        public string ErrorMessage { get; set; }
        public string ViewHeading { get; set; }

        public TableModel(string[] headings, List<string[]> rows) : this(headings, rows, null, null, null) { }
        public TableModel(string[] headings, List<string[]> rows, string viewHeading) : this(headings, rows, null, null, viewHeading) { }

        public TableModel(string[] headings, List<string[]> rows, int[] linkColumns = null, List<string[]> linkRows = null, string viewHeading = null)
        {
            Headings = headings;
            Rows = rows;
            ViewHeading = viewHeading;
            LinkColumns = linkColumns;
            LinkRows = linkRows;

            if (LinkRows == null)
            {
                LinkRows = new List<string[]>();
            }
        }

        public TableModel(string errorMessage, string viewHeading = null)
        {
            ErrorMessage = errorMessage;
            ViewHeading = viewHeading;
        }
    }
}
