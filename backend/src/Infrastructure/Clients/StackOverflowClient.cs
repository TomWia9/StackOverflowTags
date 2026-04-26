using System.IO.Compression;
using System.Text.Json;
using Domain.Interfaces;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Clients;

public sealed class StackOverflowClient(
    HttpClient http,
    ILogger<StackOverflowClient> logger,
    IConfiguration configuration)
    : IStackOverflowClient
{
    private readonly string _baseUrl =
        configuration["StackOverflowApiBaseUrl"] ?? throw new InvalidOperationException();

    private const int PageSize = 100;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<StackOverflowTag>> FetchTagsAsync(
        int minCount, CancellationToken ct = default)
    {
        var result = new List<StackOverflowTag>();
        int page = 1;

        while (result.Count < minCount)
        {
            var url = $"{_baseUrl}?page={page}&pagesize={PageSize}&order=desc&sort=popular&site=stackoverflow";
            logger.LogDebug("Fetching Stack Overflow tags page {Page}", page);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.AcceptEncoding.ParseAdd("gzip");

            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(ct);
            var encoding = response.Content.Headers.ContentEncoding;
            if (encoding.Contains("gzip"))
                stream = new GZipStream(stream, CompressionMode.Decompress);

            StackOverflowApiResponse? apiResponse;
            await using (stream)
                apiResponse = await JsonSerializer.DeserializeAsync<StackOverflowApiResponse>(stream, JsonOptions, ct);

            if (apiResponse?.Items is null || apiResponse.Items.Count == 0) break;

            var newTags = apiResponse.Items.Select(i => new StackOverflowTag(i.Name, i.Count));
            var existingNames = result.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            result.AddRange(newTags.Where(t => !existingNames.Contains(t.Name)));

            if (!apiResponse.HasMore) break;

            if (apiResponse.Backoff.HasValue)
            {
                logger.LogWarning("Stack Overflow API backoff: {Seconds}s", apiResponse.Backoff.Value);
                await Task.Delay(TimeSpan.FromSeconds(apiResponse.Backoff.Value), ct);
            }
            else
            {
                await Task.Delay(150, ct);
            }

            page++;
        }

        logger.LogInformation("Fetched {Count} tags from Stack Overflow", result.Count);
        return result;
    }
}