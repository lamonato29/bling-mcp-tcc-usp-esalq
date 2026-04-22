namespace Bling.Application.Common;

/// <summary>
/// Interface base para Use Cases
/// </summary>
public interface IUseCase<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input);
}

/// <summary>
/// Interface para Use Cases sem input
/// </summary>
public interface IUseCase<TOutput>
{
    Task<TOutput> ExecuteAsync();
}

/// <summary>
/// Resultado paginado
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore => Page * PageSize < TotalItems;
}
