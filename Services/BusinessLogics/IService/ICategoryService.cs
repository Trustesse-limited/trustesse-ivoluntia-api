using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Category;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ICategoryService
    {
        Task<GlobalRequestReponse<string>> CreateCategory(CreateCategoryRequestDto createCategoryRequestDto);
        Task<GlobalRequestReponse<List<GetCategoryResponseDto>>> GetCategory();
        Task<GlobalRequestReponse<GetCategoryResponseDto>> GetCategoryById(string id);
        Task<GlobalRequestReponse<string>> DeleteCategory(string id);
    }
}
