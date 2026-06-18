using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IOrganizationService
    {
        Task<GlobalRequestReponse<List<OrganizationDto>>> GetOrganization(GetOrganizationDto getOrganizationDto);
     
        Task<GlobalRequestReponse<OrganizationDto>> GetOrganizationByID(string id);
    }
}
