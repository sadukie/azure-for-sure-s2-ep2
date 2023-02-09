using Npgsql;
namespace Driver
{
    public class Program
    {
       
        static async Task Main(string[] args)
        {
            // Replace <cluster> with your cluster name and <password> with your password:
            var connStr = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));

            connStr.TrustServerCertificate = true;

            using (var conn = new NpgsqlConnection(connStr.ToString()))
            {
                Console.WriteLine("Opening connection");
                conn.Open();
                using (var command = new NpgsqlCommand("DROP TABLE IF EXISTS products;", conn))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Finished dropping table (if existed)");
                }
                using (var command = new NpgsqlCommand("CREATE TABLE products (product_id text, category text, name text, price float, sale bool);", conn))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Finished creating table");
                }
                using (var command = new NpgsqlCommand("CREATE INDEX idx_products_id ON products(product_id);", conn))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Finished creating index");
                }

                var products = new List<Product>(){
                    new Product(){id="8675307",Category="books",Name= "Diving Into Microsoft .NET Entity Framework", Price=0.00, Sale=false},
                    new Product(){id="8675308",Category="books",Name= "WPF Simplified: Build Windows Apps Using C# and XAML", Price=7.00, Sale=true},
                    new Product(){id="8675309",Category= "events",Name= "Azure for Sure - Season 2, Episode 2 - Data for All - Azure Cosmos DB", Price=0.00, Sale=false},
                };

                // Write items
                foreach (Product item in products){
                    using (var command = new NpgsqlCommand("INSERT INTO  products (product_id,category,name,price,sale) VALUES (@n1, @q1, @a, @b, @c)", conn))
                    {
                        command.Parameters.AddWithValue("n1", item.id);
                        command.Parameters.AddWithValue("q1", item.Category);
                        command.Parameters.AddWithValue("a", item.Name);
                        command.Parameters.AddWithValue("b", item.Price);
                        command.Parameters.AddWithValue("c", item.Sale);
                        int nRows = command.ExecuteNonQuery();
                        Console.WriteLine(String.Format("Number of rows inserted={0}", nRows));
                    }
                }

                // Read items
                /* using (var command = new NpgsqlCommand("SELECT * FROM products WHERE product_id='8675308';", conn))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync()){
                        Console.WriteLine(reader.GetString(0));
                    }
                }*/

                // Get the number of books - scalar demo
                /*using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM products WHERE category='books';", conn))
                {
                    var count = await command.ExecuteScalarAsync();
                    Console.WriteLine($"There are {count} books in the system");
                }*/


                // Delete an item
                using (var command = new NpgsqlCommand("DELETE FROM products WHERE product_id=@id1;", conn))
                {
                    command.Parameters.AddWithValue("id1","8675308");
                    await command.ExecuteNonQueryAsync();                    
                }
            }
            Console.WriteLine("Press RETURN to exit");
            Console.ReadLine();
        }
    }
}