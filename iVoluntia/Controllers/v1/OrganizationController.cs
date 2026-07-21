using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.GlobalRequest;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Commons.Validators;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : BaseController
    {
        private readonly IOrganizationService _organizationService;
        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        [HttpGet("get")]
        public async Task<IActionResult> GetOrganization([FromQuery] PagedRequestDTO pagedRequestDTO)
            => BuildHttpResponse<List<OrganizationResponseDto>>(await _organizationService.GetOrganization(pagedRequestDTO.Validate()));

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetOrganizationById([FromQuery] string id)
            => BuildHttpResponse<OrganizationResponseDto>(await _organizationService.GetOrganizationByID(id));

        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        [HttpPatch("/organizations/{id}/status")]
        public async Task<IActionResult> OrganizationStatusUpdate([FromBody] UpdateOrganizationStatusDto updateOrganizationStatusDto, string id)
           => BuildHttpResponse<string>(await _organizationService.OrganizationStatusUpdate(updateOrganizationStatusDto.Validate(id), id));
    }
}
