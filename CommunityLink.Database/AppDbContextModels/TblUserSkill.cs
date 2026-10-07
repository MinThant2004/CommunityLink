using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserSkill
{
    public int SkillId { get; set; }

    public int UserId { get; set; }

    public string SkillName { get; set; } = null!;

    public int EndorsementCount { get; set; }

    public bool IsVerified { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<TblSkillEndorsement> TblSkillEndorsements { get; set; } = new List<TblSkillEndorsement>();

    public virtual TblUser User { get; set; } = null!;
}
