using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;


public abstract class RepositoryBase<T>(iVoluntiaDataContext repositoryContext) : IRepositoryBase<T> where T : class
{


    public void Create(T entity) => repositoryContext.Set<T>().Add(entity);
    public void Update(T entity) => repositoryContext.Set<T>().Update(entity);

    public IQueryable<T> FindAll(bool trackChanges) =>
        !trackChanges ? repositoryContext.Set<T>().AsNoTracking() : repositoryContext.Set<T>();

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) =>
        !trackChanges ? repositoryContext.Set<T>().Where(expression).AsNoTracking() : repositoryContext.Set<T>().Where(expression);

    public void Delete(T entity) => repositoryContext.Set<T>().Remove(entity);

    public virtual async Task<int> BulkUpdateAsync<TProperty>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
            CancellationToken cancellationToken = default)
    {
        return await BulkUpdateAsync(predicate, setPropertyCalls, cancellationToken);
    }

    public virtual async Task<int> BulkUpdateAsync(
            Expression<Func<T, bool>> predicate,
            Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
            CancellationToken cancellationToken = default)
    {
        return await repositoryContext.Set<T>()
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

}
