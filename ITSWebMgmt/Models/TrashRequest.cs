﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public enum TrashRequestStatus { NotConfirmed, Confirmed};
    public class TrashRequest
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ComputerName { get; set; }
        public string Desciption { get; set; }
        public string Comment { get; set; }
        public string RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public string EquipmentManager { get; set; }
        public string EquipmentManagerEmail { get; set; }
        public TrashRequestStatus Status { get; set; } = TrashRequestStatus.NotConfirmed;
    }
}
