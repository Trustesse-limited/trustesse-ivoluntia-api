using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.GenericResponse;
using Trustesse.Ivoluntia.Commons.DTOs.GlobalRequest;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class OrganizationService: IOrganizationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        public OrganizationService(IMapper mapper, IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<GlobalRequestReponse<List<OrganizationResponseDto>>> GetOrganization(PagedRequestDTO pagedRequestDTO)
        {
            if (pagedRequestDTO.All == false && pagedRequestDTO.Status != null)
            {
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(f => f.Status == pagedRequestDTO.Status, null, pagedRequestDTO.Page, pagedRequestDTO.PageSize);
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true); 
            }
            else if (pagedRequestDTO.OrderByColumn != null && pagedRequestDTO.OrderBy != null && pagedRequestDTO.SearchQuery != null)
            {
                var organizations = await _unitOfWork.OrganizationRepository.SearchAndOrder(null, pagedRequestDTO.Page, pagedRequestDTO.PageSize, pagedRequestDTO.SearchQuery, pagedRequestDTO.OrderByColumn, pagedRequestDTO.OrderBy).ToListAsync();
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organizations);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
            }
            else if (pagedRequestDTO.All == false && pagedRequestDTO.Status == null)
            {
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(null, null, pagedRequestDTO.Page, pagedRequestDTO.PageSize);
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
            }
            else
            {
                //no pagination 
                var organization = await _unitOfWork.OrganizationRepository.GetAllAsync();
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if(organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
                return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
            }                      
        }
        public async Task<GlobalRequestReponse<OrganizationResponseDto>> GetOrganizationByID(string id)
        {
            var response = await _unitOfWork.OrganizationRepository.GetByExpressionAsync(f => f.Id == id);
            var organizationResponseDto = _mapper.Map<OrganizationResponseDto>(response);
            if (response != null)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
            return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
        }
    }
}
