using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sefic.Api.Infrastructure.Persistence;

namespace Sefic.Api.Tests;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly string _databaseName =
        $"SeficPartnersTest_{Environment.ProcessId}_{Guid.NewGuid().ToString("N")[..8]}";

    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public string ConnectionString =>
        $@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={_databaseName};Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public async Task InitializeAsync()
    {
        await DropDatabaseAsync();

        var options = new DbContextOptionsBuilder<SeficDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using (var db = new SeficDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:SeficDb", ConnectionString));

        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        _factory?.Dispose();
        await DropDatabaseAsync();
    }

    private async Task DropDatabaseAsync()
    {
        var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" }.ConnectionString;

        await using var connection = new SqlConnection(master);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"IF DB_ID(N'{_databaseName}') IS NOT NULL " +
            $"BEGIN ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{_databaseName}]; END";
        await command.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture>;
