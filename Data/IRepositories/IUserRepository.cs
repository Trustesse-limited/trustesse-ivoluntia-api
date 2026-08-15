using System;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;


public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetUserByEmailWithFoundationAsync(string email, CancellationToken cancellationToken);
}
