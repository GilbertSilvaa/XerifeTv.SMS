using SharedKernel;

namespace Notifications.Infrastructure;

/*
    An aggregate created exclusively for Notifications, 
    so that we can have an AggregateRoot specific to Notifications 
    that can be used to publish Notifications-related integration events in InboxMessages.
 */

public abstract class NotificationAggregateRoot : AggregateRoot;
