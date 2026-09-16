using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblCommunity
{
    public int CommunityId { get; set; }

    public int? ParentCommunityId { get; set; }

    public int OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string Visibility { get; set; } = null!;

    public string JoinPolicy { get; set; } = null!;

    public int MemberCount { get; set; }

    public int PostCount { get; set; }

    public decimal? AverageRating { get; set; }

    public int RatingCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<TblCommunity> InverseParentCommunity { get; set; } = new List<TblCommunity>();

    public virtual TblUser Owner { get; set; } = null!;

    public virtual TblCommunity? ParentCommunity { get; set; }

    public virtual ICollection<TblCommunityJoinRequest> TblCommunityJoinRequests { get; set; } = new List<TblCommunityJoinRequest>();

    public virtual ICollection<TblCommunityMember> TblCommunityMembers { get; set; } = new List<TblCommunityMember>();

    public virtual ICollection<TblCommunityRating> TblCommunityRatings { get; set; } = new List<TblCommunityRating>();

    public virtual ICollection<TblPostShare> TblPostShares { get; set; } = new List<TblPostShare>();

    public virtual ICollection<TblPost> TblPosts { get; set; } = new List<TblPost>();
}
