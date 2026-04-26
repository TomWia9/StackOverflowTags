using Application.Tags.Dtos;
using MediatR;

namespace Application.Tags.Commands;

public sealed record FetchTagsCommand(int MinCount = 1000) : IRequest<FetchTagsResultDto>;