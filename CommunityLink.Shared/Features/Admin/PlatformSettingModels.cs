namespace CommunityLink.Shared.Features.Admin;

public class PlatformCommissionSettingDto
{
    public decimal CommissionPercentage { get; set; } = 10.00m;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}

public class UpdateCommissionSettingRequestDto
{
    public decimal CommissionPercentage { get; set; }
}
