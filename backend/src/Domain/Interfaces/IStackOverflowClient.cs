namespace Domain.Interfaces;

public record StackOverflowTag(string Name, long Count);

public interface IStackOverflowClient
{
    Task<IReadOnlyList<StackOverflowTag>> FetchTagsAsync(int minCount, CancellationToken ct = default);
}
