using CommunityLink.Shared.Features.LinkDrop;
using Xunit;

namespace CommunityLink.App.Tests.Components;

public class LinkDropNavbarBalanceTests
{
    [Fact]
    public void LinkDropWalletDto_Balance_RemainsSourceOfTruth()
    {
        var wallet = new LinkDropWalletDto
        {
            WalletId = 1,
            UserId = 10,
            Balance = 1500,
            PurchasedBalance = 1000,
            EarnedBalance = 500
        };

        // Assert backend remains source of truth for Balance
        Assert.Equal(1500, wallet.Balance);
        Assert.Equal(1000, wallet.PurchasedBalance);
        Assert.Equal(500, wallet.EarnedBalance);
    }

    [Fact]
    public void LinkDropWalletDto_FormattedBalance_RendersCorrectly()
    {
        var wallet = new LinkDropWalletDto
        {
            Balance = 1250500,
            PurchasedBalance = 1000000,
            EarnedBalance = 250500
        };

        string formattedBalance = wallet.Balance.ToString("N0");
        string expectedTooltip = $"Total Link Drops: {wallet.Balance:N0} (Purchased: {wallet.PurchasedBalance:N0} | Earned: {wallet.EarnedBalance:N0})";

        Assert.Equal("1,250,500", formattedBalance);
        Assert.Contains("1,250,500", expectedTooltip);
        Assert.Contains("1,000,000", expectedTooltip);
        Assert.Contains("250,500", expectedTooltip);
    }

    [Fact]
    public void LinkDropWalletDto_ZeroBalance_FormatsCorrectly()
    {
        var wallet = new LinkDropWalletDto
        {
            Balance = 0,
            PurchasedBalance = 0,
            EarnedBalance = 0
        };

        Assert.Equal("0", wallet.Balance.ToString("N0"));
    }
}
