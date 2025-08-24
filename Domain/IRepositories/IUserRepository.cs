using System;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IUserRepository 
{
    Task<User> GetUserByEmailWithFoundationAsync(string email, CancellationToken cancellationToken);
}
