using Trustesse.Ivoluntia.Commons.DTOs.UserQualification;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IUserQualificationService
{
    Task<GlobalRequestReponse<UserQualificationDto>> AddUserQualification(CreateUserQualificationRequestDto request);
}
