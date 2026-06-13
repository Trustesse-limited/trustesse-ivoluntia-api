using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class GetOrganizationDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20; 
        public string? Status { get; set; }
        public bool All { get; set; } = false;   
    }
}
