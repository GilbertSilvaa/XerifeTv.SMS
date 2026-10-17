using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.PaymentSetup;
using Payments.Infrastructure.Persistence.Database;

namespace Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentSetupRepository : BaseRepository<PaymentSetup>, IPaymentSetupRepository
{
    public PaymentSetupRepository(PaymentDbContext dbContext) : base(dbContext) { }

    public async Task<PaymentSetup?> GetBySessionIdAsync(string sessionId)
    {
        return await _dataSet
            .SingleOrDefaultAsync(e => e.SessionId == sessionId && !e.IsDeleted);
    }
}