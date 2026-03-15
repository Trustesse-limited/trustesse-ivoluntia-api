using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class ProgramRejectionReason: BaseEntity
    {
        public string ProgramId { get; set; }
        public Program Program {  get; set; }      
        public string QueriedMessage { get; set; }
        public string QueriedBy { get; set; }
        public string QueriedByFullName { get; set; }
    }
}
