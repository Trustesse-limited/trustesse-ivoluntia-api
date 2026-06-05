using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IVolunteerService
    {
        Task<ApiResponse<IEnumerable<VolunteerDto>>> GetVolunteers(string foundationId, bool? isActive);
    }
}
