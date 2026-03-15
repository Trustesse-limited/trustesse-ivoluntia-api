using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Program
{
    public class UpdateProgramStatusDto
    {
        public string ProgramId { get; set; }   
        public string Status { get; set; }  
        public string? QueriedComment { get; set; } 
    }
}
