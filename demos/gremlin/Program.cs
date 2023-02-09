// Based on the Azure Cosmos DB for Apache Gremlin quickstart
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json;

namespace GremlinNetSample
{
    class Program
    {
        private static string Host => Environment.GetEnvironmentVariable("GREMLIN_HOST") ?? throw new ArgumentException("Missing env var: Host");
        private static string PrimaryKey => Environment.GetEnvironmentVariable("GREMLIN_PRIMARY_KEY") ?? throw new ArgumentException("Missing env var: PrimaryKey");
        private static string Database => Environment.GetEnvironmentVariable("GREMLIN_DATABASE_NAME") ?? throw new ArgumentException("Missing env var: DatabaseName");
        private static string Container => Environment.GetEnvironmentVariable("GREMLIN_CONTAINER_NAME") ?? throw new ArgumentException("Missing env var: ContainerName");

        private static bool EnableSSL
        {
            get
            {
                if (Environment.GetEnvironmentVariable("EnableSSL") == null)
                {
                    return true;
                }

                if (!bool.TryParse(Environment.GetEnvironmentVariable("EnableSSL"), out bool value))
                {
                    throw new ArgumentException("Invalid env var: EnableSSL is not a boolean");
                }

                return value;
            }
        }

        private static int Port
        {
            get
            {
                if (Environment.GetEnvironmentVariable("Port") == null)
                {
                    return 443;
                }

                if (!int.TryParse(Environment.GetEnvironmentVariable("Port"), out int port))
                {
                    throw new ArgumentException("Invalid env var: Port is not an integer");
                }

                return port;
            } 
        }

        private static Dictionary<string, string> gremlinQueries = new Dictionary<string, string>
        {
            { "Cleanup",        "g.V().drop()" },
            { "AddVertex 1",    "g.addV('person').property('id', 'simon').property('firstName', 'Stephen').property('lastName','Simon').property('Twitter', '@codewithsimon').property('pk', 'pk')" },
            { "AddVertex 2",    "g.addV('person').property('id', 'sadukie').property('firstName', 'Sarah').property('lastName', 'Dutkiewicz').property('pk', 'pk')" },
            { "AddVertex 3",    "g.addV('person').property('id', 'jguadagno').property('firstName', 'Joe').property('lastName', 'Guadagno').property('pk', 'pk')" },
            { "AddVertex 4",    "g.addV('community').property('id', 'csharpcorner').property('name', 'C# Corner').property('pk', 'pk')" },
            { "AddEdge 1",      "g.V('jguadagno').addE('knows').to(g.V('simon'))" },
            { "AddEdge 2",      "g.V('sadukie').addE('knows').to(g.V('jguadagno'))" },
            { "AddEdge 3",      "g.V('simon').addE('is in').to(g.V('csharpcorner'))" },
            { "UpdateVertex",   "g.V('jguadagno').property('Twitter', '@jguadagno')" },
            { "CountVertices",  "g.V().count()" },
            { "Filter Range",   "g.V().hasLabel('person').has('Twitter')" },
            { "Project",        "g.V().hasLabel('person').values('firstName')" },
            { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
            { "Traverse",       "g.V('jguadagno').out('knows').hasLabel('person')" },
            { "Traverse 2x",    "g.V('jguadagno').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
            { "Loop",           "g.V('sadukie').repeat(out()).until(has('id', 'simon')).path()" },
           // { "DropEdge",       "g.V('sadukie').outE('knows').where(inV().has('id', 'jguadagno')).drop()" },
            { "CountEdges",     "g.E().count()" },
            //{ "DropVertex",     "g.V('sadukie').drop()" },
        };

        static void Main(string[] args)
        {
            string containerLink = "/dbs/" + Database + "/colls/" + Container;
            Console.WriteLine($"Connecting to: host: {Host}, port: {Port}, container: {containerLink}, ssl: {EnableSSL}");
            var gremlinServer = new GremlinServer(Host, Port, enableSsl: EnableSSL, 
                                                    username: containerLink, 
                                                    password: PrimaryKey);

            ConnectionPoolSettings connectionPoolSettings = new ConnectionPoolSettings()
            {
                MaxInProcessPerConnection = 10,
                PoolSize = 30, 
                ReconnectionAttempts= 3,
                ReconnectionBaseDelay = TimeSpan.FromMilliseconds(500)
            };

            var webSocketConfiguration =
                new Action<ClientWebSocketOptions>(options =>
                {
                    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                });


            using (var gremlinClient = new GremlinClient(
                gremlinServer, 
                new GraphSON2Reader(), 
                new GraphSON2Writer(), 
                GremlinClient.GraphSON2MimeType, 
                connectionPoolSettings, 
                webSocketConfiguration))
            {
                
                foreach (var query in gremlinQueries)
                {
                    Console.WriteLine(String.Format("Running this query: {0}: {1}", query.Key, query.Value));

                    // Create async task to execute the Gremlin query.
                    var resultSet = SubmitRequest(gremlinClient, query).Result;
                    if (resultSet.Count > 0)
                    {
                        Console.WriteLine("\tResult:");
                        foreach (var result in resultSet)
                        {
                            // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                            string output = JsonConvert.SerializeObject(result);
                            Console.WriteLine($"\t{output}");
                        }
                        Console.WriteLine();
                    }

                    // Print the status attributes for the result set.
                    // This includes the following:
                    //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
                    //  x-ms-total-request-charge   : The total request units charged for processing a request.
                    //  x-ms-total-server-time-ms   : The total time executing processing the request on the server.
                    PrintStatusAttributes(resultSet.StatusAttributes);
                    Console.WriteLine();
                }
            }

            // Exit program
            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadLine();
        }

        private static Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, KeyValuePair<string, string> query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query.Value);
            }
            catch (ResponseException e)
            {
                Console.WriteLine("\tRequest Error!");

                // Print the Gremlin status code.
                Console.WriteLine($"\tStatusCode: {e.StatusCode}");

                // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
                // additional attributes for retry handling and diagnostics.
                // These include:
                //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
                //                              : attribute 'x-ms-status-code' returns 429.
                //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
                PrintStatusAttributes(e.StatusAttributes);
                Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
                Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

                throw;
            }
        }

        private static void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
        {
            Console.WriteLine($"\tStatusAttributes:");
            Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
            Console.WriteLine($"\t[\"x-ms-total-server-time-ms\"] : { GetValueAsString(attributes, "x-ms-total-server-time-ms")}");
            Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
        }

        public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        }

        public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return null;
        }
    }
}