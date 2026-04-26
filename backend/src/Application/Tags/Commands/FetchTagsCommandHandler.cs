using Application.Tags.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tags.Commands;

public sealed class FetchTagsCommandHandler(
    IStackOverflowClient soClient,
    ITagRepository repository,
    ILogger<FetchTagsCommandHandler> logger)
    : IRequestHandler<FetchTagsCommand, FetchTagsResultDto>
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<FetchTagsResultDto> Handle(
        FetchTagsCommand request,
        CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        var started = DateTime.UtcNow;
        try
        {
            logger.LogInformation("Fetching at least {Min} tags from Stack Overflow", request.MinCount);

            var rawTags = await soClient.FetchTagsAsync(request.MinCount, cancellationToken);

            var totalCount = rawTags.Sum(t => t.Count);
            var fetchedAt = DateTime.UtcNow;

            var tags = rawTags
                .Select(t => Tag.Create(
                    t.Name,
                    t.Count,
                    totalCount > 0 ? (double)t.Count / totalCount * 100.0 : 0,
                    fetchedAt))
                .ToList();

            await repository.ReplaceAllAsync(tags, cancellationToken);

            var elapsed = DateTime.UtcNow - started;
            logger.LogInformation("Stored {Count} tags in {Elapsed}", tags.Count, elapsed);

            return new FetchTagsResultDto(tags.Count, elapsed);
        }
        finally
        {
            _lock.Release();
        }
    }
}