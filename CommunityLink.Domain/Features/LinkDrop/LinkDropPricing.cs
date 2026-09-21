namespace CommunityLink.Domain.Features.LinkDrop;

public static class LinkDropPricing
{
    public const string DefaultCurrency = "MMK";

    // 1 Link Drop = 10 MMK for custom (manual) purchases.
    // Seeded packages must stay consistent: 100 Drops = 1,000 MMK, 200 Drops = 2,000 MMK.
    public const decimal MmkPerDrop = 10.0m;

    public static long DropsFromMoney(decimal moneyMmk) =>
        (long)(moneyMmk / MmkPerDrop);

    public static decimal MoneyFromDrops(long drops) =>
        drops * MmkPerDrop;
}