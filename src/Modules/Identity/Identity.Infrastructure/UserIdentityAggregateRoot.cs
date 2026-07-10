using SharedKernel;

namespace Identity.Infrastructure;

/*
    An aggregate created exclusively for Identity, 
    so that we can have an AggregateRoot specific to Identity 
    that can be used to publish Identity-related integration events.
 */
public abstract class UserIdentityAggregateRoot : AggregateRoot;