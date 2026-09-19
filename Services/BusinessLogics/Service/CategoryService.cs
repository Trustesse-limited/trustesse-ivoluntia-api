using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Category;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<GlobalRequestReponse<string>> CreateCategory(CreateCategoryRequestDto createCategoryRequestDto)
        {
            var cause = _unitOfWork.CategoryRepository.GetByExpressionAsync(s => s.Name == createCategoryRequestDto.Name);
            if (cause != null)
                return ResponseHelper.BuildResponse<string>("category already exist", StatusCodes.Status400BadRequest, null, false);
            var mapCause = _mapper.Map<FoundationCategory>(createCategoryRequestDto);
            await _unitOfWork.CategoryRepository.AddAsync(mapCause);
            var response = await _unitOfWork.CompleteAsync();
            if (response > 0)
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "category created", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<List<GetCategoryResponseDto>>> GetCategory()
        {
            var category = await _unitOfWork.CategoryRepository.GetAsync();
            var mapCategory = _mapper.Map<List<GetCategoryResponseDto>>(category);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, mapCategory, true);
        }

        public async Task<GlobalRequestReponse<GetCategoryResponseDto>> GetCategoryById(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<GetCategoryResponseDto>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return ResponseHelper.BuildResponse<GetCategoryResponseDto>("category not found", StatusCodes.Status404NotFound, null, false);
            var mapCategory = _mapper.Map<GetCategoryResponseDto>(category);
            return ResponseHelper.BuildResponse<GetCategoryResponseDto>("success", StatusCodes.Status200OK, mapCategory, true);
        }
        public async Task<GlobalRequestReponse<string>> DeleteCategory(string id)
        {
            if (id == null)
                return ResponseHelper.BuildResponse<string>("id cannot be null", StatusCodes.Status400BadRequest, null, false);
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return ResponseHelper.BuildResponse<string>("cause not found", StatusCodes.Status404NotFound, null, false);
            await _unitOfWork.CategoryRepository.DeleteAsync(category);
            var response = await _unitOfWork.CompleteAsync();
            if (response > 0)
                return ResponseHelper.BuildResponse<string>("success", StatusCodes.Status200OK, "skill deleted", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
