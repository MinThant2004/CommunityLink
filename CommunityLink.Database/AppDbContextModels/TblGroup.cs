using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblGroup
{
    public int GroupId { get; set; }

    public int SubCommunityId { get; set; }

    public int CreatorId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string Visibility { get; set; } = null!;

    public string JoinPolicy { get; set; } = null!;

    public int MemberCount { get; set; }

    public int PostCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public long JoinFeeLinkDrops { get; set; }

    public decimal CommissionPercentageSnapshot { get; set; }

    public virtual TblUser Creator { get; set; } = null!;

    public virtual TblCommunity SubCommunity { get; set; } = null!;

    public virtual ICollection<TblGroupJoinRequest> TblGroupJoinRequests { get; set; } = new List<TblGroupJoinRequest>();

    public virtual ICollection<TblGroupMember> TblGroupMembers { get; set; } = new List<TblGroupMember>();

    public virtual ICollection<TblGroupRating> TblGroupRatings { get; set; } = new List<TblGroupRating>();

    public virtual ICollection<TblPost> TblPosts { get; set; } = new List<TblPost>();
}
