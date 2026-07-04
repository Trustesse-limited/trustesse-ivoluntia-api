using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class OrganizationResponseDto
    {
        public string Id { get; set; }  
        public string Name { get; set; }
        public string Mission { get; set; }
        public string? Logo { get; set; }
        public string? Website { get; set; }
        public string Email { get; set; }
        public DateTime YearEstablished { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; } 
    }
}
