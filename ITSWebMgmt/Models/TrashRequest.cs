using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public enum TrashRequestStatus { NotConfirmed, Confirmed};
    public class TrashRequest
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        [Required]
        [StringLength(9, ErrorMessage = "Must be AAU follows by 5-6 digits.", MinimumLength = 8)]
        public string ComputerName { get; set; }
        public string Desciption { get; set; }
        public string Comment { get; set; }
        [Required]
        [EmailAddress]
        public string RequestedBy { get; set; }
        public string RequestedFor { get; set; }
        public string RequestedForOSSSName { get; set; }
        public string RequestedForOESSStaffID { get; set; }
        public string CreatedBy { get; set; }
        public string EquipmentManager { get; set; }
        [EmailAddress]
        public string EquipmentManagerEmail { get; set; }
        public TrashRequestStatus Status { get; set; } = TrashRequestStatus.NotConfirmed;
    }
}
