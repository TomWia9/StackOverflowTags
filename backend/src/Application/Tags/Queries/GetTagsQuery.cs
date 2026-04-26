using Application.Common;
using Application.Tags.Dtos;

using MediatR;

namespace Application.Tags.Queries;

public sealed record GetTagsQuery(
    int Page,
    int PageSize,
    string SortBy,
    string SortOrder)
    : IRequest<PaginatedResult<TagDto>>;