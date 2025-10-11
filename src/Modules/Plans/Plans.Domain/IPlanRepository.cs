using SharedKernel;

namespace Plans.Domain;

public interface IPlanRepository : IRepository<Plan>
{
	Task<bool> ExistsByNameAsync(string name);
	Task<bool> ExistsByPriceAsync(Money price);
	Task<bool> ExistsByScreensAsync(int screens);
}