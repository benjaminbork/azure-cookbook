using System.Net;
using KeylessCosmos.Api.Cosmos;
using KeylessCosmos.Api.Todos;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddKeylessCosmos(builder.Configuration);
builder.Services.AddHostedService<TodoSeeder>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    if (exception is CosmosException { StatusCode: HttpStatusCode.Forbidden } cosmosEx)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await Results.Problem(
            title: "Cosmos DB denied the request (data-plane RBAC)",
            detail: "The identity has a valid Entra ID token but no sqlRoleAssignment that allows this dataAction. " +
                    "Check infra/modules/cosmos-role-assignment.bicep (scope + role) and allow up to ~5 minutes for propagation. " +
                    $"SubStatus: {cosmosEx.SubStatusCode}.",
            statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(context);
        return;
    }

    await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/health/cosmos", async (Container container, CancellationToken ct) =>
{
    var response = await container.ReadContainerAsync(cancellationToken: ct);
    return Results.Ok(new
    {
        status = "ok",
        container = response.Resource.Id,
        partitionKey = response.Resource.PartitionKeyPath,
        requestCharge = response.RequestCharge,
    });
});

app.MapTodos();

app.Run();
