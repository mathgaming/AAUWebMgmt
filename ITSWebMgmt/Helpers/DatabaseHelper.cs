using ITSWebMgmt.Models;
using ITSWebMgmt.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private WebMgmtContext _context;
        public DatabaseHelper(WebMgmtContext context)
        {
            _context = context;
        }

        public TrashRequest GetTrashRequest(string computerName)
        {
            if (_context != null)
            {
                var temp = _context.TrashRequests.FirstOrDefault(x => x.ComputerName == computerName);
                return temp;
            }

            return null;
        }
    }

    public interface IDatabaseHelper
    {
        public TrashRequest GetTrashRequest(string computerName);
    }
}
