using MediatR;

namespace Application.Tags.Commands;

public sealed record EnsureTagsLoadedCommand(int MinCount = 1000) : IRequest;