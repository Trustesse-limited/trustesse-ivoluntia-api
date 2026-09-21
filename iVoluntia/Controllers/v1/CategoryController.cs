using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Category;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpPost("create-category")]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequestDto createCategoryRequestDto)
          => BuildHttpResponse<string>(await _categoryService.CreateCategory(createCategoryRequestDto));

        [HttpGet("get-all-category")]
        public async Task<IActionResult> GetCause()
           => BuildHttpResponse<List<GetCategoryResponseDto>>(await _categoryService.GetCategory());

        [HttpGet("get-category-by-id")]
        public async Task<IActionResult> GetCauseId([FromQuery] string id)
          => BuildHttpResponse<GetCategoryResponseDto>(await _categoryService.GetCategoryById(id));

        [HttpDelete("delete-category-by-id")]
        public async Task<IActionResult> DeleteCause(string id)
          => BuildHttpResponse<string>(await _categoryService.DeleteCategory(id));
    }
}
