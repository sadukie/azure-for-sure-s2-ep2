using Microsoft.Azure.Cosmos;

using CosmosClient client = new(
    accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
    authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!
);

Database database = await client.CreateDatabaseIfNotExistsAsync(
    id: "csharpcorner"
);

Container container =  database.GetContainer("products");
if (container != null) {
    await container.DeleteContainerAsync();
}

container = await database.CreateContainerIfNotExistsAsync(
    id: "products",
    partitionKeyPath: "/Category",
    throughput: 400
);

Console.WriteLine($"Container Id:\t{container.Id}");

var products = new List<Product>(){
    new Product(){id="8675307",Category="books",Name= "Diving Into Microsoft .NET Entity Framework", Price=0.00, Sale=false},
    new Product(){id="8675308",Category="books",Name= "WPF Simplified: Build Windows Apps Using C# and XAML", Price=7.00, Sale=true},
    new Product(){id="8675309",Category= "events",Name= "Azure for Sure - Season 2, Episode 2 - Data for All - Azure Cosmos DB", Price=0.00, Sale=false},
};


// Create items
foreach (Product item in products){
    Product createdItem = await container.CreateItemAsync<Product>(
        item: item,
        partitionKey: new PartitionKey(item.Category)
    );
    Console.WriteLine($"Created item:\t{createdItem.id}\t[{createdItem.Category},{createdItem.Name},{createdItem.Price}]");
}

// Read a single item
Product readItem = await container.ReadItemAsync<Product>(
    id: "8675308",
    partitionKey: new PartitionKey("books")
);
Console.WriteLine($"Found item:\t{readItem.id}\t[{readItem.Category},{readItem.Name},{readItem.Price}]");

// Read items with a parameterized query
var query = new QueryDefinition(
    query: "SELECT * FROM products p WHERE p.Category = @category"
)
    .WithParameter("@category", "books");

using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(
    queryDefinition: query
);

while (feed.HasMoreResults)
{
    FeedResponse<Product> response = await feed.ReadNextAsync();
    foreach (Product item in response)
    {
        Console.WriteLine($"Found item:\t{item.Name}");
    }
}

// Update the price of an item
readItem.Price = 9.99;
Console.WriteLine("Updating item");
await container.UpsertItemAsync<Product>(readItem,new PartitionKey(readItem.Category));

// Confirm the update
readItem = await container.ReadItemAsync<Product>(
    id: "8675308",
    partitionKey: new PartitionKey("books")
);
Console.WriteLine($"Found item:\t{readItem.id}\t[{readItem.Category},{readItem.Name},{readItem.Price}]");

// Delete the item
Console.WriteLine($"Deleting item {readItem.id}");

await container.DeleteItemAsync<Product>(readItem.id,new PartitionKey(readItem.Category));

// Confirm the delete
Console.WriteLine($"Confirming item {readItem.id} is deleted");

try {
    readItem = await container.ReadItemAsync<Product>(
    id: "8675308",
    partitionKey: new PartitionKey("books"));
    Console.WriteLine($"Not deleted");
} catch (CosmosException ex){
    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound){
        Console.WriteLine("That item is not found and may be deleted.");
    }
}
