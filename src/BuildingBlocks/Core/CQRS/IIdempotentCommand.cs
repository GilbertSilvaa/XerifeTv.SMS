namespace BuildingBlocks.Core.CQRS;

public interface IIdempotentCommand : IIdempotentCommand<Unit>;

public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    string IdempotencyKey { get; }
}