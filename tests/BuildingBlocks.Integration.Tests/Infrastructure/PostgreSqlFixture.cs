using BuildingBlocks.Integration.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    private Respawner _respawner = default!;
    private DbConnection _connection = default!;

    public string ConnectionString => _container.GetConnectionString();

    public PostgreSqlFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FakeDbContext>()
                        .UseNpgsql(ConnectionString)
                        .Options;

        await using var db = new FakeDbContext(options);
        await db.Database.EnsureCreatedAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgreSqlFixture>;