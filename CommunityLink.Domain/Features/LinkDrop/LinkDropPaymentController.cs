using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.LinkDrop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Domain.Features.LinkDrop;

[Route("api/linkdrops")]
public class LinkDropPaymentController : BaseController
{
    private readonly ILinkDropPaymentService _linkDropService;
    private readonly ICurrentUserContext _currentUser;

    public LinkDropPaymentController(ILinkDropPaymentService linkDropService, ICurrentUserContext currentUser)
    {
        _linkDropService = linkDropService;
        _currentUser = currentUser;
    }

    // =========================================================================
    // USER ENDPOINTS
    // =========================================================================

    [HttpGet("packages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePackages()
    {
        var packages = await _linkDropService.GetActivePackagesAsync();
        return ToActionResult(Result<List<LinkDropPackageDto>>.Success(packages));
    }

    [HttpGet("rate")]
    [AllowAnonymous]
    public IActionResult GetConversionRate()
    {
        return ToActionResult(Result<decimal>.Success(LinkDropPricing.MmkPerDrop));
    }

    [HttpGet("payment-methods")]
    [Authorize]
    public async Task<IActionResult> GetActivePaymentMethods()
    {
        var methods = await _linkDropService.GetActivePaymentMethodsAsync();
        return ToActionResult(Result<List<PaymentMethodDto>>.Success(methods));
    }

    [HttpPost("purchases")]
    [Authorize]
    public async Task<IActionResult> SubmitPurchase([FromForm] CreatePurchaseRequestDto request, IFormFile? proofFile)
    {
        if (!_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        try
        {
            Stream? proofStream = null;
            string? fileName = null;
            string? contentType = null;
            long fileSize = 0;

            if (proofFile != null && proofFile.Length > 0)
            {
                proofStream = proofFile.OpenReadStream();
                fileName = proofFile.FileName;
                contentType = proofFile.ContentType;
                fileSize = proofFile.Length;
            }

            var purchase = await _linkDropService.SubmitPurchaseAsync(
                _currentUser.UserId.Value,
                request,
                proofStream,
                fileName,
                contentType,
                fileSize
            );

            return ToActionResult(Result<PurchaseResponseDto>.Success(purchase));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpGet("purchases")]
    [Authorize]
    public async Task<IActionResult> GetUserPurchases([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var purchases = await _linkDropService.GetUserPurchasesAsync(_currentUser.UserId.Value, page, pageSize);
        return ToActionResult(Result<List<PurchaseResponseDto>>.Success(purchases));
    }

    [HttpGet("purchases/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetPurchaseById(int id)
    {
        if (!_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var purchase = await _linkDropService.GetPurchaseByIdAsync(_currentUser.UserId.Value, id);
        if (purchase == null)
            return ToActionResult(Result.Failure($"Purchase #{id} was not found.", ResultStatus.NotFound));

        return ToActionResult(Result<PurchaseResponseDto>.Success(purchase));
    }

    [HttpGet("wallet")]
    [Authorize]
    public async Task<IActionResult> GetUserWallet()
    {
        if (!_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var wallet = await _linkDropService.GetUserWalletAsync(_currentUser.UserId.Value);
        return ToActionResult(Result<LinkDropWalletDto>.Success(wallet));
    }

    [HttpGet("transactions")]
    [Authorize]
    public async Task<IActionResult> GetUserTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var transactions = await _linkDropService.GetUserTransactionsAsync(_currentUser.UserId.Value, page, pageSize);
        return ToActionResult(Result<List<LinkDropTransactionDto>>.Success(transactions));
    }

    // =========================================================================
    // ADMIN ENDPOINTS
    // =========================================================================

    [HttpGet("/api/admin/linkdrops/purchases")]
    [Authorize]
    public async Task<IActionResult> GetPendingPurchases([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!_currentUser.IsAdmin)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        var pending = await _linkDropService.GetPendingPurchasesAsync(page, pageSize);
        return ToActionResult(Result<List<PurchaseResponseDto>>.Success(pending));
    }

    [HttpPost("/api/admin/linkdrops/purchases/{id:int}/approve")]
    [Authorize]
    public async Task<IActionResult> ApprovePurchase(int id, [FromBody] ApprovePurchaseDto? dto)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var approved = await _linkDropService.ApprovePurchaseAsync(_currentUser.UserId.Value, id, dto?.Notes);
            return ToActionResult(Result<PurchaseResponseDto>.Success(approved));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpPost("/api/admin/linkdrops/purchases/{id:int}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectPurchase(int id, [FromBody] RejectPurchaseDto dto)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var rejected = await _linkDropService.RejectPurchaseAsync(_currentUser.UserId.Value, id, dto.RejectionReason);
            return ToActionResult(Result<PurchaseResponseDto>.Success(rejected));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpGet("/api/admin/linkdrops/packages")]
    [Authorize]
    public async Task<IActionResult> GetAllPackages()
    {
        if (!_currentUser.IsAdmin)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        var packages = await _linkDropService.GetAllPackagesAsync();
        return ToActionResult(Result<List<LinkDropPackageDto>>.Success(packages));
    }

    [HttpPost("/api/admin/linkdrops/packages")]
    [Authorize]
    public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequestDto request)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var package = await _linkDropService.CreatePackageAsync(_currentUser.UserId.Value, request);
            return ToActionResult(Result<LinkDropPackageDto>.Success(package));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpPut("/api/admin/linkdrops/packages/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePackage(int id, [FromBody] UpdatePackageRequestDto request)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var package = await _linkDropService.UpdatePackageAsync(_currentUser.UserId.Value, id, request);
            return ToActionResult(Result<LinkDropPackageDto>.Success(package));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpPost("/api/admin/linkdrops/packages/{id:int}/toggle")]
    [Authorize]
    public async Task<IActionResult> TogglePackageStatus(int id)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        var isActive = await _linkDropService.TogglePackageStatusAsync(_currentUser.UserId.Value, id);
        return ToActionResult(Result<bool>.Success(isActive));
    }

    [HttpGet("/api/admin/linkdrops/payment-methods")]
    [Authorize]
    public async Task<IActionResult> GetAllPaymentMethods()
    {
        if (!_currentUser.IsAdmin)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        var methods = await _linkDropService.GetAllPaymentMethodsAsync();
        return ToActionResult(Result<List<PaymentMethodDto>>.Success(methods));
    }

    [HttpPost("/api/admin/linkdrops/payment-methods")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequestDto request)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var method = await _linkDropService.CreatePaymentMethodAsync(_currentUser.UserId.Value, request);
            return ToActionResult(Result<PaymentMethodDto>.Success(method));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpPut("/api/admin/linkdrops/payment-methods/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodRequestDto request)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        try
        {
            var method = await _linkDropService.UpdatePaymentMethodAsync(_currentUser.UserId.Value, id, request);
            return ToActionResult(Result<PaymentMethodDto>.Success(method));
        }
        catch (InvalidOperationException ex)
        {
            return ToActionResult(Result.Failure(ex.Message, ResultStatus.ValidationError));
        }
    }

    [HttpPost("/api/admin/linkdrops/payment-methods/{id:int}/toggle")]
    [Authorize]
    public async Task<IActionResult> TogglePaymentMethodStatus(int id)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UserId.HasValue)
            return ToActionResult(Result.Failure("Administrator privileges required.", ResultStatus.Forbidden));

        var isActive = await _linkDropService.TogglePaymentMethodStatusAsync(_currentUser.UserId.Value, id);
        return ToActionResult(Result<bool>.Success(isActive));
    }
}
