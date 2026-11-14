using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.UserProfile
{
    public class VolunteerProfileUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public VolunteerProfileUpdateDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }  
    }
}
