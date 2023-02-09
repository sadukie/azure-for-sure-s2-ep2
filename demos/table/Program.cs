using Azure.Data.Tables;

TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING"));

TableClient tableClient = tableServiceClient.GetTableClient(
    tableName: "csharpcorner"
);

await tableClient.CreateIfNotExistsAsync();

var products = new List<Product>{
    new Product(){
        RowKey = "8675307",
        PartitionKey = "books",
        Name = "Diving Into Microsoft .NET Entity Framework",
        Price = 0.00,
        Sale = false
    },
    new Product(){
        RowKey = "8675308",
        PartitionKey = "books",
        Name = "WPF Simplified: Build Windows Apps Using C# and XAML",
        Price = 7.00,
        Sale = true
    },
    new Product(){
        RowKey = "8675309",
        PartitionKey = "events",
        Name = "Azure for Sure - Season 2, Episode 2 - Data for All - Azure Cosmos DB",
        Price = 0.00,
        Sale = false
    },
};

// Create an item
foreach (Product product in products){
    await tableClient.AddEntityAsync<Product>(product);
}

// Read a single item
var book8675307 = await tableClient.GetEntityAsync<Product>(
    rowKey: "8675307",
    partitionKey: "books"
);
Console.WriteLine("Single product:");
Console.WriteLine(book8675307.Value.Name);

// Update an item
Product productToUpdate = (Product)book8675307.Value;
productToUpdate.Price = 9.99;
await tableClient.UpsertEntityAsync<Product>(productToUpdate);
// Get the updated item
var updatedProduct = await tableClient.GetEntityAsync<Product>(
    rowKey: "8675307",
    partitionKey: "books"
);
Console.WriteLine("Single product:");
Console.WriteLine(updatedProduct.Value.Name + " " + updatedProduct.Value.Price.ToString());

// Query for all items in a partition
var allBooks = tableClient.Query<Product>(x => x.PartitionKey == "books");

Console.WriteLine("All books:");
foreach (var item in allBooks)
{
    Console.WriteLine(item.Name);
}
