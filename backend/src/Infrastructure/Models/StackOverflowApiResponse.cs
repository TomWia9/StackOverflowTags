using System.Text.Json.Serialization;

namespace Infrastructure.Models;

public sealed class StackOverflowApiResponse
{
    [JsonPropertyName("items")] public List<StackOverflowApiTag>? Items { get; set; }
    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
    [JsonPropertyName("backoff")] public int? Backoff { get; set; }
}