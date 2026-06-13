using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Validators;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly GetOrganizationDtoValidator _organizationDtoValidator;
        private readonly GetByIdDtoValidator _getByIdDtoValidator;
        public OrganizationController(IOrganizationService organizationService, GetOrganizationDtoValidator organizationDtoValidator, GetByIdDtoValidator getByIdDtoValidator)
        {
            _organizationService = organizationService;
            _organizationDtoValidator = organizationDtoValidator;
            _getByIdDtoValidator = getByIdDtoValidator;
        }
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        [HttpGet("get")]
        public async Task<IActionResult> GetOrganization([FromQuery] GetOrganizationDto getOrganizationDto)
        {
            var result = _organizationDtoValidator.Validate(getOrganizationDto); 
            if (result.IsValid)
            {
                var response = await _organizationService.GetOrganization(getOrganizationDto);
                if(response.StatusCode == StatusCodes.Status200OK)
                    return Ok(response);
                return BadRequest(response);        
            }
            return BadRequest("one or more invalid inputs");
        }
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetOrganizationById([FromQuery] GetByIdDto getByIdDto)
        {
            var result = _getByIdDtoValidator.Validate(getByIdDto);  
            if (result.IsValid)
            {
                var response = await _organizationService.GetOrganizationByID(getByIdDto.Id);  
                if(response.StatusCode == StatusCodes.Status200OK) 
                    return Ok(response);
                return BadRequest(response);
            }
            return BadRequest("one or more invalid inputs");
        }
    }
}
