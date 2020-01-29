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

        public TableModel(string[] headings, List<string[]> rows)
        {
            Headings = headings;
            Rows = rows;
        }
    }
}
