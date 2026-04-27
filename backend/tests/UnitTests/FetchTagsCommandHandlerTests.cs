using Application.Tags.Commands;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests;

public class FetchTagsCommandHandlerTests
{
    private readonly Mock<IStackOverflowClient> _soClient = new();
    private readonly Mock<ITagRepository> _repo = new();
    private readonly FetchTagsCommandHandler _sut;

    public FetchTagsCommandHandlerTests()
    {
        _sut = new FetchTagsCommandHandler(
            _soClient.Object,
            _repo.Object,
            NullLogger<FetchTagsCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_StoresCorrectPercentages_WhenTagsAreFetched()
    {
        // Arrange
        var fakeTags = new List<StackOverflowTag>
        {
            new("javascript", 2_000_000),
            new("python", 1_000_000),
            new("java", 1_000_000),
        };
        _soClient.Setup(c => c.FetchTagsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeTags);

        Tag[]? stored = null;
        _repo.Setup(r => r.ReplaceAllAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Tag>, CancellationToken>((tags, _) => stored = tags.ToArray())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new FetchTagsCommand(1), CancellationToken.None);

        // Assert
        result.FetchedCount.Should().Be(3);
        stored.Should().NotBeNull();
        stored!.Single(t => t.Name == "javascript").Percentage.Should().BeApproximately(50.0, 0.001);
        stored!.Single(t => t.Name == "python").Percentage.Should().BeApproximately(25.0, 0.001);
    }

    [Fact]
    public async Task Handle_CallsReplaceAll_AlwaysEvenWithEmptyResponse()
    {
        // Arrange
        _soClient.Setup(c => c.FetchTagsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repo.Setup(r => r.ReplaceAllAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(new FetchTagsCommand(1), CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ReplaceAllAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectDuration()
    {
        // Arrange
        _soClient.Setup(c => c.FetchTagsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new StackOverflowTag("dotnet", 500_000)]);
        _repo.Setup(r => r.ReplaceAllAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new FetchTagsCommand(1), CancellationToken.None);

        // Assert
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}