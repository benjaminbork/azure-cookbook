using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace KeylessCosmos.Api.Cosmos;

public static class CosmosServiceCollectionExtensions
{
    public static IServiceCollection AddKeylessCosmos(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<CosmosOptions>()
            .Bind(config.GetSection(CosmosOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            var env = sp.GetRequiredService<IHostEnvironment>();

            var credential = CredentialFactory.Create(env, config);

            return new CosmosClient(
                accountEndpoint: options.Endpoint,
                tokenCredential: credential,
                clientOptions: new CosmosClientOptions
                {
                    ApplicationName = "keyless-cosmos-sample",
                    ConnectionMode = ConnectionMode.Direct,
                    UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web),
                });
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            return sp.GetRequiredService<CosmosClient>().GetContainer(options.DatabaseName, options.ContainerName);
        });

        return services;
    }
}
