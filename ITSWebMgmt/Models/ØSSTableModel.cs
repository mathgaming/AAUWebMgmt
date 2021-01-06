using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class ØSSTableModel
    {
        public TableModel InfoTable { get; set; } = new TableModel("No information found from ØSS");
        public TableModel TransactionTable { get; set; } = new TableModel("No information found from ØSS");
    }
}
