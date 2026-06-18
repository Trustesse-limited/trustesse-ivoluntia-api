using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.Validators;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class GetByIdDto
    {
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "id should contain only letters numbers and -")]
        public string Id { get; set; } 
    }
}
