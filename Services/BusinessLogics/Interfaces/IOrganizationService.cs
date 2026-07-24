using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.GlobalRequest;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IOrganizationService
    {
        Task<GlobalRequestReponse<List<OrganizationResponseDto>>> GetOrganization(PagedRequestDTO pagedRequestDTO);
        Task<GlobalRequestReponse<OrganizationResponseDto>> GetOrganizationByID(string id);
        Task<GlobalRequestReponse<string>> OrganizationStatusUpdate(UpdateOrganizationStatusDto updateOrganizationStatusDto, string foundationId);
    }
}
