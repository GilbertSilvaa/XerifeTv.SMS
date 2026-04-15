using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using Subscribers.Application.Queries.ReadModels;
using Subscribers.Domain.Entities;
using Subscribers.Infrastructure.Persistence.Database;

namespace Subscribers.Infrastructure.Persistence.Repositories;

public sealed class SubscribersReadRepository : ISubscribersReadRepository
{
    private readonly SubscriberDbContext _dbContext;
    private readonly DbSet<Subscriber> _dataSet;

    public SubscribersReadRepository(SubscriberDbContext dbContext)
    {
        _dbContext = dbContext;
        _dataSet = _dbContext.Subscribers;
    }

    public async Task<PagedList<SubscriberDto>> GetSubscribersAsync(PagedQuery query)
    {
        var totalCount = await _dataSet.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var subscribers = await _dataSet
            .AsNoTracking()
            .Include(s => s.Signatures)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new SubscriberDto(
                s.UserName,
                s.Email,
                s.Signatures.Select(sig => new SignatureDto(
                    sig.Plan,
                    sig.Status,
                    sig.StartDate,
                    sig.EndDate,
                    sig.RenewalDate)).ToList()))
            .ToListAsync();

        return new PagedList<SubscriberDto>(query.Page, totalPages, subscribers);
    }

    public async Task<SubscriberDto?> GetSubscriberByIdAsync(Guid id)
    {
        return await _dataSet
            .AsNoTracking()
            .Include(s => s.Signatures)
            .Where(s => s.Id == id)
            .Select(s => new SubscriberDto(
                s.UserName,
                s.Email,
                s.Signatures.Select(sig => new SignatureDto(
                    sig.Plan,
                    sig.Status,
                    sig.StartDate,
                    sig.EndDate,
                    sig.RenewalDate)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<SubscriberDto?> GetSubscriberByEmailAsync(string email)
    {
        return await _dataSet
            .AsNoTracking()
            .Include(s => s.Signatures)
            .Where(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            .Select(s => new SubscriberDto(
                s.UserName,
                s.Email,
                s.Signatures.Select(sig => new SignatureDto(
                    sig.Plan,
                    sig.Status,
                    sig.StartDate,
                    sig.EndDate,
                    sig.RenewalDate)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<SubscriberDto?> GetSubscriberByUserNameAsync(string userName)
    {
        return await _dataSet
            .AsNoTracking()
            .Include(s => s.Signatures)
            .Where(s => s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
            .Select(s => new SubscriberDto(
                s.UserName,
                s.Email,
                s.Signatures.Select(sig => new SignatureDto(
                    sig.Plan,
                    sig.Status,
                    sig.StartDate,
                    sig.EndDate,
                    sig.RenewalDate)).ToList()))
            .FirstOrDefaultAsync();
    }
}
