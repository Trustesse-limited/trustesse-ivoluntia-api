using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.UserProfile
{
    public class FoundationProfileUpdateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }

        public FoundationProfileUpdateDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }
    }
}
