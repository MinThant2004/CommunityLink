using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblSkillEndorsement
{
    public int EndorsementId { get; set; }

    public int SkillId { get; set; }

    public int EndorserUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual TblUser EndorserUser { get; set; } = null!;

    public virtual TblUserSkill Skill { get; set; } = null!;
}
