using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Mock<IStackOverflowClient> SoClientMock { get; } = new();

    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"so-tags-test-{Guid.NewGuid()}.db");

    public ApiFactory()
    {
        // Mock must be ready before host starts, Program.cs calls
        // EnsureTagsLoadedCommand on startup which immediately hits the SO client.
        SetupMockTags();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var apiOutput = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "backend", "src", "Api", "bin", "Debug", "net10.0"));

        if (Directory.Exists(apiOutput))
            builder.UseContentRoot(apiOutput);

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IStackOverflowClient>();
            services.AddSingleton<IStackOverflowClient>(SoClientMock.Object);

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    public void SetupMockTags(int count = 1050)
    {
        SoClientMock
            .Setup(c => c.FetchTagsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GenerateTags(count));
    }

    private static List<StackOverflowTag> GenerateTags(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new StackOverflowTag($"tag-{i:D4}", (long)(count - i + 1) * 100))
            .ToList();
}
