using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPost
{
    public int PostId { get; set; }

    public int AuthorId { get; set; }

    public int? CommunityId { get; set; }

    public string Content { get; set; } = null!;

    public string PostType { get; set; } = "STANDARD";

    public string? Subtitle { get; set; }

    public string? CodeSnippet { get; set; }

    public string? CodeLanguage { get; set; }

    public string? CodeFileName { get; set; }

    public string? DiagramImageUrl { get; set; }

    public string? DiagramCaption { get; set; }

    public bool HasPoll { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public int ShareCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int? GroupId { get; set; }

    public virtual TblUser Author { get; set; } = null!;

    public virtual TblCommunity? Community { get; set; }

    public virtual TblGroup? Group { get; set; }

    public virtual ICollection<TblComment> TblComments { get; set; } = new List<TblComment>();

    public virtual ICollection<TblPoll> TblPolls { get; set; } = new List<TblPoll>();

    public virtual ICollection<TblPostImage> TblPostImages { get; set; } = new List<TblPostImage>();

    public virtual ICollection<TblPostLike> TblPostLikes { get; set; } = new List<TblPostLike>();

    public virtual ICollection<TblPostShare> TblPostShares { get; set; } = new List<TblPostShare>();

    public virtual ICollection<TblSavedPost> TblSavedPosts { get; set; } = new List<TblSavedPost>();
}
