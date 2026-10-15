using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Customer;
using Payments.Infrastructure.Persistence.Database;

namespace Payments.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(PaymentDbContext dbContext) : base(dbContext) { }
}
