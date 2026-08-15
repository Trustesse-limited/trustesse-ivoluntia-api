using MapsterMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class CauseService: ICauseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CauseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<GlobalRequestReponse<string>> CreateCause(CreateCauseRequestDto createCauseRequestDto)
        {
            var cause = _unitOfWork.CauseRepository.GetByExpressionAsync(s => s.Name == createCauseRequestDto.Name);
            if (cause != null)
                return ResponseHelper.BuildResponse<string>("cause already exist", StatusCodes.Status400BadRequest, null, false);
            var mapCause = _mapper.Map<Cause>(createCauseRequestDto);
            await _unitOfWork.CauseRepository.AddAsync(mapCause);
            var response = await _unitOfWork.CompleteAsync();
            if (response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "cause created", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<List<GetCauseResponseDto>>> GetCause()
        {
            var causes = await _unitOfWork.CauseRepository.GetAsync();
            var mapCauses = _mapper.Map<List<GetCauseResponseDto>>(causes);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapCauses, true);
        }

        public async Task<GlobalRequestReponse<GetCauseResponseDto>> GetCauseById(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<GetCauseResponseDto>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var cause = await _unitOfWork.CauseRepository.GetByIdAsync(id);
            if (cause == null)
                return ResponseHelper.BuildResponse<GetCauseResponseDto>("cause not found", StatusCodes.Status404NotFound, null, false);
            var mapCause = _mapper.Map<GetCauseResponseDto>(cause);
            return ResponseHelper.BuildResponse<GetCauseResponseDto>("success", StatusCodes.Status200OK, mapCause, true);
        }
        public async Task<GlobalRequestReponse<string>> DeleteCause(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<string>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var cause = await _unitOfWork.CauseRepository.GetByIdAsync(id);
            if (cause == null)
                return ResponseHelper.BuildResponse<string>("cause not found", StatusCodes.Status404NotFound, null, false);
            await _unitOfWork.CauseRepository.DeleteAsync(cause);
            var response = await _unitOfWork.CompleteAsync();
            if (response > 0)
                return ResponseHelper.BuildResponse<string>("success", StatusCodes.Status200OK, "skill deleted", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
