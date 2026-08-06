using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class CreateOrganizationAccountDetailRequestDto
    {
        public string AccountNumber {get; set; }
        public string BankName {get; set; }
        public string BankCode { get; set; }
        public CreateOrganizationAccountDetailRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        } 
    }
}
