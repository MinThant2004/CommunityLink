using System;
using System.Collections.Generic;
using System.Linq;
using CommunityLink.Shared.Features.Payout;
using Xunit;

namespace CommunityLink.App.Tests.Components;

public class CreatorPayoutsPaginationTests
{
    private static List<CreatorPayoutModel> CreateSamplePayouts(int count)
    {
        var list = new List<CreatorPayoutModel>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new CreatorPayoutModel(
                CreatorPayoutRequestId: i,
                CreatorUserId: 43,
                AmountLinkDrops: 100 * i,
                AmountMMK: 10000 * i,
                PaymentMethod: "KBZPay",
                PaymentAccountName: $"Test User {i}",
                PaymentAccountNumber: $"09999999{i:D2}",
                Status: i % 2 == 0 ? "COMPLETED" : "PENDING",
                AdminNote: null,
                CreatedAt: DateTime.UtcNow.AddHours(-i),
                ReviewedAt: null
            ));
        }
        return list;
    }

    [Fact]
    public void Pagination_CalculatesTotalPages_Correctly()
    {
        var payouts = CreateSamplePayouts(25);
        int pageSize = 10;
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)payouts.Count / pageSize));

        Assert.Equal(3, totalPages);
    }

    [Fact]
    public void Pagination_SlicesItems_AccordingToPageSize()
    {
        var payouts = CreateSamplePayouts(25);
        int pageSize = 10;

        // Page 1
        int pageNumber = 1;
        var page1 = payouts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Assert.Equal(10, page1.Count);
        Assert.Equal(1, page1.First().CreatorPayoutRequestId);
        Assert.Equal(10, page1.Last().CreatorPayoutRequestId);

        // Page 3
        pageNumber = 3;
        var page3 = payouts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Assert.Equal(5, page3.Count);
        Assert.Equal(21, page3.First().CreatorPayoutRequestId);
        Assert.Equal(25, page3.Last().CreatorPayoutRequestId);
    }

    [Fact]
    public void Pagination_RowNumbers_AreOneBasedAndSequential()
    {
        int pageNumber = 2;
        int pageSize = 10;
        int index = 0; // First item on page 2

        int rowNo = (pageNumber - 1) * pageSize + index + 1;
        Assert.Equal(11, rowNo);

        index = 9; // Last item on page 2
        rowNo = (pageNumber - 1) * pageSize + index + 1;
        Assert.Equal(20, rowNo);
    }

    [Fact]
    public void Pagination_RangeStats_AreAccurate()
    {
        int totalCount = 25;
        int pageNumber = 3;
        int pageSize = 10;

        int startItem = (pageNumber - 1) * pageSize + 1;
        int endItem = Math.Min(pageNumber * pageSize, totalCount);

        Assert.Equal(21, startItem);
        Assert.Equal(25, endItem);
    }
}
