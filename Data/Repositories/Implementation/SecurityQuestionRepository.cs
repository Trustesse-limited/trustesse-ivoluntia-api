using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class SecurityQuestionRepository : ISecurityQuestionRepository
    {
        private readonly iVoluntiaDataContext _context;

        public SecurityQuestionRepository(iVoluntiaDataContext context)
        {
            _context = context;
        }

        public async Task<SecurityQuestion> AddSecurityQuestion(SecurityQuestion data)
        {
            await _context.SecurityQuestions.AddAsync(data);
            return data;
        }

        public IQueryable<SecurityQuestion> GetSecurityQuestions()
        {
            var query = _context.SecurityQuestions.Where(sq => sq.IsActive).AsQueryable();
            return query;
        }
    }
}



