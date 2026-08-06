using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class FoundationBankAccountDetailUpdateHistory: BaseEntity
    {
        public string CurrentAccountNumber { get; set; }
        public string PreviousAccountNumber { get; set; }
        public Foundation Foundation { get; set; }    
        public string FoundationId { get; set; }
    }
}
