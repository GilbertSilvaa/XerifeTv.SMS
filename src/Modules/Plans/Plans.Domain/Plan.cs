using Plans.Domain.Events;
using Plans.Domain.Exceptions;
using SharedKernel;

namespace Plans.Domain;

public sealed class Plan : AggregateRoot
{
	public string Name { get; private set; } = default!;
	public string Description { get; private set; } = default!;
	public int MaxSimultaneousScreens { get; private set; }
	public Money Price { get; private set; } = default!;

	private Plan() { }

	private Plan(string name, string description, int maxSimultaneousScreens, Money price)
	{
		Name = name;
		Description = description;
		MaxSimultaneousScreens = maxSimultaneousScreens;
		Price = price;

		EnsureValidState();
	}

	public static Plan Create(string name, string description, int maxSimultaneousScreens, Money price)
	{
		var plan = new Plan(name, description, maxSimultaneousScreens, price);

		plan.AddDomainEvent(new PlanCreatedDomainEvent(
			plan.Id,
			plan.Name,
			plan.Description,
			plan.MaxSimultaneousScreens,
			plan.Price));

		return plan;
	}

	public void AdjustMaxSimultaneousScreens(int newMaxSimultaneousScreens)
	{
		if (newMaxSimultaneousScreens == MaxSimultaneousScreens) return;

		MaxSimultaneousScreens = newMaxSimultaneousScreens;
		EnsureValidState();
		AddDomainEvent(new PlanSimultaneousScreensAjustedDomainEvent(Id, newMaxSimultaneousScreens));
	}

	public void AdjustPrice(Money newPrice)
	{
		if (Price.Equals(newPrice)) return;

		Price = newPrice;
		EnsureValidState();
		AddDomainEvent(new PlanPriceAdjustedDomainEvent(Id, newPrice));
	}

	public override bool Delete()
	{
		bool isDeleted = base.Delete();

		if (isDeleted) AddDomainEvent(new PlanDeletedDomainEvent(Id));

		return isDeleted;
	}

	private void EnsureValidState()
	{
		if (string.IsNullOrWhiteSpace(Name) || Name.Length < 5)
			throw new InvalidPlanNameException(Name);

		if (string.IsNullOrWhiteSpace(Description) || Description.Length < 10)
			throw new InvalidPlanDescriptionException();

		if (MaxSimultaneousScreens < 1)
			throw new InvalidPlanSimultaneousScreensException(MaxSimultaneousScreens);

		if (Price.Amount <= 0)
			throw new InvalidPlanPriceException();
	}
}