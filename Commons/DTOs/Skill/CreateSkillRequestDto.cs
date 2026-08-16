using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Skill
{
    public class CreateSkillRequestDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        
        public CreateSkillRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }
           

    }
}
