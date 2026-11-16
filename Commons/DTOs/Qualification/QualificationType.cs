using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Qualification
{
    public class QualificationType
    {
        public List<string> Title{  get; set; }  = new List<string>();
        public List<string> ProofOfQualificationURL { get; set; } = new List<string>(); 
    }
}
