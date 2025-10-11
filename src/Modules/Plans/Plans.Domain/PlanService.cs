using Plans.Domain.Exceptions;
using SharedKernel;

namespace Plans.Domain;

public sealed class PlanService
{
	private readonly IPlanRepository _repository;

	public PlanService(IPlanRepository repository)
	{
		_repository = repository;
	}

	public async Task<Result<Plan>> CreatePLanAsync(string name, string description, int screens, Money price)
	{
		if (await _repository.ExistsByNameAsync(name))
		{
			Error error = new("PlanService.CreatePLanAsync", $"Plan with name '{name}' already exists.");
			return Result<Plan>.Failure(error);
		}

		if (await _repository.ExistsByPriceAsync(price))
		{
			Error error = new("PlanService.CreatePLanAsync", $"Plan with price {price.Currency} {price.Amount} already exists.");
			return Result<Plan>.Failure(error);
		}

		if (await _repository.ExistsByScreensAsync(screens))
		{
			Error error = new("PlanService.CreatePLanAsync", $"Plan with {screens} screens already exists.");
			return Result<Plan>.Failure(error);
		}

		try
		{
			var plan = Plan.Create(name, description, screens, price);
			return Result<Plan>.Success(plan);
		}
		catch (Exception ex) when (ex is InvalidPlanNameException
							|| ex is InvalidPlanPriceException
							|| ex is InvalidPlanDescriptionException
							|| ex is InvalidPlanSimultaneousScreensException)
		{
			Error error = new("PlanService.CreatePLanAsync", ex.InnerException?.Message ?? ex.Message);
			return Result<Plan>.Failure(error);
		}
	}

	public async Task<Result<Plan>> AdjustPlanMaxSimultaneousScreensAsync(Plan plan, int newMaxSimultaneousScreens)
	{
		if (await _repository.ExistsByScreensAsync(newMaxSimultaneousScreens))
		{
			Error error = new("PlanService.CreatePLanAsync", $"Plan with {newMaxSimultaneousScreens} screens already exists.");
			return Result<Plan>.Failure(error);
		}

		try
		{
			plan.AdjustMaxSimultaneousScreens(newMaxSimultaneousScreens);
			return Result<Plan>.Success(plan);
		}
		catch (InvalidPlanSimultaneousScreensException ex)
		{
			Error error = new("PlanService.CreatePLanAsync", ex.InnerException?.Message ?? ex.Message);
			return Result<Plan>.Failure(error);
		}
	}

	public async Task<Result<Plan>> AdjustPlanPriceAsync(Plan plan, Money newPrice)
	{
		if (await _repository.ExistsByPriceAsync(newPrice))
		{
			Error error = new("PlanService.CreatePLanAsync", $"Plan with price {newPrice.Currency} {newPrice.Amount} already exists.");
			return Result<Plan>.Failure(error);
		}

		try
		{
			plan.AdjustPrice(newPrice);
			return Result<Plan>.Success(plan);
		}
		catch (InvalidPlanPriceException ex)
		{
			Error error = new("PlanService.CreatePLanAsync", ex.InnerException?.Message ?? ex.Message);
			return Result<Plan>.Failure(error);
		}
	}
}