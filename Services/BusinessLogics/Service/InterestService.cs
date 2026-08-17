using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class InterestService : IInterestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public InterestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GlobalRequestReponse<string>> CreateInterest(CreateInterestRequestDto createInterestDto)
        {
            var interest = await _unitOfWork.InterestRepository.GetByExpressionAsync(i => i.Name == createInterestDto.Name);
            if(interest != null)
                return ResponseHelper.BuildResponse<string>("interest exist", StatusCodes.Status400BadRequest, null, false);
            var mapInterest = _mapper.Map<Interest>(createInterestDto);
            await _unitOfWork.InterestRepository.AddAsync(mapInterest);  
            var response = await _unitOfWork.CompleteAsync(); 
            if(response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "skill created", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<List<GetInterestResponseDto>>> GetInterest()
        {
            var interest = await _unitOfWork.InterestRepository.GetAsync();
            var mapInterest = _mapper.Map<List<GetInterestResponseDto>>(interest);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapInterest, true);
        }
        public async Task<GlobalRequestReponse<GetInterestResponseDto>> GetInterestById(string id)
        {
            if(id == null)
                return ResponseHelper.BuildResponse<GetInterestResponseDto>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var interest = await _unitOfWork.InterestRepository.GetByIdAsync(id);
            if(interest == null)
                return ResponseHelper.BuildResponse<GetInterestResponseDto>("interest not found", StatusCodes.Status404NotFound, null, false);
            var mapInterest = _mapper.Map<GetInterestResponseDto>(interest);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapInterest, true);
        }
        public async Task<GlobalRequestReponse<string>> DeleteInterest(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<string>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var interest = await _unitOfWork.InterestRepository.GetByIdAsync(id);
            if (interest == null)
                return ResponseHelper.BuildResponse<string>("interest not found", StatusCodes.Status404NotFound, null, false);
            await _unitOfWork.InterestRepository.DeleteAsync(interest);
            var response = await _unitOfWork.CompleteAsync(); 
            if(response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "interest deleted", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
