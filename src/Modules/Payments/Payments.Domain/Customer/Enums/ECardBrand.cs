namespace Payments.Domain.Customer.Enums;

public enum ECardBrand
{
    VISA,
    MASTERCARD,
    ELO,
    AMERICAN_EXPRESS,
    HIPERCARD
}

public static class CardBrandExtensions
{
    public static ECardBrand ToCardBrand(this string? brand) =>
        brand?.Trim().ToLowerInvariant() switch
        {
            "visa" => ECardBrand.VISA,
            "mastercard" => ECardBrand.MASTERCARD,
            "elo" => ECardBrand.ELO,
            "amex" or "american_express" => ECardBrand.AMERICAN_EXPRESS,
            "hipercard" => ECardBrand.HIPERCARD,
            _ => ECardBrand.VISA
        };
}