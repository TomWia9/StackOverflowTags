namespace Domain.Entities;

public sealed class Tag
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public long Count { get; private set; }
    public double Percentage { get; private set; }
    public DateTime FetchedAt { get; private set; }

    private Tag() { }

    public static Tag Create(string name, long count, double percentage, DateTime fetchedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return new Tag
        {
            Name = name,
            Count = count,
            Percentage = percentage,
            FetchedAt = fetchedAt
        };
    }
}
