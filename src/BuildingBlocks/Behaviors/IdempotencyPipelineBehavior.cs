using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Behaviors;

public sealed class IdempotencyPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentCommand<TResponse>
    where TResponse : Result
{
    private const int IDEMPOTENCY_KEY_EXPIRATION_MINUTES = 5;

    private readonly ICacheService _cacheService;

    public IdempotencyPipelineBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = $"idempotency:{request.IdempotencyKey}";

        try
        {
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await next(cancellationToken),
                TimeSpan.FromMinutes(IDEMPOTENCY_KEY_EXPIRATION_MINUTES))
                ?? throw new InvalidOperationException("Failed to process or retrieve command.");
        }
        catch
        {
            await _cacheService.DeleteAsync(cacheKey);
            throw;
        }
    }
}