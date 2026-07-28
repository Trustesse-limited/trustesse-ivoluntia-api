using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class AccountDetailUpdateHistory: BaseEntity
    {
        public string OrganizationId { get; set; }
        public string CurrentAccountNumber { get; set; }    
        public string? PreviousAccountNumber { get; set; }


    }
}
