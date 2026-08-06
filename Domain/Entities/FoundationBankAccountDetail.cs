using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class FoundationBankAccountDetail : BaseEntity
    {
        public Foundation Foundation { get; set; }  
        public string FoundationId { get; set; }    
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
    }
}
