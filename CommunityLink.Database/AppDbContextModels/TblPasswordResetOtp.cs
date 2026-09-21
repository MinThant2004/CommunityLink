using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPasswordResetOtp
{
    public int OtpId { get; set; }

    public string Email { get; set; } = null!;

    public string OtpCode { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? VerifiedAt { get; set; }
}
