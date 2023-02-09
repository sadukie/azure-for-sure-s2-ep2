public class Product{
   public string id {get; init;} = default!; // must be lower case for Azure Cosmos DB
   public string Category {get; set;} = default!;
   public string Name {get; set;} = default!;
   public double Price {get; set;}
   public bool Sale {get; set;}
}