using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ISecurityQuestionService
    {
        Task<ApiResponse<SecurityQuestionDto>> AddSecurityQuestion(CreateSecurityQuestionRequest request);
        Task<ApiResponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions();
        Task<ApiResponse<bool>> RemoveSecurityQuestion(string questionId);
        Task<ApiResponse<string>> RequestSecurityQuestionResetAsync();
        Task<ApiResponse<ResetSecurityQuestionsResponse>> ResetSecurityQuestionsAsync(ResetSecurityQuestionsRequest request);
        Task<ApiResponse<SetupSecurityQuestionsResponse>> SetupSecurityQuestionsAsync(SetupSecurityQuestionsRequest request);
        Task<ApiResponse<ValidateSecurityQuestionsResponse>> ValidateSecurityQuestionsAsync(ValidateSecurityQuestionsRequest request);
    }
}
