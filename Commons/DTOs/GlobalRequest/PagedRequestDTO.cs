using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.GlobalRequest
{
    public class PagedRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "value must be greater than 0")]
        public int Page { get; set; } = 1;
        [Range(1, 20, ErrorMessage = "value must be between 1 and 20")]
        public int PageSize { get; set; } = 20;
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "status must contain only letters")]
        public string? Status { get; set; }
        public bool All { get; set; } = false;
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "item must contain only letters")]
        public string? SearchQuery { get; set; }
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "item must contain only letters")]
        public string? OrderByColumn { get; set; }
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "item must contain only letters")]
        public string? OrderBy { get; set; } = "ASC";

        public PagedRequestDTO Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;        
        }
    }
}
