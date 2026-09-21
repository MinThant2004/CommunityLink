using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.LinkDrop;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class LinkDropPaymentEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetAdminTokenAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("admin@communitylink.local", "Admin@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    [Fact]
    public async Task GetActivePackages_ReturnsOkAndPackagesList()
    {
        var response = await _client.GetAsync("/api/linkdrops/packages");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<List<LinkDropPackageDto>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetActivePaymentMethods_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/linkdrops/payment-methods");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserWallet_WithAuthenticatedUser_ReturnsWallet()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/linkdrops/wallet");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<LinkDropWalletDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Balance >= 0);
    }

    [Fact]
    public async Task Admin_CreatePaymentMethod_And_User_SubmitCustomPurchase_And_Admin_Approve_Flow()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Admin creates a payment method
        var createMethodRequest = new CreatePaymentMethodRequestDto
        {
            MethodName = "KBZPay Test",
            AccountName = "Community Link Official",
            AccountNumber = "09123456789",
            Instructions = "Transfer with note",
            DisplayOrder = 1
        };

        var methodRes = await _client.PostAsJsonAsync("/api/admin/linkdrops/payment-methods", createMethodRequest);
        Assert.Equal(HttpStatusCode.OK, methodRes.StatusCode);
        var methodData = await methodRes.Content.ReadFromJsonAsync<Result<PaymentMethodDto>>();
        Assert.NotNull(methodData?.Data);
        int paymentMethodId = methodData.Data.PaymentMethodId;

        // 2. User submits a custom purchase request (@ 10 MMK per Drop)
        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(paymentMethodId.ToString()), "PaymentMethodId");
        formData.Add(new StringContent("true"), "IsCustomPurchase");
        formData.Add(new StringContent("1500.00"), "CustomRealMoneyAmount");
        formData.Add(new StringContent("REF-TEST-123456"), "TransactionReferenceNo");
        formData.Add(new StringContent("Testing payment submit"), "UserNotes");

        var submitRes = await _client.PostAsync("/api/linkdrops/purchases", formData);
        Assert.Equal(HttpStatusCode.OK, submitRes.StatusCode);
        var submitData = await submitRes.Content.ReadFromJsonAsync<Result<PurchaseResponseDto>>();
        Assert.NotNull(submitData?.Data);
        Assert.Equal("PENDING", submitData.Data.Status);
        int purchaseId = submitData.Data.PurchaseId;

        // 3. Admin gets pending queue
        var pendingRes = await _client.GetAsync("/api/admin/linkdrops/purchases");
        Assert.Equal(HttpStatusCode.OK, pendingRes.StatusCode);
        var pendingData = await pendingRes.Content.ReadFromJsonAsync<Result<List<PurchaseResponseDto>>>();
        Assert.NotNull(pendingData?.Data);
        Assert.Contains(pendingData.Data, p => p.PurchaseId == purchaseId);

        // 4. Admin approves the purchase
        var approveRes = await _client.PostAsJsonAsync($"/api/admin/linkdrops/purchases/{purchaseId}/approve", new ApprovePurchaseDto { Notes = "Verified payment" });
        Assert.Equal(HttpStatusCode.OK, approveRes.StatusCode);
        var approveData = await approveRes.Content.ReadFromJsonAsync<Result<PurchaseResponseDto>>();
        Assert.NotNull(approveData?.Data);
        Assert.Equal("APPROVED", approveData.Data.Status);

        // 5. Check user wallet balance increased
        var walletRes = await _client.GetAsync("/api/linkdrops/wallet");
        var walletData = await walletRes.Content.ReadFromJsonAsync<Result<LinkDropWalletDto>>();
        Assert.NotNull(walletData?.Data);
        Assert.Equal(150, walletData.Data.Balance); // 1500 MMK / 10 = 150 drops

        // 6. Check transaction ledger entry
        var txRes = await _client.GetAsync("/api/linkdrops/transactions");
        var txData = await txRes.Content.ReadFromJsonAsync<Result<List<LinkDropTransactionDto>>>();
        Assert.NotNull(txData?.Data);
        Assert.Contains(txData.Data, t => t.ReferenceId == purchaseId && t.TransactionType == "PURCHASE" && t.Amount == 150);
    }
}
