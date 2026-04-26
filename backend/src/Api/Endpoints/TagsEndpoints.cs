using Application.Tags.Commands;
using Application.Tags.Dtos;
using Application.Tags.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags")
            .WithOpenApi();

        group.MapGet("/", GetTags)
            .WithName("GetTags")
            .WithSummary("Get paginated tags")
            .WithDescription("Returns a paginated, sortable list of Stack Overflow tags with percentage share.");

        group.MapPost("/refresh", RefreshTags)
            .WithName("RefreshTags")
            .WithSummary("Refresh tags from Stack Overflow")
            .WithDescription("Forces a re-fetch of all tags from the Stack Overflow API and recomputes percentages.");

        return app;
    }

    private static async Task<Results<Ok<Application.Common.PaginatedResult<TagDto>>, BadRequest<string>>> GetTags(
        IMediator mediator,
        int page = 1,
        int pageSize = 25,
        string sortBy = "name",
        string sortOrder = "asc",
        CancellationToken ct = default)
    {
        try
        {
            var result = await mediator.Send(
                new GetTagsQuery(page, pageSize, sortBy, sortOrder), ct);
            
            return TypedResults.Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return TypedResults.BadRequest(errors);
        }
    }

    private static async Task<Ok<FetchTagsResultDto>> RefreshTags(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new FetchTagsCommand(), ct);
        
        return TypedResults.Ok(result);
    }
}
