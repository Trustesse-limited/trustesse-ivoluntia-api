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
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class OrganizationService: IOrganizationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly GlobalRequestReponse<List<OrganizationDto>> globalRequestReponse;
        public OrganizationService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<GlobalRequestReponse<List<OrganizationDto>>> GetOrganization(GetOrganizationDto getOrganizationDto)
        {
            var globalRequestReponse = new GlobalRequestReponse<List<OrganizationDto>>();
            if (getOrganizationDto.All == false & getOrganizationDto.Status != null)
            {
                if (getOrganizationDto.PageSize > 20)
                {
                    getOrganizationDto.PageSize = 20;
                }
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(f => f.Status == getOrganizationDto.Status, null, getOrganizationDto.Page, getOrganizationDto.PageSize);
                var organizationDto = _mapper.Map<List<OrganizationDto>>(organization);
                globalRequestReponse.isSuccessfull = true;
                globalRequestReponse.Message = "success";
                globalRequestReponse.ResponseCode = StatusCodes.Status200OK;
                globalRequestReponse.Data = organizationDto;   
                return globalRequestReponse;    
            }
            else if (getOrganizationDto.All == false & getOrganizationDto.Status == null)
            {
                if (getOrganizationDto.PageSize > 20)
                {
                    getOrganizationDto.PageSize = 20;
                }
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(null, null, getOrganizationDto.Page, getOrganizationDto.PageSize);
                var organizationDto = _mapper.Map<List<OrganizationDto>>(organization);
                globalRequestReponse.isSuccessfull = true;
                globalRequestReponse.Message = "success";
                globalRequestReponse.ResponseCode = StatusCodes.Status200OK;
                globalRequestReponse.Data = organizationDto;
                return globalRequestReponse;
            }
            else
            {
                //no pagination 
                var organization = await _unitOfWork.OrganizationRepository.GetAllAsync();
                if (organization != null)
                {
                    var organizationDto = _mapper.Map<List<OrganizationDto>>(organization);
                    globalRequestReponse.isSuccessfull = true;
                    globalRequestReponse.ResponseCode = StatusCodes.Status200OK;
                    globalRequestReponse.Message = "success";
                    globalRequestReponse.Data = organizationDto;
                    return globalRequestReponse;
                }
                else
                {
                    globalRequestReponse.isSuccessfull = false;
                    globalRequestReponse.ResponseCode = StatusCodes.Status400BadRequest;
                    globalRequestReponse.Message = "unsuccessful";
                    return globalRequestReponse;    
                }   
            }                      
        }
        public async Task<GlobalRequestReponse<OrganizationDto>> GetOrganizationByID(string id)
        {
            var globalRequestReponse = new GlobalRequestReponse<OrganizationDto>();
            var response = await _unitOfWork.OrganizationRepository.GetByExpressionAsync(f => f.Id == id);
            if (response != null)
            {
                var organizationDto = _mapper.Map<OrganizationDto>(response);
                globalRequestReponse.isSuccessfull = true;
                globalRequestReponse.Message = "successful";  
                globalRequestReponse.Data = organizationDto;
                globalRequestReponse.ResponseCode = StatusCodes.Status200OK;
                return globalRequestReponse;
            }
            globalRequestReponse.isSuccessfull = false;
            globalRequestReponse.Message = "no user found";
            globalRequestReponse.ResponseCode = StatusCodes.Status404NotFound;
            return globalRequestReponse;
        }
    }
}
