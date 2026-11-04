using KeylessCosmos.Api.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace KeylessCosmos.Api.Todos;

public sealed class TodoSeeder(
    Container container,
    IOptions<CosmosOptions> options,
    ILogger<TodoSeeder> logger) : BackgroundService
{
    private static readonly Todo[] SampleTodos =
    [
        new("sample-1", "Deploy the Bicep template", true, DateTimeOffset.Parse("2026-01-01T09:00:00Z")),
        new("sample-2", "Call /health/cosmos", false, DateTimeOffset.Parse("2026-01-01T09:01:00Z")),
        new("sample-3", "Check that the account keys return 401", false, DateTimeOffset.Parse("2026-01-01T09:02:00Z")),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.SeedSampleData)
        {
            return;
        }

        try
        {
            using var iterator = container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");
            var count = 0;
            while (iterator.HasMoreResults)
            {
                count += (await iterator.ReadNextAsync(stoppingToken)).Sum();
            }

            if (count > 0)
            {
                return;
            }

            foreach (var todo in SampleTodos)
            {
                await container.UpsertItemAsync(todo, new PartitionKey(todo.Id), cancellationToken: stoppingToken);
            }

            logger.LogInformation("Seeded {Count} sample todos", SampleTodos.Length);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Seeding sample todos failed – the API still works, restart to retry");
        }
    }
}
