using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityLink.Database.AppDbContextModels;

[Table("TblPasswordResetOtp")]
public partial class TblPasswordResetOtp
{
    [Key]
    public int OtpId { get; set; }

    public string Email { get; set; } = null!;

    public string OtpCode { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
