using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;

namespace Trustesse.Ivoluntia.Commons.Validators
{
    public class GetOrganizationDtoValidator: AbstractValidator<GetOrganizationDto>
    {
        public GetOrganizationDtoValidator()
        {
            RuleFor(x => x.Status)
            .MaximumLength(128).WithMessage("status is too long")
            .Matches(@"^[A-Za-z]+$").WithMessage("status should contain only letters");
            RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("number must be greater than zero");
            RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("number must be greater than zero");
        }
    }
}
