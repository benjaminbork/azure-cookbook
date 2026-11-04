using Azure.Core;
using Azure.Identity;

namespace KeylessCosmos.Api.Cosmos;

public static class CredentialFactory
{
    public static TokenCredential Create(IHostEnvironment env, IConfiguration config)
    {
        if (env.IsDevelopment())
        {
            return new AzureCliCredential();
        }

        var clientId = config["AZURE_CLIENT_ID"]
            ?? throw new InvalidOperationException(
                "AZURE_CLIENT_ID is not set. It must contain the client ID of the user-assigned managed identity.");

        return new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));
    }
}
