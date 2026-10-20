using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Payments.Application.Commands.ProcessPaymentGatewayEvent;

public sealed record ProcessPaymentGatewayEventCommand(
    string EventId, 
    string EventType,
    string DataJson) : IIdempotentCommand<Result>
{
    public string IdempotencyKey => $"GATEWAY_PAYMENT_EVENT_{EventId}";
}