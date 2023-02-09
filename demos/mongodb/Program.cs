using MongoDB.Driver;

var client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_CONNECTION"));
var db = client.GetDatabase("csharpcorner");

var products = new List<Product>(){
    new Product("8675307","books", "Diving Into Microsoft .NET Entity Framework", 0.00, false),
    new Product("8675308","books", "WPF Simplified: Build Windows Apps Using C# and XAML", 7.00, true),
    new Product("8675309", "events", "Azure for Sure - Season 2, Episode 2 - Data for All - Azure Cosmos DB", 0.00, false),
};

var productsCollection = db.GetCollection<Product>("products");


// Delete all events
productsCollection.DeleteMany("{}");

// Add items
foreach (Product item in products){
    productsCollection.InsertOne(item);
}
// Read a single item from container
Product windowsBook = (await productsCollection.FindAsync(p => p.Name.Contains("Windows"))).FirstOrDefault();
Console.WriteLine("Single product:");
Console.WriteLine(windowsBook.Name+ " " + windowsBook.Price);

// Read multiple items from container
var filteredProducts = productsCollection.AsQueryable().Where(p => p.Category == "books");

Console.WriteLine("Multiple books:");
foreach (var item in filteredProducts)
{
    Console.WriteLine(item.Name);
}

// Read a single item from container
Product firstEvent = (await productsCollection.FindAsync(p => p.Category.Contains("events"))).FirstOrDefault();
Console.WriteLine("Single event:");
Console.WriteLine(firstEvent.Name+ " " + firstEvent.Price);


// Delete events
var deleteEvents = Builders<Product>.Filter.Eq("Category", "events");
productsCollection.DeleteMany(deleteEvents);

// Confirm no more events
var queryForEvents = productsCollection.Find(p => p.Category.Contains("events"));
var eventCount = (await queryForEvents.CountDocumentsAsync());
Console.WriteLine("Event count after deletion:");
Console.WriteLine(eventCount);