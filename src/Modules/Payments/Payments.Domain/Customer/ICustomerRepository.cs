using SharedKernel;

namespace Payments.Domain.Customer;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdentityIdAsync(Guid identityId);
}
