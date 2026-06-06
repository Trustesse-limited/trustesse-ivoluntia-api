namespace Trustesse.Ivoluntia.Domain.Entities;

public class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? CreatedBy { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime? DateUpdated { get; set; }
    public bool IsDeprecated { get; set; }
}

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class BaseQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}