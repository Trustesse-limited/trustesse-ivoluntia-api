using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<ApiResponse<List<Foundation>>> GetOrganization(GetOrganizationDto getOrganizationDto);
        Task<ApiResponse<Foundation>> GetOrganizationById(string id);
    }
}
