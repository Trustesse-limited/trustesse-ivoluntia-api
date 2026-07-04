using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly iVoluntiaDataContext _dbContext;

    public GenericRepository(iVoluntiaDataContext context)
    {
        _dbContext = context;
    }
    public void Add(T entity)
    {
        _dbContext.Add<T>(entity);
    }
    public async Task AddAsync(T entity)
    {
        await _dbContext.AddAsync<T>(entity);
    }

    public async Task AddManyAsync(List<T> entities)
    {
        await _dbContext.AddRangeAsync(entities);
    }

    public int Count(Expression<Func<T, bool>> expression)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();
        query = query.Where(expression);
        return query.Count();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();
        query = query.Where(expression);
        return await query.CountAsync();
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await Task.Run(() => _dbContext.Set<T>().Remove(entity));
    }

    public async Task ExecuteSqlAsync(string sql, object[] parameters)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public async Task<IQueryable<T>> ExecuteSqlAsync(string sql)
    {
        return await Task.Run(() => _dbContext.Set<T>().FromSqlRaw<T>(sql));
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<List<T>> GetAsync(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, int pageNumber = 1, int pageSize = 20)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        if (orderby != null)
        {
            var ordered = orderby(query);
            if (pageNumber != 0 && pageSize != 0)
            {
                query = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return await query.ToListAsync();
            }
            else
            {
                return await ordered.ToListAsync();
            }
        }
        else
        {
            if (pageNumber != 0 && pageSize != 0)
            {
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            return await query.ToListAsync();
        }
    }

    public async Task<T> GetByExpressionAsync(Expression<Func<T, bool>> expression)
    {
        return await _dbContext.Set<T>().FirstOrDefaultAsync(expression);
    }

    public IQueryable<T> GetByExpression(Expression<Func<T, bool>> expression)
    {
        return _dbContext.Set<T>()
            .AsNoTracking()
            .Where(expression);
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<T> GetEntityWithSpec(ISpecification<T> specification)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync();
    }

    public IQueryable<T> GetQueryable(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, int pageNumber = 0, int pageSize = 0)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsNoTracking();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        if (orderby != null)
        {
            var ordered = orderby(query);
            if (pageNumber != 0 && pageSize != 0)
            {
                query = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
        }
        else
        {
            if (pageNumber != 0 && pageSize != 0)
            {
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
        }

        return query;
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
    {
        return await ApplySpecification(specification).ToListAsync();
    }

    public void Update(T entity)
    {
        _dbContext.Attach<T>(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
    }
    public void UpdateTable(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }
    public async Task UpdateAsync(T entity)
    {
        await Task.Run(() => _dbContext.Attach<T>(entity));
        _dbContext.Entry(entity).State = EntityState.Modified;
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specifications)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), specifications);
    }

    public virtual async Task<int> BulkUpdateAsync(
           Expression<Func<T, bool>> predicate,
           Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
           CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>()
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    public IQueryable<T> SearchAndOrder(
       
        Expression<Func<T, bool>>? filterExpression = null,
        int pageNumber = 1,
        int pageSize = 20,
        string searchQuery = null,
        string orderByColumn = null,
        string orderBy = "ASC")
    {
        IQueryable<T> source = _dbContext.Set<T>().AsNoTracking();
   
        if (filterExpression != null)
        {
            source = source.Where(filterExpression);
        }
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? orExpression = null;

            foreach (var prop in typeof(T).GetProperties()
           .Where(p => p.PropertyType == typeof(string)))
            {
                var propertyAccess = Expression.Property(parameter, prop);
                var searchConst = Expression.Constant(searchQuery, typeof(string));
                var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                var containsCall = Expression.Call(propertyAccess, containsMethod, searchConst);
                var andExpression = Expression.AndAlso(notNull, containsCall);

                orExpression = orExpression == null
                    ? andExpression
                    : Expression.OrElse(orExpression, andExpression);
            }

            if (orExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
                source = source.Where(lambda);
            }
        }
        if (!string.IsNullOrWhiteSpace(orderByColumn))
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(orderByColumn,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var propertyAccess = Expression.Property(parameter, property);
            var orderByLambda = Expression.Lambda(propertyAccess, parameter);

            string orderDirection = string.Equals(orderBy, "DESC", StringComparison.OrdinalIgnoreCase)
                ? "OrderByDescending"
                : "OrderBy";

            var resultExp = Expression.Call(
                typeof(Queryable),
                orderDirection,
                new Type[] { typeof(T), property.PropertyType },
                source.Expression,
                Expression.Quote(orderByLambda));

            source = source.Provider.CreateQuery<T>(resultExp);
        }
        source = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return source;
    }
}