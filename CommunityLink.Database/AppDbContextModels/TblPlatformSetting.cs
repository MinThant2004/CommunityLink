using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPlatformSetting
{
    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
