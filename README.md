# Azure Cookbook 🧑‍🍳

Small, deployable Azure samples. Each folder is a self-contained [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/) project: `cd` into it and run `azd up`.

## Samples

| Service | Sample | What it shows | Stack |
|---|---|---|---|
| Azure Cosmos DB | [Keyless with Managed Identity](azure-cosmos-db/keyless) | Cosmos DB for NoSQL without access keys: managed identity, data-plane RBAC and an Entra group for developers | Bicep · .NET 10 · App Service |

## Run a sample

```bash
cd azure-cosmos-db/keyless
```

```bash
azd up
```

Clean up with `azd down --purge`.
