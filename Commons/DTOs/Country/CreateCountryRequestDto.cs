using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Country
{
    public class CreateCountryRequestDto
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }

        public CreateCountryRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;    
        }
    }
}
