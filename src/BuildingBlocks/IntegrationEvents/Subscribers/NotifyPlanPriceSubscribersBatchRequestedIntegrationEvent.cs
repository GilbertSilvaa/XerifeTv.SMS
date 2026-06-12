using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("notifyplanssubscribersbatchrequested", 1.0)]
public record NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent(
        Guid PlanId,
        string PlanName,
        Money NewPrice,
        int? PageSubscribersCursor = null)
    : IntegrationEvent("notifyplanssubscribersbatchrequested", 1.0);
