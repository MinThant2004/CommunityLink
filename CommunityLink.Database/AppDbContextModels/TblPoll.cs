using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPoll
{
    public int PollId { get; set; }

    public int PostId { get; set; }

    public string Question { get; set; } = null!;

    public bool IsMultipleChoice { get; set; }

    public int TotalVotes { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblPost Post { get; set; } = null!;

    public virtual ICollection<TblPollOption> TblPollOptions { get; set; } = new List<TblPollOption>();

    public virtual ICollection<TblPollVote> TblPollVotes { get; set; } = new List<TblPollVote>();
}
