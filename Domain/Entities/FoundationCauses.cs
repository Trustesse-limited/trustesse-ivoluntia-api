using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class FoundationCauses
    {
        public string CauseId { get; set; } 
        public Cause Cause { get; set; }    
        public string FoundationId { get; set; }   
        public Foundation Foundation { get; set; }
        public DateTime DateCreated { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeprecated { get; set; }
    }
}
