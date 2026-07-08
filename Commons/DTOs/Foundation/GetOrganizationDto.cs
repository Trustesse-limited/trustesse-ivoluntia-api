using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class GetOrganizationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "value should be greater than 0")]
        public int Page { get; set; } = 1;
        [Range(1, int.MaxValue, ErrorMessage = "value should  be greater than 0")]
        public int PageSize { get; set; } = 20;
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "status should contain only letters")]
        public string? Status { get; set; }
        public bool All { get; set; } = false;   
    }
}
