namespace SharedKernel;

public interface IRepository<T> where T : Entity
{
	public Task AddOrUpdateAsync(T entity);
	public Task RemoveAsync(Guid id);
	public Task<T?> GetByIdAsync(Guid id);
	public Task<int> CountAsync();
}