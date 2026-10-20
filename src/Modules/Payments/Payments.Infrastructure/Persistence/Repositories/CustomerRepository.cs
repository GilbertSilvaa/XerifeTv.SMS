using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Customer;
using Payments.Infrastructure.Persistence.Database;

namespace Payments.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(PaymentDbContext dbContext) : base(dbContext) { }

    public async Task<Customer?> GetByExternalIdAsync(string externalId)
    {
        return await _dataSet
            .SingleOrDefaultAsync(e => e.ExternalId == externalId && !e.IsDeleted);
    }

    public async Task<Customer?> GetByIdentityIdAsync(Guid identityId)
    {
        return await _dataSet
            .SingleOrDefaultAsync(e => e.IdentityId == identityId && !e.IsDeleted);
    }
}