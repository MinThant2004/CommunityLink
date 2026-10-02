namespace CommunityLink.Domain.Features.LinkDrop;

public static class LinkDropPricing
{
    public const string DefaultCurrency = "MMK";

    // 1 Link Drop = 1 MMK as base exchange rate.
    // 100 Drops = 100 MMK, 200 Drops = 200 MMK, etc.
    public const decimal MmkPerDrop = 1.0m;

    public static long DropsFromMoney(decimal moneyMmk) =>
        (long)(moneyMmk / MmkPerDrop);

    public static decimal MoneyFromDrops(long drops) =>
        drops * MmkPerDrop;
}