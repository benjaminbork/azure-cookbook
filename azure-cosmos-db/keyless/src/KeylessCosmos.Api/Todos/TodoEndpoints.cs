using System.Net;
using Microsoft.Azure.Cosmos;

namespace KeylessCosmos.Api.Todos;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodos(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todos").WithTags("Todos");

        group.MapGet("/", async (Container container, CancellationToken ct) =>
        {
            using var iterator = container.GetItemQueryIterator<Todo>(
                "SELECT * FROM c ORDER BY c.createdAt");

            var todos = new List<Todo>();
            while (iterator.HasMoreResults)
            {
                todos.AddRange(await iterator.ReadNextAsync(ct));
            }

            return Results.Ok(todos);
        });

        group.MapGet("/{id}", async (string id, Container container, CancellationToken ct) =>
        {
            try
            {
                var response = await container.ReadItemAsync<Todo>(id, new PartitionKey(id), cancellationToken: ct);
                return Results.Ok(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
        });

        group.MapPost("/", async (TodoInput input, Container container, CancellationToken ct) =>
        {
            var todo = new Todo(Guid.NewGuid().ToString(), input.Title, input.IsComplete, DateTimeOffset.UtcNow);

            var response = await container.CreateItemAsync(todo, new PartitionKey(todo.Id), cancellationToken: ct);
            return Results.Created($"/todos/{todo.Id}", response.Resource);
        });

        group.MapPut("/{id}", async (string id, TodoInput input, Container container, CancellationToken ct) =>
        {
            try
            {
                var existing = await container.ReadItemAsync<Todo>(id, new PartitionKey(id), cancellationToken: ct);
                var updated = existing.Resource with { Title = input.Title, IsComplete = input.IsComplete };

                var response = await container.ReplaceItemAsync(updated, id, new PartitionKey(id), cancellationToken: ct);
                return Results.Ok(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/{id}", async (string id, Container container, CancellationToken ct) =>
        {
            try
            {
                await container.DeleteItemAsync<Todo>(id, new PartitionKey(id), cancellationToken: ct);
                return Results.NoContent();
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Results.NotFound();
            }
        });

        return app;
    }
}
