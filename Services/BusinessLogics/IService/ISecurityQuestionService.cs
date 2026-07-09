using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ISecurityQuestionService
    {
        Task<GlobalRequestReponse<SecurityQuestionDto>> AddSecurityQuestion(CreateSecurityQuestionRequest request);
        Task<GlobalRequestReponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions();
        Task<GlobalRequestReponse<bool>> RemoveSecurityQuestion(string questionId);
        Task<GlobalRequestReponse<string>> RequestSecurityQuestionResetAsync();
        Task<GlobalRequestReponse<ResetSecurityQuestionsResponse>> ResetSecurityQuestionsAsync(ResetSecurityQuestionsRequest request);
        Task<GlobalRequestReponse<SetupSecurityQuestionsResponse>> SetupSecurityQuestionsAsync(SetupSecurityQuestionsRequest request);
        Task<GlobalRequestReponse<ValidateSecurityQuestionsResponse>> ValidateSecurityQuestionsAsync(ValidateSecurityQuestionsRequest request);
    }
}
