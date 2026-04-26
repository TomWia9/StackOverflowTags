using Domain.Entities;

namespace Domain.Interfaces;

public interface ITagRepository
{
    Task<(IReadOnlyList<Tag> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);
    Task ReplaceAllAsync(IEnumerable<Tag> tags, CancellationToken ct = default);
}
