using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IVolunteerService
    {
        Task<GlobalRequestReponse<IEnumerable<VolunteerDto>>> GetVolunteers(string foundationId, bool? isActive);
    }
}
