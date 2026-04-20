using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class UserProgram
    {
        public string ProgramId { get; set; } 
        public Program Program { get; set; }    
        public string UserId { get; set; }
        public User User { get; set; } 
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }       
    }
}
