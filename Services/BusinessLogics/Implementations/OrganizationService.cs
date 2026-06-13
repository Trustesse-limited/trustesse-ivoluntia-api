using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.GenericResponse;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class OrganizationService: IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        public OrganizationService(IOrganizationRepository organizationRepository, IMapper mapper)
        {
            _organizationRepository = organizationRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse<object>> GetOrganization(GetOrganizationDto getOrganizationDto)
        {
            var response = await _organizationRepository.GetOrganization(getOrganizationDto);
            if(response.Data == null)
            {
              return ApiResponse<object>.Failure(StatusCodes.Status404NotFound, "Not found");
            }
            if (getOrganizationDto.All == false & getOrganizationDto.Status != null)
            {
               var organizationDto = _mapper.Map<List<OrganizationDto>>(response.Data);
                    // pagesize  limit
               if (getOrganizationDto.PageSize > 20)
               {
                  getOrganizationDto.PageSize = 20;
               }
               var organizationPaginated = PageList<OrganizationDto>.ToPageList(organizationDto, getOrganizationDto.Page, getOrganizationDto.PageSize);
               return ApiResponse<object>.Success($"{response.Message} page:{getOrganizationDto.Page} page size:{getOrganizationDto.PageSize}", organizationPaginated);
            }
            else if (getOrganizationDto.All == false & getOrganizationDto.Status == null)
            {
              var organizationDto = _mapper.Map<List<OrganizationDto>>(response.Data);
              if(getOrganizationDto.PageSize > 20)
              {
                 getOrganizationDto.PageSize = 20;   
              }
              var organizationPaginated = PageList<OrganizationDto>.ToPageList(organizationDto, getOrganizationDto.Page, getOrganizationDto.PageSize);
              return ApiResponse<object>.Success($"{response.Message} page:{getOrganizationDto.Page} page size:{getOrganizationDto.PageSize}", organizationPaginated);
            }
            else
            {
               //no pagination
             var organizationDto = _mapper.Map<List<OrganizationDto>>(response.Data);
             return ApiResponse<object>.Success(response.Message, organizationDto);
            }                      
        }
        public async Task<ApiResponse<OrganizationDto>> GetOrganizationByID(string id)
        {
            var response = await _organizationRepository.GetOrganizationById(id);   
            if (response.StatusCode == StatusCodes.Status200OK)
            {
                var organizationDto = _mapper.Map<OrganizationDto>(response.Data);
                return ApiResponse<OrganizationDto>.Success(response.Message, organizationDto);
            }
            return ApiResponse<OrganizationDto>.Failure(response.StatusCode, response.Message);
        }
    }
}
