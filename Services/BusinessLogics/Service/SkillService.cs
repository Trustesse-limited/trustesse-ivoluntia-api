using MapsterMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class SkillService: ISkillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SkillService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<GlobalRequestReponse<string>> CreateSkill(CreateSkillRequestDto createSkillDto)
        {
            var skill = _unitOfWork.skillRepo.GetByExpressionAsync(s => s.Name == createSkillDto.Name);
            if(skill != null)
                return ResponseHelper.BuildResponse<string>("skill already exist", StatusCodes.Status400BadRequest, null, false);
            var mapSkill = _mapper.Map<Skill>(createSkillDto);
            await _unitOfWork.skillRepo.AddAsync(mapSkill);
            var response = await _unitOfWork.CompleteAsync(); 
            if(response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "skill created", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<List<GetSkillResponseDto>>> GetSkill()
        {
            var skill = await _unitOfWork.skillRepo.GetAsync();
            var mapSkill = _mapper.Map<List<GetSkillResponseDto>>(skill);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapSkill, true);
        }
        
        public async Task<GlobalRequestReponse<GetSkillResponseDto>> GetSkillById(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<GetSkillResponseDto>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var skill = await _unitOfWork.skillRepo.GetByIdAsync(id);
            if(skill == null)
                return ResponseHelper.BuildResponse<GetSkillResponseDto>("skill not found", StatusCodes.Status404NotFound, null, false);
            var mapSkill = _mapper.Map<GetSkillResponseDto>(skill);
            return ResponseHelper.BuildResponse<GetSkillResponseDto>("success", StatusCodes.Status200OK, mapSkill, true);
        }
        public async Task<GlobalRequestReponse<string>> DeleteSkill(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<string> ("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var skill = await _unitOfWork.skillRepo.GetByIdAsync(id);
            if(skill == null)
                return ResponseHelper.BuildResponse<string>("skill not found", StatusCodes.Status404NotFound, null, false);
            await _unitOfWork.skillRepo.DeleteAsync(skill); 
            var response = await _unitOfWork.CompleteAsync(); 
            if(response > 0)
                return ResponseHelper.BuildResponse<string>("success", StatusCodes.Status200OK, "skill deleted", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
