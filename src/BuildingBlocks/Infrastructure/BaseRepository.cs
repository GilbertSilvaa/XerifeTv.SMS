using SharedKernel;

namespace BuildingBlocks.Infrastructure;

public class BaseRepository<T> : IRepository<T> where T : AggregateRoot
{
	private readonly DbContext _dbContext;
	private readonly DbSet<T> _dataSet;

	public BaseRepository(DbContext dbContext)
	{
		_dbContext = dbContext;
		_dataSet = _dbContext.Set<T>();
	}

	public async Task AddOrUpdateAsync(T entity)
	{
		_dataSet.Update(entity);
		await _dbContext.SaveChangesAsync();
	}

	public async Task<int> CountAsync()
	{
		return await _dataSet.Where(e => !e.IsDeleted).CountAsync();
	}

	public async Task<T?> GetByIdAsync(Guid id)
	{
		return await _dataSet.SingleOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
	}

	public async Task RemoveAsync(Guid id)
	{
		var result = await _dataSet.SingleOrDefaultAsync(e => e.Id == id);

		if (result is T entity)
		{
			entity.Delete();
			_dataSet.Update(entity);
			await _dbContext.SaveChangesAsync();
		}
	}
}