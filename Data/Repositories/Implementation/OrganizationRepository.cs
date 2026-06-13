using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class OrganizationRepository: IOrganizationRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public OrganizationRepository(iVoluntiaDataContext iVoluntiaDataContext)
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
        public async Task<ApiResponse<List<Foundation>>> GetOrganization(GetOrganizationDto getOrganizationDto)
        {
            if(getOrganizationDto.Status == null & getOrganizationDto.All == false)
            {
                var response = await _iVoluntiaDataContext.Foundations.ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }
            else if(getOrganizationDto.Status == FoundationStatus.Active.ToString())
            {
                var response = await _iVoluntiaDataContext.Foundations.Where(f => f.Status == "Active").ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }
            else if (getOrganizationDto.Status == FoundationStatus.Pending.ToString())
            {
                var response = await _iVoluntiaDataContext.Foundations.Where(f => f.Status == "Pending").ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }
            else if (getOrganizationDto.Status == FoundationStatus.Decline.ToString())
            {
                var response = await _iVoluntiaDataContext.Foundations.Where(f => f.Status == "Decline").ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }
            else if(getOrganizationDto.Status == FoundationStatus.Block.ToString())
            {
                var response = await _iVoluntiaDataContext.Foundations.Where(f => f.Status == "Block").ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }
            else 
            {
                var response = await _iVoluntiaDataContext.Foundations.ToListAsync();
                return ApiResponse<List<Foundation>>.Success("success", response);
            }  
        }
        public async Task<ApiResponse<Foundation>> GetOrganizationById(string id)
        {
            var organization = await _iVoluntiaDataContext.Foundations.Where(f => f.Id == id).FirstOrDefaultAsync(); 
            if(organization == null)
            {
                return ApiResponse<Foundation>.Failure(StatusCodes.Status404NotFound, "foundation not found");
            }
            return ApiResponse<Foundation>.Success("success", organization);
        }
    }
}
