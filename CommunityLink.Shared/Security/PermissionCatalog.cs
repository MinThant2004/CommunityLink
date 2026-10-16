namespace CommunityLink.Shared.Security;

public sealed record PermissionDefinition(string Code, string Name, string Module, string? RoutePath, int Type, bool AdminDefault, bool MemberDefault);

public static class PermissionCatalog
{
    public const string PolicyPrefix = "Permission:";

    // User Permissions (Type = 0)
    public const string CommunityView = "COMMUNITY.VIEW";
    public const string GroupView = "GROUP.VIEW";
    public const string GroupCreate = "GROUP.CREATE";
    public const string PostView = "POST.VIEW";
    public const string PostCreate = "POST.CREATE";
    public const string PostStandaloneCreate = "POST.STANDALONE.CREATE";
    public const string PostDelete = "POST.DELETE";
    public const string PostManagement = "POST.MANAGEMENT";
    public const string PollView = "POLL.VIEW";
    public const string PollVote = "POLL.VOTE";
    public const string PollCreate = "POLL.CREATE";
    public const string ChatAccess = "CHAT.ACCESS";
    public const string ChatSend = "CHAT.SEND";
    public const string GroupChatCreate = "GROUPCHAT.CREATE";
    public const string ReportView = "REPORT.VIEW";
    public const string CreatorEarnings = "CREATOR.EARNINGS";
    public const string CreatorPayouts = "CREATOR.PAYOUTS";

    // Admin Permissions (Type = 1)
    public const string CommunityCreate = "COMMUNITY.CREATE";
    public const string SubCommunityCreate = "SUBCOMMUNITY.CREATE";
    public const string CommunityManage = "COMMUNITY.MANAGE";
    public const string CommunityModerate = "COMMUNITY.MODERATE";
    public const string GroupManage = "GROUP.MANAGE";
    public const string GroupManagement = "GROUP.MANAGEMENT";
    public const string AdminUserView = "ADMIN.USER.VIEW";
    public const string AdminUserManage = "ADMIN.USER.MANAGE";
    public const string AdminRbacManage = "ADMIN.RBAC.MANAGE";
    public const string SystemAuditView = "SYSTEMAUDIT.VIEW";

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        // User Domain Permissions (Type = 0)
        new(CommunityView, "View Communities", "Community", "/communities", 0, false, true),
        new(GroupView, "View Groups", "Group", "/groups", 0, false, true),
        new(GroupCreate, "Create Group", "Group", "/groups", 0, false, true),
        new(PostView, "View Feed Posts", "Post", "/feed", 0, false, true),
        new(PostCreate, "Create Post", "Post", "/feed", 0, false, true),
        new(PostStandaloneCreate, "Create Standalone Post & Poll", "Post", null, 0, false, true),
        new(PostDelete, "Delete Post / Moderation", "Post", null, 0, false, true),
        new(PostManagement, "Post & Poll Moderation", "Content Moderation", null, 0, false, false),
        new(PollView, "View Polls", "Poll", "/polls", 0, false, true),
        new(PollVote, "Vote on Polls", "Poll", "/polls", 0, false, true),
        new(PollCreate, "Create Polls", "Poll", "/polls/create", 0, false, true),
        new(ChatAccess, "Access Chat Rooms", "Chat", "/chat", 0, false, true),
        new(ChatSend, "Send Chat Messages", "Chat", "/chat", 0, false, true),
        new(GroupChatCreate, "Create Group Chat (Premium)", "Chat", null, 0, false, false),
        new(ReportView, "View Reports", "Reports", "/reports", 0, false, true),
        new(CreatorEarnings, "View Creator Earnings", "Creator", "/creator/earnings", 0, false, false),
        new(CreatorPayouts, "Manage Creator Payouts", "Creator", "/creator/payouts", 0, false, false),

        // Admin Domain Permissions (Type = 1)
        new(CommunityCreate, "Create Community", "Community", "/communities/create", 1, true, false),
        new(SubCommunityCreate, "Create Sub-Community", "Community", "/communities/create", 1, true, false),
        new(CommunityManage, "Manage Community Settings", "Community", null, 1, true, false),
        new(CommunityModerate, "Moderate Communities & Join Requests", "Moderation", "/admin/communities", 1, true, false),
        new(GroupManage, "Manage Group Settings", "Group", null, 1, true, false),
        new(GroupManagement, "Group Moderation & Active Status", "Content Moderation", "/admin/content-moderation?tab=groups", 1, true, false),
        new(AdminUserView, "View Admin Users", "Administration", "/admin/users", 1, true, false),
        new(AdminUserManage, "Manage Admin Users", "Administration", "/admin/users", 1, true, false),
        new(AdminRbacManage, "Manage RBAC Matrix", "Administration", "/admin/rbac", 1, true, false),
        new(SystemAuditView, "View System Audit Logs", "Administration", "/admin/audits", 1, true, false)
    ];

    public static IReadOnlyList<string> DefaultForRole(string roleCode) => roleCode.ToUpperInvariant() switch
    {
        "SYSTEM_ADMIN" or "SYSTEMADMIN" => All.Where(x => x.Type == 1).Select(x => x.Code).ToArray(),
        "ADMIN" => All.Where(x => x.Type == 1).Select(x => x.Code).ToArray(),
        "DEFAULT_USER" or "USER" or "MEMBER" => All.Where(x => x.Type == 0 && x.MemberDefault).Select(x => x.Code).ToArray(),
        "MODERATOR" => All.Where(x => x.Type == 0).Select(x => x.Code).ToArray(),
        "DOMAIN_PROFESSIONAL" or "DOMAIN_PRO" or "PUBLIC_FIGURE" => All.Where(x => x.Type == 0 && (x.MemberDefault || x.Code == GroupChatCreate || x.Code == CreatorEarnings || x.Code == CreatorPayouts)).Select(x => x.Code).ToArray(),
        _ => []
    };
}