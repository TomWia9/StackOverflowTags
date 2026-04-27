using System.Net;
using System.Net.Http.Json;
using Application.Common;
using Application.Tags.Commands;
using Application.Tags.Dtos;
using Application.Tags.Queries;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

public sealed class TagsEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public TagsEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.SetupMockTags(1050);
        _client = factory.CreateClient();
    }

    // ── GET /api/tags ──────────────────────────────────────────────────────

    [Fact]
    public async Task GET_Tags_Returns200()
    {
        var response = await _client.GetAsync("/api/tags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_Tags_ReturnsCorrectPaginationShape()
    {
        var result = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>("/api/tags?page=1&pageSize=20");

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(20);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1000);
        result.TotalPages.Should().Be((int)Math.Ceiling(result.TotalCount / 20.0));
    }

    [Fact]
    public async Task GET_Tags_SecondPage_HasDifferentItems()
    {
        var page1 = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>("/api/tags?page=1&pageSize=25");
        var page2 = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>("/api/tags?page=2&pageSize=25");

        var names1 = page1!.Items.Select(t => t.Name).ToHashSet();
        var names2 = page2!.Items.Select(t => t.Name).ToHashSet();

        names1.Intersect(names2).Should().BeEmpty();
    }

    [Fact]
    public async Task GET_Tags_SortByName_Ascending_IsOrdered()
    {
        var result = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>(
            "/api/tags?page=1&pageSize=50&sortBy=name&sortOrder=asc");

        result!.Items.Select(t => t.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GET_Tags_SortByName_Descending_IsOrdered()
    {
        var result = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>(
            "/api/tags?page=1&pageSize=50&sortBy=name&sortOrder=desc");

        result!.Items.Select(t => t.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GET_Tags_SortByPercentage_Descending_IsOrdered()
    {
        var result = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>(
            "/api/tags?page=1&pageSize=50&sortBy=percentage&sortOrder=desc");

        result!.Items.Select(t => t.Percentage).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GET_Tags_PercentageSum_IsApproximately100()
    {
        // Fetch all pages and sum up percentages
        double total = 0;
        int page = 1;
        while (true)
        {
            var result = await _client.GetFromJsonAsync<PaginatedResult<TagDto>>(
                $"/api/tags?page={page}&pageSize=100");
            total += result!.Items.Sum(t => t.Percentage);
            if (page >= result.TotalPages) break;
            page++;
        }

        total.Should().BeApproximately(100.0, 0.01);
    }

    // ── Validation errors ──────────────────────────────────────────────────

    [Fact]
    public async Task GET_Tags_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/api/tags?page=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Tags_PageSizeTooLarge_Returns400()
    {
        var response = await _client.GetAsync("/api/tags?pageSize=101");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Tags_InvalidSortBy_Returns400()
    {
        var response = await _client.GetAsync("/api/tags?sortBy=invalid");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Tags_InvalidSortOrder_Returns400()
    {
        var response = await _client.GetAsync("/api/tags?sortOrder=random");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /api/tags/refresh ─────────────────────────────────────────────

    [Fact]
    public async Task POST_Refresh_Returns200()
    {
        _factory.SetupMockTags(1000);
        var response = await _client.PostAsync("/api/tags/refresh", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Refresh_ReturnsResult_WithFetchedCount()
    {
        _factory.SetupMockTags(1000);
        var result = await _client.PostAsJsonAsync<object?>("/api/tags/refresh", null);
        var body = await result.Content.ReadFromJsonAsync<FetchTagsResultDto>();

        body.Should().NotBeNull();
        body!.FetchedCount.Should().Be(1000);
    }

    // ── OpenAPI ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GET_OpenApi_Returns200()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}