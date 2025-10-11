using SharedKernel;

namespace BuildingBlocks.Mapping;

public class MapsterConfig : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Money, Money>();
		config.NewConfig<Money, Money>()
			  .MapWith(src => Money.From(src.Amount, src.Currency));
	}
}
