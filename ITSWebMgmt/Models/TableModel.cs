using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class TableModel
    {
        public string[] Headings { get; set; }
        public List<string[]> Rows { get; set; }
        public string ErrorMessage { get; set; }
        public string ViewHeading { get; set; }

        public TableModel(string[] headings, List<string[]> rows, string viewHeading = null)
        {
            Headings = headings;
            Rows = rows;
            ViewHeading = viewHeading;
        }

        public TableModel(string errorMessage, string viewHeading = null)
        {
            ErrorMessage = errorMessage;
            ViewHeading = viewHeading;
        }
    }
}
