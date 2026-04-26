using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TagRepository(AppDbContext db) : ITagRepository
{
    public async Task<(IReadOnlyList<Tag> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string sortBy, string sortOrder, CancellationToken ct = default)
    {
        var query = db.Tags.AsQueryable();

        query = (sortBy, sortOrder) switch
        {
            ("percentage", "desc") => query.OrderByDescending(t => t.Percentage),
            ("percentage", _) => query.OrderBy(t => t.Percentage),
            ("name", "desc") => query.OrderByDescending(t => t.Name),
            _ => query.OrderBy(t => t.Name),
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<int> CountAsync(CancellationToken ct = default) =>
        db.Tags.CountAsync(ct);

    public async Task ReplaceAllAsync(IEnumerable<Tag> tags, CancellationToken ct = default)
    {
        await db.Tags.ExecuteDeleteAsync(ct);

        // Fresh context state, nothing tracked from before the delete.
        db.ChangeTracker.Clear();

        db.Tags.AddRange(tags);
        await db.SaveChangesAsync(ct);
    }
}