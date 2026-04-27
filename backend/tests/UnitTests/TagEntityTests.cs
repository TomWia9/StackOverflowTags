using Domain.Entities;
using FluentAssertions;

namespace UnitTests;

public class TagEntityTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var fetchedAt = DateTime.UtcNow;
        var tag = Tag.Create("javascript", 1_000_000, 25.5, fetchedAt);

        tag.Name.Should().Be("javascript");
        tag.Count.Should().Be(1_000_000);
        tag.Percentage.Should().Be(25.5);
        tag.FetchedAt.Should().Be(fetchedAt);
    }

    [Fact]
    public void Create_ThrowsOnEmptyName()
    {
        var act = () => Tag.Create("", 100, 1.0, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ThrowsOnNegativeCount()
    {
        var act = () => Tag.Create("tag", -1, 1.0, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
