namespace Tawzeef.Domain.Common;

/// <summary>
/// Lives in Domain (not Application) because <see cref="Tawzeef.Domain.Contracts.IBaseRepository{T}"/>
/// returns it, and Domain may not reference Application types.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
