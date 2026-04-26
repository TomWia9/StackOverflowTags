using FluentValidation;

namespace Application.Tags.Queries;

public sealed class GetTagsQueryValidator : AbstractValidator<GetTagsQuery>
{
    private static readonly string[] AllowedSortBy = ["name", "percentage"];
    private static readonly string[] AllowedSortOrder = ["asc", "desc"];

    public GetTagsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(v => AllowedSortBy.Contains(v.ToLower()))
            .WithMessage("SortBy must be 'name' or 'percentage'.");
        RuleFor(x => x.SortOrder)
            .Must(v => AllowedSortOrder.Contains(v.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}