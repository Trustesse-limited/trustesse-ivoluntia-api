using System;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UserRepository(iVoluntiaDataContext context) : RepositoryBase<User>(context), IUserRepository
{
    public async Task<User?> GetUserByEmailWithFoundationAsync(string email, CancellationToken cancellationToken)
    {
        return await FindByCondition(
            x => x.Email.Trim().ToLower() == email.Trim().ToLower(), false)
            .Include(x => x.Foundation)
            .Include(x => x.OnboardingProgress)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
