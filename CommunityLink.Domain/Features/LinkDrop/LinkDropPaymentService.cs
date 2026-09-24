using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared.Features.LinkDrop;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.LinkDrop;

public class LinkDropPaymentService : ILinkDropPaymentService
{
    private readonly AppDbContext _db;

    public LinkDropPaymentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LinkDropPackageDto>> GetActivePackagesAsync()
    {
        return await _db.TblLinkDropPackages
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.PackageId)
            .Select(p => MapPackageToDto(p))
            .ToListAsync();
    }

    public async Task<List<PaymentMethodDto>> GetActivePaymentMethodsAsync()
    {
        return await _db.TblPaymentMethods
            .AsNoTracking()
            .Where(m => m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.PaymentMethodId)
            .Select(m => MapPaymentMethodToDto(m))
            .ToListAsync();
    }

    public async Task<PurchaseResponseDto> SubmitPurchaseAsync(
        int userId,
        CreatePurchaseRequestDto request,
        Stream? proofStream = null,
        string? fileName = null,
        string? contentType = null,
        long fileSize = 0)
    {
        var userExists = await _db.TblUsers.AnyAsync(u => u.UserId == userId && u.IsActive && !u.IsDeleted);
        if (!userExists)
            throw new InvalidOperationException("User not found or account is inactive.");

        var paymentMethod = await _db.TblPaymentMethods
            .FirstOrDefaultAsync(m => m.PaymentMethodId == request.PaymentMethodId && m.IsActive && !m.IsDeleted);
        if (paymentMethod == null)
            throw new InvalidOperationException("Selected payment method is invalid or inactive.");

        if (string.IsNullOrWhiteSpace(request.TransactionReferenceNo))
            throw new InvalidOperationException("Transaction reference number is required.");

        var cleanRefNo = request.TransactionReferenceNo.Trim();
        var isDuplicateRef = await _db.TblLinkDropPurchases
            .AnyAsync(p => p.TransactionReferenceNo == cleanRefNo && p.Status != "REJECTED" && !p.IsDeleted);
        if (isDuplicateRef)
            throw new InvalidOperationException("This Transaction ID has already been submitted for a purchase request.");

        var purchase = new TblLinkDropPurchase
        {
            PurchaseNumber = $"LDP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            UserId = userId,
            PaymentMethodId = request.PaymentMethodId,
            TransactionReferenceNo = cleanRefNo,
            UserNotes = request.UserNotes,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        if (!request.IsCustomPurchase && request.PackageId.HasValue)
        {
            var package = await _db.TblLinkDropPackages
                .FirstOrDefaultAsync(p => p.PackageId == request.PackageId.Value && p.IsActive && !p.IsDeleted);
            if (package == null)
                throw new InvalidOperationException("Selected package is invalid or inactive.");

            purchase.PackageId = package.PackageId;
            purchase.IsCustomPurchase = false;
            purchase.SnapshotPackageName = package.PackageName;
            purchase.SnapshotRealMoneyAmount = package.RealMoneyAmount;
            purchase.SnapshotLinkDropAmount = package.LinkDropAmount + package.BonusAmount;
            purchase.SnapshotCurrency = package.Currency;
            purchase.SnapshotConversionRate = null;
        }
        else
        {
            purchase.IsCustomPurchase = true;

            if (request.CustomRealMoneyAmount.HasValue && request.CustomRealMoneyAmount.Value > 0)
            {
                purchase.SnapshotRealMoneyAmount = request.CustomRealMoneyAmount.Value;
                purchase.SnapshotLinkDropAmount = LinkDropPricing.DropsFromMoney(request.CustomRealMoneyAmount.Value);
            }
            else if (request.CustomLinkDropAmount.HasValue && request.CustomLinkDropAmount.Value > 0)
            {
                purchase.SnapshotLinkDropAmount = request.CustomLinkDropAmount.Value;
                purchase.SnapshotRealMoneyAmount = LinkDropPricing.MoneyFromDrops(request.CustomLinkDropAmount.Value);
            }
            else
            {
                throw new InvalidOperationException("Custom purchase requires a valid real-money amount or Link Drop quantity.");
            }

            purchase.SnapshotPackageName = "Custom Purchase";
            purchase.SnapshotCurrency = LinkDropPricing.DefaultCurrency;
            purchase.SnapshotConversionRate = LinkDropPricing.MmkPerDrop;
        }

        _db.TblLinkDropPurchases.Add(purchase);

        // User submission notification
        var submissionNotification = new TblNotification
        {
            RecipientUserId = userId,
            NotificationType = "LINK_DROP_SUBMITTED",
            Title = "Link Drop Purchase Submitted",
            Message = $"Your Link Drop purchase request #{purchase.PurchaseNumber} ({purchase.SnapshotLinkDropAmount:N0} Drops) has been submitted and is pending admin verification.",
            TargetEntityName = "TblLinkDropPurchase",
            TargetEntityId = purchase.PurchaseId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.TblNotifications.Add(submissionNotification);

        await _db.SaveChangesAsync();

        // Handle Proof File Upload or URL
        TblLinkDropPurchaseProof? proofEntity = null;

        if (proofStream != null && !string.IsNullOrWhiteSpace(fileName))
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            if (!allowedExts.Contains(ext))
                throw new InvalidOperationException("Invalid file type. Only JPG, PNG, WEBP, and PDF receipts are allowed.");

            long actualSize = fileSize > 0 ? fileSize : proofStream.Length;
            if (actualSize > 5 * 1024 * 1024)
                throw new InvalidOperationException("Receipt proof file exceeds maximum allowed limit of 5 MB.");

            var yearMonth = DateTime.UtcNow.ToString("yyyy/MM");
            var relativeFolder = Path.Combine("uploads", "payment-proofs", yearMonth);
            var absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolder);
            Directory.CreateDirectory(absoluteFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var absoluteFilePath = Path.Combine(absoluteFolder, uniqueFileName);

            using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await proofStream.CopyToAsync(fileStream);
            }

            var webUrl = $"/{relativeFolder.Replace('\\', '/')}/{uniqueFileName}";

            proofEntity = new TblLinkDropPurchaseProof
            {
                PurchaseId = purchase.PurchaseId,
                FileUrl = webUrl,
                OriginalFileName = fileName,
                ContentType = contentType ?? "application/octet-stream",
                FileSize = actualSize,
                UploadedAt = DateTime.UtcNow
            };
            _db.TblLinkDropPurchaseProofs.Add(proofEntity);
            await _db.SaveChangesAsync();
        }
        else if (!string.IsNullOrWhiteSpace(request.ProofFileUrl))
        {
            proofEntity = new TblLinkDropPurchaseProof
            {
                PurchaseId = purchase.PurchaseId,
                FileUrl = request.ProofFileUrl.Trim(),
                OriginalFileName = Path.GetFileName(request.ProofFileUrl),
                ContentType = "image/jpeg",
                FileSize = 0,
                UploadedAt = DateTime.UtcNow
            };
            _db.TblLinkDropPurchaseProofs.Add(proofEntity);
            await _db.SaveChangesAsync();
        }

        return await GetPurchaseByIdInternalAsync(purchase.PurchaseId)
               ?? throw new InvalidOperationException("Failed to retrieve created purchase request.");
    }

    public async Task<List<PurchaseResponseDto>> GetUserPurchasesAsync(int userId, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var purchases = await _db.TblLinkDropPurchases
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Package)
            .Include(p => p.PaymentMethod)
            .Include(p => p.ReviewedByAdmin)
            .Include(p => p.TblLinkDropPurchaseProof)
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return purchases.Select(p => MapPurchaseToDto(p)).ToList();
    }

    public async Task<PurchaseResponseDto?> GetPurchaseByIdAsync(int userId, int purchaseId)
    {
        var purchase = await _db.TblLinkDropPurchases
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Package)
            .Include(p => p.PaymentMethod)
            .Include(p => p.ReviewedByAdmin)
            .Include(p => p.TblLinkDropPurchaseProof)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.UserId == userId && !p.IsDeleted);

        return purchase != null ? MapPurchaseToDto(purchase) : null;
    }

    public async Task<LinkDropWalletDto> GetUserWalletAsync(int userId)
    {
        var wallet = await _db.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                Balance = 0,
                PurchasedBalance = 0,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _db.TblLinkDropWallets.Add(wallet);
            await _db.SaveChangesAsync();
        }
        else if (wallet.PurchasedBalance == 0 && wallet.EarnedBalance == 0 && wallet.Balance > 0)
        {
            wallet.PurchasedBalance = wallet.Balance;
            await _db.SaveChangesAsync();
        }

        return MapWalletToDto(wallet);
    }

    public async Task<List<LinkDropTransactionDto>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var transactions = await _db.TblLinkDropTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapTransactionToDto(t))
            .ToListAsync();

        return transactions;
    }

    // =========================================================================
    // ADMIN METHODS
    // =========================================================================

    public async Task<List<PurchaseResponseDto>> GetPendingPurchasesAsync(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var purchases = await _db.TblLinkDropPurchases
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Package)
            .Include(p => p.PaymentMethod)
            .Include(p => p.ReviewedByAdmin)
            .Include(p => p.TblLinkDropPurchaseProof)
            .Where(p => p.Status == "PENDING" && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return purchases.Select(p => MapPurchaseToDto(p)).ToList();
    }

    public async Task<PurchaseResponseDto> ApprovePurchaseAsync(int adminId, int purchaseId, string? notes = null)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;
        if (_db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            tx = await _db.Database.BeginTransactionAsync();
        }

        try
        {
            var purchase = await _db.TblLinkDropPurchases
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && !p.IsDeleted);

            if (purchase == null)
                throw new InvalidOperationException($"Purchase request #{purchaseId} was not found.");

            if (purchase.Status != "PENDING")
                throw new InvalidOperationException($"Purchase request #{purchaseId} is already in state '{purchase.Status}'. Only PENDING requests can be approved.");

            // 1. Update purchase request status
            purchase.Status = "APPROVED";
            purchase.ReviewedByAdminId = adminId;
            purchase.ReviewedAt = DateTime.UtcNow;
            purchase.UpdatedAt = DateTime.UtcNow;
            purchase.UpdatedBy = adminId;

            // 2. Fetch or create user wallet
            var wallet = await _db.TblLinkDropWallets
                .FirstOrDefaultAsync(w => w.UserId == purchase.UserId);

            if (wallet == null)
            {
                wallet = new TblLinkDropWallet
                {
                    UserId = purchase.UserId,
                    Balance = 0,
                    PurchasedBalance = 0,
                    EarnedBalance = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId
                };
                _db.TblLinkDropWallets.Add(wallet);
                await _db.SaveChangesAsync();
            }

            // Runtime Data Migration Safety: Initialize PurchasedBalance if legacy wallet had Balance > 0 but Purchased/Earned = 0
            if (wallet.PurchasedBalance == 0 && wallet.EarnedBalance == 0 && wallet.Balance > 0)
            {
                wallet.PurchasedBalance = wallet.Balance;
            }

            long purchasedCredit = purchase.SnapshotLinkDropAmount;
            long bonusCredit = 0;

            if (purchase.PackageId.HasValue)
            {
                var package = await _db.TblLinkDropPackages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PackageId == purchase.PackageId.Value);

                if (package != null)
                {
                    purchasedCredit = package.LinkDropAmount;
                    bonusCredit = package.BonusAmount;
                }
            }

            long pBefore = wallet.PurchasedBalance;
            long eBefore = wallet.EarnedBalance;
            long balanceBefore = wallet.Balance;

            wallet.PurchasedBalance += purchasedCredit;
            wallet.EarnedBalance += bonusCredit; // Existing EarnedBalance is preserved and incremented by bonusCredit
            wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance; // Maintain invariant: Balance = PurchasedBalance + EarnedBalance
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.UpdatedBy = adminId;

            // 3. Insert transaction ledger entry with source-separated balance snapshots
            var transaction = new TblLinkDropTransaction
            {
                WalletId = wallet.WalletId,
                UserId = purchase.UserId,
                TransactionType = "PURCHASE",
                Amount = purchase.SnapshotLinkDropAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                PurchasedBalanceBefore = pBefore,
                PurchasedBalanceAfter = wallet.PurchasedBalance,
                EarnedBalanceBefore = eBefore,
                EarnedBalanceAfter = wallet.EarnedBalance,
                PurchasedAmountDeducted = 0,
                EarnedAmountDeducted = 0,
                ReferenceType = "TblLinkDropPurchase",
                ReferenceId = purchase.PurchaseId,
                Notes = notes ?? $"Link Drops Purchase #{purchase.PurchaseNumber} Approved",
                CreatedAt = DateTime.UtcNow
            };

            _db.TblLinkDropTransactions.Add(transaction);

            // 4. Create user approval notification
            var approvalNotification = new TblNotification
            {
                RecipientUserId = purchase.UserId,
                ActorUserId = adminId,
                NotificationType = "LINK_DROP_APPROVED",
                Title = "Link Drop Purchase Approved!",
                Message = $"Your Link Drop purchase request #{purchase.PurchaseNumber} has been approved! {purchase.SnapshotLinkDropAmount:N0} Link Drops have been added to your wallet.",
                TargetEntityName = "TblLinkDropPurchase",
                TargetEntityId = purchase.PurchaseId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.TblNotifications.Add(approvalNotification);

            await _db.SaveChangesAsync();
            if (tx != null) await tx.CommitAsync();

            return await GetPurchaseByIdInternalAsync(purchaseId)
                   ?? throw new InvalidOperationException("Failed to retrieve approved purchase detail.");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_TblLinkDropTransaction_Purchase_Reference") == true)
        {
            if (tx != null) await tx.RollbackAsync();
            throw new InvalidOperationException("Idempotency violation: This purchase request has already been credited.");
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync();
            throw;
        }
        finally
        {
            tx?.Dispose();
        }
    }

    public async Task<PurchaseResponseDto> RejectPurchaseAsync(int adminId, int purchaseId, string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new InvalidOperationException("Rejection reason is required.");

        var purchase = await _db.TblLinkDropPurchases
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && !p.IsDeleted);

        if (purchase == null)
            throw new InvalidOperationException($"Purchase request #{purchaseId} was not found.");

        if (purchase.Status != "PENDING")
            throw new InvalidOperationException($"Purchase request #{purchaseId} is already in state '{purchase.Status}'. Only PENDING requests can be rejected.");

        purchase.Status = "REJECTED";
        purchase.RejectionReason = rejectionReason.Trim();
        purchase.ReviewedByAdminId = adminId;
        purchase.ReviewedAt = DateTime.UtcNow;
        purchase.UpdatedAt = DateTime.UtcNow;
        purchase.UpdatedBy = adminId;

        // User rejection notification
        var rejectionNotification = new TblNotification
        {
            RecipientUserId = purchase.UserId,
            ActorUserId = adminId,
            NotificationType = "LINK_DROP_REJECTED",
            Title = "Link Drop Purchase Rejected",
            Message = $"Your Link Drop purchase request #{purchase.PurchaseNumber} was rejected. Reason: {purchase.RejectionReason}",
            TargetEntityName = "TblLinkDropPurchase",
            TargetEntityId = purchase.PurchaseId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.TblNotifications.Add(rejectionNotification);

        await _db.SaveChangesAsync();

        return await GetPurchaseByIdInternalAsync(purchaseId)
               ?? throw new InvalidOperationException("Failed to retrieve rejected purchase detail.");
    }

    public async Task<List<LinkDropPackageDto>> GetAllPackagesAsync()
    {
        return await _db.TblLinkDropPackages
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.PackageId)
            .Select(p => MapPackageToDto(p))
            .ToListAsync();
    }

    public async Task<LinkDropPackageDto> CreatePackageAsync(int adminId, CreatePackageRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PackageName))
            throw new InvalidOperationException("Package name is required.");
        if (request.LinkDropAmount <= 0)
            throw new InvalidOperationException("Link drop amount must be greater than 0.");
        if (request.RealMoneyAmount <= 0)
            throw new InvalidOperationException("Real money amount must be greater than 0.");

        var package = new TblLinkDropPackage
        {
            PackageName = request.PackageName.Trim(),
            Description = request.Description?.Trim(),
            LinkDropAmount = request.LinkDropAmount,
            BonusAmount = request.BonusAmount >= 0 ? request.BonusAmount : 0,
            RealMoneyAmount = request.RealMoneyAmount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpper(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminId,
            IsDeleted = false
        };

        _db.TblLinkDropPackages.Add(package);
        await _db.SaveChangesAsync();

        return MapPackageToDto(package);
    }

    public async Task<LinkDropPackageDto> UpdatePackageAsync(int adminId, int packageId, UpdatePackageRequestDto request)
    {
        var package = await _db.TblLinkDropPackages
            .FirstOrDefaultAsync(p => p.PackageId == packageId && !p.IsDeleted);

        if (package == null)
            throw new InvalidOperationException($"Package #{packageId} was not found.");

        package.PackageName = request.PackageName.Trim();
        package.Description = request.Description?.Trim();
        package.LinkDropAmount = request.LinkDropAmount;
        package.BonusAmount = request.BonusAmount >= 0 ? request.BonusAmount : 0;
        package.RealMoneyAmount = request.RealMoneyAmount;
        package.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpper();
        package.DisplayOrder = request.DisplayOrder;
        package.IsActive = request.IsActive;
        package.UpdatedAt = DateTime.UtcNow;
        package.UpdatedBy = adminId;

        await _db.SaveChangesAsync();

        return MapPackageToDto(package);
    }

    public async Task<bool> TogglePackageStatusAsync(int adminId, int packageId)
    {
        var package = await _db.TblLinkDropPackages
            .FirstOrDefaultAsync(p => p.PackageId == packageId && !p.IsDeleted);

        if (package == null)
            return false;

        package.IsActive = !package.IsActive;
        package.UpdatedAt = DateTime.UtcNow;
        package.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        return package.IsActive;
    }

    public async Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync()
    {
        return await _db.TblPaymentMethods
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.PaymentMethodId)
            .Select(m => MapPaymentMethodToDto(m))
            .ToListAsync();
    }

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(int adminId, CreatePaymentMethodRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MethodName))
            throw new InvalidOperationException("Payment method name is required.");
        if (string.IsNullOrWhiteSpace(request.AccountName))
            throw new InvalidOperationException("Account name is required.");
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            throw new InvalidOperationException("Account number is required.");

        var method = new TblPaymentMethod
        {
            MethodName = request.MethodName.Trim(),
            AccountName = request.AccountName.Trim(),
            AccountNumber = request.AccountNumber.Trim(),
            QrCodeImageUrl = request.QrCodeImageUrl?.Trim(),
            Instructions = request.Instructions?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminId,
            IsDeleted = false
        };

        _db.TblPaymentMethods.Add(method);
        await _db.SaveChangesAsync();

        return MapPaymentMethodToDto(method);
    }

    public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(int adminId, int paymentMethodId, UpdatePaymentMethodRequestDto request)
    {
        var method = await _db.TblPaymentMethods
            .FirstOrDefaultAsync(m => m.PaymentMethodId == paymentMethodId && !m.IsDeleted);

        if (method == null)
            throw new InvalidOperationException($"Payment method #{paymentMethodId} was not found.");

        method.MethodName = request.MethodName.Trim();
        method.AccountName = request.AccountName.Trim();
        method.AccountNumber = request.AccountNumber.Trim();
        method.QrCodeImageUrl = request.QrCodeImageUrl?.Trim();
        method.Instructions = request.Instructions?.Trim();
        method.DisplayOrder = request.DisplayOrder;
        method.IsActive = request.IsActive;
        method.UpdatedAt = DateTime.UtcNow;
        method.UpdatedBy = adminId;

        await _db.SaveChangesAsync();

        return MapPaymentMethodToDto(method);
    }

    public async Task<bool> TogglePaymentMethodStatusAsync(int adminId, int paymentMethodId)
    {
        var method = await _db.TblPaymentMethods
            .FirstOrDefaultAsync(m => m.PaymentMethodId == paymentMethodId && !m.IsDeleted);

        if (method == null)
            return false;

        method.IsActive = !method.IsActive;
        method.UpdatedAt = DateTime.UtcNow;
        method.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        return method.IsActive;
    }

    // =========================================================================
    // HELPER MAPPERS & UTILS
    // =========================================================================

    private async Task<PurchaseResponseDto?> GetPurchaseByIdInternalAsync(int purchaseId)
    {
        var p = await _db.TblLinkDropPurchases
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Package)
            .Include(x => x.PaymentMethod)
            .Include(x => x.ReviewedByAdmin)
            .Include(x => x.TblLinkDropPurchaseProof)
            .FirstOrDefaultAsync(x => x.PurchaseId == purchaseId && !x.IsDeleted);

        return p != null ? MapPurchaseToDto(p) : null;
    }

    private static LinkDropPackageDto MapPackageToDto(TblLinkDropPackage p)
    {
        return new LinkDropPackageDto
        {
            PackageId = p.PackageId,
            PackageName = p.PackageName,
            Description = p.Description,
            LinkDropAmount = p.LinkDropAmount,
            BonusAmount = p.BonusAmount,
            RealMoneyAmount = p.RealMoneyAmount,
            Currency = p.Currency,
            DisplayOrder = p.DisplayOrder,
            IsActive = p.IsActive
        };
    }

    private static PaymentMethodDto MapPaymentMethodToDto(TblPaymentMethod m)
    {
        return new PaymentMethodDto
        {
            PaymentMethodId = m.PaymentMethodId,
            MethodName = m.MethodName,
            AccountName = m.AccountName,
            AccountNumber = m.AccountNumber,
            QrCodeImageUrl = m.QrCodeImageUrl,
            Instructions = m.Instructions,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive
        };
    }

    private static PurchaseResponseDto MapPurchaseToDto(TblLinkDropPurchase p)
    {
        return new PurchaseResponseDto
        {
            PurchaseId = p.PurchaseId,
            PurchaseNumber = p.PurchaseNumber,
            UserId = p.UserId,
            UserName = p.User?.DisplayName ?? p.User?.UserName,
            PackageId = p.PackageId,
            PackageName = p.Package?.PackageName ?? p.SnapshotPackageName,
            PaymentMethodId = p.PaymentMethodId,
            PaymentMethodName = p.PaymentMethod?.MethodName ?? "Manual Transfer",
            IsCustomPurchase = p.IsCustomPurchase,
            SnapshotPackageName = p.SnapshotPackageName,
            SnapshotRealMoneyAmount = p.SnapshotRealMoneyAmount,
            SnapshotLinkDropAmount = p.SnapshotLinkDropAmount,
            SnapshotCurrency = p.SnapshotCurrency,
            SnapshotConversionRate = p.SnapshotConversionRate,
            TransactionReferenceNo = p.TransactionReferenceNo,
            UserNotes = p.UserNotes,
            Status = p.Status,
            RejectionReason = p.RejectionReason,
            ReviewedByAdminId = p.ReviewedByAdminId,
            ReviewedByAdminName = p.ReviewedByAdmin?.FullName,
            ReviewedAt = p.ReviewedAt,
            CreatedAt = p.CreatedAt,
            Proof = p.TblLinkDropPurchaseProof != null ? new PurchaseProofDto
            {
                ProofId = p.TblLinkDropPurchaseProof.ProofId,
                PurchaseId = p.TblLinkDropPurchaseProof.PurchaseId,
                FileUrl = p.TblLinkDropPurchaseProof.FileUrl,
                OriginalFileName = p.TblLinkDropPurchaseProof.OriginalFileName,
                ContentType = p.TblLinkDropPurchaseProof.ContentType,
                FileSize = p.TblLinkDropPurchaseProof.FileSize,
                UploadedAt = p.TblLinkDropPurchaseProof.UploadedAt
            } : null
        };
    }

    private static LinkDropWalletDto MapWalletToDto(TblLinkDropWallet w)
    {
        return new LinkDropWalletDto
        {
            WalletId = w.WalletId,
            UserId = w.UserId,
            Balance = w.Balance,
            PurchasedBalance = w.PurchasedBalance,
            EarnedBalance = w.EarnedBalance,
            UpdatedAt = w.UpdatedAt ?? w.CreatedAt
        };
    }

    private static LinkDropTransactionDto MapTransactionToDto(TblLinkDropTransaction t)
    {
        return new LinkDropTransactionDto
        {
            TransactionId = t.TransactionId,
            WalletId = t.WalletId,
            UserId = t.UserId,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            PurchasedBalanceBefore = t.PurchasedBalanceBefore,
            PurchasedBalanceAfter = t.PurchasedBalanceAfter,
            EarnedBalanceBefore = t.EarnedBalanceBefore,
            EarnedBalanceAfter = t.EarnedBalanceAfter,
            PurchasedAmountDeducted = t.PurchasedAmountDeducted,
            EarnedAmountDeducted = t.EarnedAmountDeducted,
            RelatedUserId = t.RelatedUserId,
            RelatedGroupId = t.RelatedGroupId,
            ReferenceType = t.ReferenceType,
            ReferenceId = t.ReferenceId,
            Notes = t.Notes,
            CreatedAt = t.CreatedAt
        };
    }
}
