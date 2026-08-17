using Microsoft.AspNetCore.Http;

namespace Trustesse.Ivoluntia.Commons.DTOs.UserQualification;

public class CreateUserQualificationRequestDto
{
    public string QualificationTypeId { get; set; }
    public IFormFile ProofOfQualification { get; set; }
}

public class UserQualificationDto
{
    public string Id { get; set; }
    public string QualificationTypeId { get; set; }
    public string ProofOfQualificationURL { get; set; }
}
