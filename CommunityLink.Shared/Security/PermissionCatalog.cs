namespace CommunityLink.Shared.Security;

public sealed record PermissionDefinition(string Code, string Name, string Module, string? RoutePath, bool AdminDefault, bool MemberDefault);

public static class PermissionCatalog
{
    public const string PolicyPrefix = "Permission:";

    // Communities
    public const string CommunityView = "COMMUNITY.VIEW";
    public const string CommunityCreate = "COMMUNITY.CREATE";
    public const string CommunityManage = "COMMUNITY.MANAGE";
    public const string CommunityModerate = "COMMUNITY.MODERATE";

    // Posts & Feed
    public const string PostView = "POST.VIEW";
    public const string PostCreate = "POST.CREATE";
    public const string PostDelete = "POST.DELETE";
    public const string PostStandaloneCreate = "POST.STANDALONE.CREATE";

    // Polls
    public const string PollView = "POLL.VIEW";
    public const string PollVote = "POLL.VOTE";
    public const string PollCreate = "POLL.CREATE";

    // Groups
    public const string GroupView = "GROUP.VIEW";
    public const string GroupCreate = "GROUP.CREATE";
    public const string GroupManage = "GROUP.MANAGE";

    // Chat & Messaging
    public const string ChatAccess = "CHAT.ACCESS";
    public const string ChatSend = "CHAT.SEND";

    // Administration & RBAC
    public const string AdminUserView = "ADMIN.USER.VIEW";
    public const string AdminUserManage = "ADMIN.USER.MANAGE";
    public const string AdminRbacManage = "ADMIN.RBAC.MANAGE";

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new(CommunityView, "View Communities", "Community", "/communities", true, true),
        new(CommunityCreate, "Create Community", "Community", "/communities/create", true, false),
        new(CommunityManage, "Manage Community Settings", "Community", null, true, false),
        new(CommunityModerate, "Moderate Communities & Join Requests", "Moderation", "/admin/communities", true, false),
        new(GroupView, "View Groups", "Group", "/groups", true, true),
        new(GroupCreate, "Create Group", "Group", "/groups", true, true),
        new(GroupManage, "Manage Group Settings", "Group", null, true, false),
        new(PostView, "View Feed Posts", "Post", "/feed", true, true),
        new(PostCreate, "Create Post", "Post", "/feed", true, true),
        new(PostDelete, "Delete Post / Moderation", "Post", null, true, false),
        new(PostStandaloneCreate, "Create Standalone Post & Poll", "Post", null, true, false),
        new(PollView, "View Polls", "Poll", "/polls", true, true),
        new(PollVote, "Vote on Polls", "Poll", "/polls", true, true),
        new(PollCreate, "Create Polls", "Poll", "/polls/create", true, false),
        new(ChatAccess, "Access Chat Rooms", "Chat", "/chat", true, true),
        new(ChatSend, "Send Chat Messages", "Chat", "/chat", true, true),
        new(AdminUserView, "View Admin Users", "Administration", "/admin/users", true, false),
        new(AdminUserManage, "Manage Admin Users", "Administration", "/admin/users", true, false),
        new(AdminRbacManage, "Manage RBAC Matrix", "Administration", "/admin/rbac", true, false)
    ];

    public static IReadOnlyList<string> DefaultForRole(string roleCode) => roleCode.ToUpperInvariant() switch
    {
        "ADMIN" => All.Select(x => x.Code).ToArray(),
        "MODERATOR" => All.Where(x => x.AdminDefault || x.MemberDefault).Select(x => x.Code).ToArray(),
        "MEMBER" => All.Where(x => x.MemberDefault).Select(x => x.Code).ToArray(),
        "USER" => All.Where(x => x.MemberDefault).Select(x => x.Code).ToArray(),
        _ => []
    };
}