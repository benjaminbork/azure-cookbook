using System.ComponentModel.DataAnnotations;

namespace KeylessCosmos.Api.Cosmos;

public sealed class CosmosOptions
{
    public const string SectionName = "Cosmos";

    [Required, Url]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    public string DatabaseName { get; init; } = string.Empty;

    [Required]
    public string ContainerName { get; init; } = string.Empty;

    public bool SeedSampleData { get; init; } = true;
}
