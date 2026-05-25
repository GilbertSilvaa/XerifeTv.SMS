using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using BuildingBlocks.Integration.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;

public abstract class PostgreSqlFixture<TDbContext> : IAsyncLifetime where TDbContext : DbContext
{
    private readonly Func<DbContextOptions<TDbContext>, TDbContext> _factory;
    private readonly PostgreSqlContainer _container;

    private Respawner _respawner = default!;
    private DbConnection _connection = default!;

    public string ConnectionString => _container.GetConnectionString();

    public PostgreSqlFixture(Func<DbContextOptions<TDbContext>, TDbContext> factory)
    {
        _factory = factory;

        _container = new PostgreSqlBuilder("postgres:17")
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

        var options = new DbContextOptionsBuilder<TDbContext>()
                        .UseNpgsql(ConnectionString)
                        .Options;

        await using var db = _factory(options);
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

public class FakeDbFixure() : PostgreSqlFixture<FakeDbContext>(options => new FakeDbContext(options));

[CollectionDefinition("PostgresFakeDbContext")]
public class PostgresCollectionFakeDbContext : ICollectionFixture<FakeDbFixure>;

public class OutboxDbFixture() : PostgreSqlFixture<OutboxDbContext>(options => new OutboxDbContext(options, default!));

[CollectionDefinition("PostgresOutboxDbContext")]
public class PostgresCollectionOutboxDbContext : ICollectionFixture<OutboxDbFixture>;

public class InboxDbFixture() : PostgreSqlFixture<InboxDbContext>(options => new InboxDbContext(options, default!));

[CollectionDefinition("PostgresInboxDbContext")]
public class PostgresCollectionInboxDbContext : ICollectionFixture<InboxDbFixture>;