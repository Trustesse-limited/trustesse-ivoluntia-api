using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class AuditLog:BaseEntity
    {
        public string Event { get; set; }
        public DateTime EventDate { get; set; } = DateTime.Now; 
        public string PerformedBy { get; set; }     
        public string? OldData { get; set; }
        public string? NewData { get; set; } 
    }
}
