using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPollOption
{
    public int PollOptionId { get; set; }

    public int PollId { get; set; }

    public string OptionText { get; set; } = null!;

    public int VoteCount { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblPoll Poll { get; set; } = null!;

    public virtual ICollection<TblPollVote> TblPollVotes { get; set; } = new List<TblPollVote>();
}
