using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class OrganizationDeclineStatus : BaseEntity
    {
        public string ModifiedBy { get; set; }  
        public DateTime ModifiedDate { get; set; }
        public string Reason { get; set; }  
        public string Status { get; set; }
        public string FoundationId { get; set; }
        public Foundation Foundation { get; set; }
    }
}
