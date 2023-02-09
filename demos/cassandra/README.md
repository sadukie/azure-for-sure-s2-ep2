# C# Demo for Azure Cosmos DB for Apache Cassandra

This is the demo created for Azure for Sure, Episode 2 - "Data for All - Azure Cosmos DB", presented by [Sadukie](https://sadukie.com).

> **Note**: This code was an attempt to try to work with .NET 6. However, .NET 6 is not supported with the DataStax C# Cassandra driver.

## References

- [Apache Cassandra client drivers](https://cassandra.apache.org/doc/latest/cassandra/getting_started/drivers.html)
  - Note: Does not seem compatible with .NET 6. Getting the error:

    ```text
    Unhandled exception. System.IO.FileLoadException: Could not load file or assembly 'Cassandra, Version=3.99.0.0, Culture=neutral, PublicKeyToken=10b231fbfc8c4b4d'. The located assembly's manifest definition does not match the assembly reference. (0x80131040)
    ```

  - Works with .NET Framework versions 4.5.2 and above and .NET Core versions 2.1 and above

- [Quickstart: Build an Apache Cassandra app with .NET SDK and Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/cassandra/manage-data-dotnet?WT.mc_id=DT-MVP-4025435)
