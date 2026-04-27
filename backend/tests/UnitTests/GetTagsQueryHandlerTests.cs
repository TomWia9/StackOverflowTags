using Application.Tags.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public class GetTagsQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly GetTagsQueryHandler _sut;

    public GetTagsQueryHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(opts);
        var repo = new TagRepository(_db);
        _sut = new GetTagsQueryHandler(repo);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedResult()
    {
        await SeedAsync(50);

        var result = await _sut.Handle(new GetTagsQuery(1, 10, "name", "asc"), CancellationToken.None);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(50);
        result.TotalPages.Should().Be(5);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SortsByNameAscending()
    {
        await SeedAsync(10);

        var result = await _sut.Handle(new GetTagsQuery(1, 10, "name", "asc"), CancellationToken.None);

        result.Items.Select(t => t.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_SortsByNameDescending()
    {
        await SeedAsync(10);

        var result = await _sut.Handle(new GetTagsQuery(1, 10, "name", "desc"), CancellationToken.None);

        result.Items.Select(t => t.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_SortsByPercentageDescending()
    {
        await SeedAsync(10);

        var result = await _sut.Handle(new GetTagsQuery(1, 10, "percentage", "desc"), CancellationToken.None);

        result.Items.Select(t => t.Percentage).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_ReturnsSecondPage()
    {
        await SeedAsync(30);

        var page1 = await _sut.Handle(new GetTagsQuery(1, 10, "name", "asc"), CancellationToken.None);
        var page2 = await _sut.Handle(new GetTagsQuery(2, 10, "name", "asc"), CancellationToken.None);

        page1.Items.Select(t => t.Name).Should().NotIntersectWith(page2.Items.Select(t => t.Name));
    }

    [Fact]
    public async Task Handle_RoundsPercentageTo6DecimalPlaces()
    {
        _db.Tags.Add(Tag.Create("tag1", 1, 33.3333333333, DateTime.UtcNow));
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new GetTagsQuery(1, 10, "name", "asc"), CancellationToken.None);

        result.Items[0].Percentage.Should().Be(Math.Round(33.3333333333, 6));
    }

    private async Task SeedAsync(int count)
    {
        var tags = Enumerable.Range(1, count).Select(i =>
            Tag.Create($"tag-{i:D3}", i * 100L, i * 0.1, DateTime.UtcNow));
        _db.Tags.AddRange(tags);
        await _db.SaveChangesAsync();
    }

    public void Dispose() => _db.Dispose();
}
