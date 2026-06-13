using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IOrganizationService
    {
        Task<ApiResponse<object>> GetOrganization(GetOrganizationDto getOrganizationDto);
        Task<ApiResponse<OrganizationDto>> GetOrganizationByID(string id);
    }
}
