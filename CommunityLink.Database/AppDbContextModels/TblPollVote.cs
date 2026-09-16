using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPollVote
{
    public int PollVoteId { get; set; }

    public int PollId { get; set; }

    public int PollOptionId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblPoll Poll { get; set; } = null!;

    public virtual TblPollOption PollOption { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
