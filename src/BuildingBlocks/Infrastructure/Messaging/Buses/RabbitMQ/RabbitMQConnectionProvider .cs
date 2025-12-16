using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;

public sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
	private readonly ConnectionFactory _factory;
	private IConnection? _connection;
	private readonly SemaphoreSlim _lock = new(1, 1);

	public RabbitMqConnectionProvider(IConfiguration configuration)
	{
		_factory = new ConnectionFactory
		{
			HostName = configuration["RabbitMQ:Host"]!,
			Port = int.Parse(configuration["RabbitMQ:Port"]!),
			UserName = configuration["RabbitMQ:Username"]!,
			Password = configuration["RabbitMQ:Password"]!,
			VirtualHost = configuration["RabbitMQ:VHost"]!
		};
	}

	public async Task<IChannel> GetChannelAsync()
	{
		await _lock.WaitAsync();
		try
		{
			if (_connection is null || !_connection.IsOpen)
				_connection = await _factory.CreateConnectionAsync();

			return await _connection.CreateChannelAsync();
		}
		finally
		{
			_lock.Release();
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_connection != null)
			await _connection.DisposeAsync();

		_lock.Dispose();
	}
}