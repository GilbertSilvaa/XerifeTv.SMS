using SharedKernel;

namespace Payments.Domain.PaymentSetup;

public interface IPaymentSetupRepository : IRepository<PaymentSetup>
{
    Task<PaymentSetup?> GetBySessionIdAsync(string sessionId);
}
