using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using Payments.Domain.Customer;
using Payments.Domain.PaymentSetup;
using Payments.Infrastructure.Persistence.Database;

namespace Payments.Infrastructure.Persistence;

public abstract class PaymentUnitOfWorks
{
    public sealed class CustomerUnitOfWork(PaymentDbContext dbContext)
        : BaseUnitOfWork<PaymentDbContext, Customer>(dbContext), IUnitOfWork<Customer>;

    public sealed class PaymentSetupUnitOfWork(PaymentDbContext dbContext)
        : BaseUnitOfWork<PaymentDbContext, PaymentSetup>(dbContext), IUnitOfWork<PaymentSetup>;
}