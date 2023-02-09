using Azure;
using Azure.Data.Tables;

// C# record type for items in the table
public record Product : ITableEntity
{
    public string RowKey { get; set; } = default!;

    public string PartitionKey { get; set; } = default!;

    public string Name { get; init; } = default!;

    public double Price { get; set; }

    public bool Sale { get; init; }

    // This is an internal Azure Cosmos DB field.
    // This is needed - otherwise, the project won't build.
    public ETag ETag { get; set; } = default!;

    // This is an internal Azure Cosmos DB field.
    // This is needed - otherwise, the project won't build.
    public DateTimeOffset? Timestamp { get; set; } = default!;
}
