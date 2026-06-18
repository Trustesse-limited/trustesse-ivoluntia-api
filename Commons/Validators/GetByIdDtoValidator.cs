using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;

namespace Trustesse.Ivoluntia.Commons.Validators
{
    public class GetByIdDtoValidator: AbstractValidator<GetByIdDto>
    {
        public GetByIdDtoValidator()
        {
            RuleFor(x => x.Id.ToString())   
           .Matches(@"^[A-Za-z0-9-]+$").WithMessage("id should contain only letters, number and -");
        }
    }
}
