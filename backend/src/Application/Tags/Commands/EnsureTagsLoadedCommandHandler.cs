using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Tags.Commands;

public sealed class EnsureTagsLoadedCommandHandler(
    ITagRepository repository,
    IMediator mediator,
    ILogger<EnsureTagsLoadedCommandHandler> logger)
    : IRequestHandler<EnsureTagsLoadedCommand>
{
    public async Task Handle(EnsureTagsLoadedCommand request, CancellationToken cancellationToken)
    {
        var count = await repository.CountAsync(cancellationToken);
        if (count >= request.MinCount)
        {
            logger.LogInformation("Database already has {Count} tags, skipping fetch", count);
            return;
        }

        logger.LogInformation("Only {Count} tags in DB, triggering fetch", count);
        await mediator.Send(new FetchTagsCommand(request.MinCount), cancellationToken);
    }
}