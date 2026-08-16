using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Cause
{
    public class GetCauseResponseDto
    {
        public string CauseId { get; set; } 
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
