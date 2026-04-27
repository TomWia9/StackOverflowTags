using Application.Tags.Queries;
using FluentAssertions;

namespace UnitTests;

public class GetTagsQueryValidatorTests
{
    private readonly GetTagsQueryValidator _validator = new();

    [Theory]
    [InlineData(1, 25, "name", "asc")]
    [InlineData(5, 100, "percentage", "desc")]
    [InlineData(1, 1, "name", "desc")]
    public void Validate_ValidQuery_ReturnsSuccess(int page, int pageSize, string sortBy, string sortOrder)
    {
        var result = _validator.Validate(new GetTagsQuery(page, pageSize, sortBy, sortOrder));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 25, "name", "asc")]    // page < 1
    [InlineData(1, 0, "name", "asc")]     // pageSize < 1
    [InlineData(1, 101, "name", "asc")]   // pageSize > 100
    [InlineData(1, 25, "invalid", "asc")] // bad sortBy
    [InlineData(1, 25, "name", "both")]   // bad sortOrder
    public void Validate_InvalidQuery_ReturnsFail(int page, int pageSize, string sortBy, string sortOrder)
    {
        var result = _validator.Validate(new GetTagsQuery(page, pageSize, sortBy, sortOrder));
        result.IsValid.Should().BeFalse();
    }
}
