using Application.Common;
using Application.Tags.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Tags.Queries;

public sealed class GetTagsQueryHandler(ITagRepository repository)
    : IRequestHandler<GetTagsQuery, PaginatedResult<TagDto>>
{
    public async Task<PaginatedResult<TagDto>> Handle(
        GetTagsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SortBy.ToLower(),
            request.SortOrder.ToLower(),
            cancellationToken);

        var dtos = items
            .Select(t => new TagDto(t.Name, t.Count, Math.Round(t.Percentage, 6)))
            .ToList();

        return new PaginatedResult<TagDto>(dtos, request.Page, request.PageSize, total);
    }
}