using Trustesse.Ivoluntia.Commons.DTOs.Qualification;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IQualificationService
{
    Task<GlobalRequestReponse<QualificationDto>> CreateQualification(CreateQualificationDto request);
    Task<GlobalRequestReponse<QualificationDto>> UpdateQualification(string qualificationId, UpdateQualificationDto request);
}
