using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblAdminInvite
{
    public int InviteId { get; set; }

    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsSuperAdmin { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public virtual TblRole Role { get; set; } = null!;
}
