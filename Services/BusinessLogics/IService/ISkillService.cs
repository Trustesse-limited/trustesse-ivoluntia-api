using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ISkillService
    {
        Task<GlobalRequestReponse<string>> CreateSkill(CreateSkillRequestDto createSkillDto);
        Task<GlobalRequestReponse<List<GetSkillResponseDto>>> GetSkill();
        Task<GlobalRequestReponse<GetSkillResponseDto>> GetSkillById(string id);
        Task<GlobalRequestReponse<string>> DeleteSkill(string id);
    }
}
