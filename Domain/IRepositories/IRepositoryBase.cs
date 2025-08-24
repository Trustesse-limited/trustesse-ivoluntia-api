using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IRepositoryBase<T>
{
    IQueryable<T> FindAll(bool trackChanges);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task<int> BulkUpdateAsync(
          Expression<Func<T, bool>> predicate,
          Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
          CancellationToken cancellationToken = default);
}
