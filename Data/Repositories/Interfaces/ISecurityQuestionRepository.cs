using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface ISecurityQuestionRepository
    {
        Task<SecurityQuestion> AddSecurityQuestion(SecurityQuestion data);
        IQueryable<SecurityQuestion> GetSecurityQuestions();
    }
}
