using System.Text.Json.Serialization;

namespace Infrastructure.Models;

public sealed class StackOverflowApiTag
{
    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("count")] public long Count { get; set; }
}