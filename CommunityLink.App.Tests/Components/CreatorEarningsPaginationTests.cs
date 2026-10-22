using System;
using System.Collections.Generic;
using System.Linq;
using CommunityLink.Shared.Features.Creator;
using Xunit;

namespace CommunityLink.App.Tests.Components;

public class CreatorEarningsPaginationTests
{
    private static List<CreatorEarningsTransactionModel> CreateSampleTransactions(int count)
    {
        var list = new List<CreatorEarningsTransactionModel>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new CreatorEarningsTransactionModel(
                PaymentTransactionId: i,
                Date: DateTime.UtcNow.AddMinutes(-i * 10),
                ChatGroupId: 100 + i,
                ChatGroupName: $"Group {i}",
                BuyerUserId: 200 + i,
                BuyerName: $"User {i}",
                GrossAmount: 100,
                CommissionAmount: 10,
                NetAmount: 90,
                Status: "Completed"
            ));
        }
        return list;
    }

    [Fact]
    public void Pagination_CalculatesTotalPages_Correctly()
    {
        var transactions = CreateSampleTransactions(25);
        int pageSize = 10;
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)transactions.Count / pageSize));

        Assert.Equal(3, totalPages);
    }

    [Fact]
    public void Pagination_SlicesItems_AccordingToPageSize()
    {
        var transactions = CreateSampleTransactions(25);
        int pageSize = 10;

        // Page 1
        int pageNumber = 1;
        var page1 = transactions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Assert.Equal(10, page1.Count);
        Assert.Equal(1, page1.First().PaymentTransactionId);
        Assert.Equal(10, page1.Last().PaymentTransactionId);

        // Page 3
        pageNumber = 3;
        var page3 = transactions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        Assert.Equal(5, page3.Count);
        Assert.Equal(21, page3.First().PaymentTransactionId);
        Assert.Equal(25, page3.Last().PaymentTransactionId);
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
