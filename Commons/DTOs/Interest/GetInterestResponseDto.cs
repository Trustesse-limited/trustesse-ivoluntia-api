using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Interest
{
    public class GetInterestResponseDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public GetInterestResponseDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }
    }
}
