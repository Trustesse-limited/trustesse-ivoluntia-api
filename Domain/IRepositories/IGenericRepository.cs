using System.Linq.Expressions;

namespace Trustesse.Ivoluntia.Domain.IRepositories;
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool isPagingEnabled { get; }
}
public interface IGenericRepository<T> where T : class  
{
    Task<IReadOnlyList<T>> GetAllAsync();

    Task<T> GetByIdAsync(Guid id);

    Task<T> GetByExpressionAsync(Expression<Func<T, bool>> expression);

    Task<List<T>> GetAsync(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null,
        int pageNumber = 0, int pageSize = 0);

    Task ExecuteSqlAsync(string sql, object[] parameters);

    Task<IQueryable<T>> ExecuteSqlAsync(string sql);

    IQueryable<T> GetQueryable(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null,
        int pageNumber = 0, int pageSize = 0);

    Task<T> GetEntityWithSpec(ISpecification<T> specification);

    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);

    Task<int> CountAsync(Expression<Func<T, bool>> expression);

    Task DeleteAsync(T entity);

    Task UpdateAsync(T entity);

    Task AddAsync(T entity);
    Task AddManyAsync(List<T> entities);

    void Add(T entity);

    void Update(T entity);

    void Delete(T entity);

    int Count(Expression<Func<T, bool>> expression);
}