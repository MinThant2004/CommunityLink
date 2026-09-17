using CommunityLink.Shared.Security;

namespace CommunityLink.App.Tests.Navigation;

public class NavMenuRenderTests
{
    [Fact]
    public void PermissionCatalog_ContainsAllCoreModules()
    {
        var modules = PermissionCatalog.All.Select(p => p.Module).Distinct().ToList();

        Assert.Contains("Community", modules);
        Assert.Contains("Post", modules);
        Assert.Contains("Poll", modules);
        Assert.Contains("Chat", modules);
        Assert.Contains("Administration", modules);
    }

    [Fact]
    public void PermissionCatalog_AdminRole_HasAllPermissions()
    {
        var adminPerms = PermissionCatalog.DefaultForRole("ADMIN");
        Assert.Equal(PermissionCatalog.All.Count, adminPerms.Count);
    }

    [Fact]
    public void PermissionCatalog_MemberRole_HasMemberDefaultPermissions()
    {
        var memberPerms = PermissionCatalog.DefaultForRole("MEMBER");
        var expectedCount = PermissionCatalog.All.Count(p => p.MemberDefault);
        Assert.Equal(expectedCount, memberPerms.Count);
    }
}