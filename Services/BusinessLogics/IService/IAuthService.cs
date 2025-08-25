using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthService
{
    Task<CustomResponse> CreateVolunteer(VolunteerSignUpDto model);
}