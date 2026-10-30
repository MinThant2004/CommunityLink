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
    public void PermissionCatalog_SystemAdminRole_HasAllAdminDomainPermissions()
    {
        var sysAdminPerms = PermissionCatalog.DefaultForRole("SYSTEM_ADMIN");
        var adminDomainCount = PermissionCatalog.All.Count(p => p.Type == 1);
        Assert.Equal(11, adminDomainCount);
        Assert.Equal(adminDomainCount, sysAdminPerms.Count);
        Assert.Contains(PermissionCatalog.AdminRbacManage, sysAdminPerms);
        Assert.Contains(PermissionCatalog.AdminUserManage, sysAdminPerms);
        Assert.Contains(PermissionCatalog.CommunityModerate, sysAdminPerms);
        Assert.Contains(PermissionCatalog.GroupManagement, sysAdminPerms);
        Assert.Contains(PermissionCatalog.ReportView, sysAdminPerms);
    }

    [Fact]
    public void PermissionCatalog_AdminRole_HasOperationalAdminPermissions_ExcludingMasterAdminControls()
    {
        var adminPerms = PermissionCatalog.DefaultForRole("ADMIN");
        Assert.Equal(9, adminPerms.Count);
        Assert.DoesNotContain(PermissionCatalog.AdminRbacManage, adminPerms);
        Assert.DoesNotContain(PermissionCatalog.AdminUserManage, adminPerms);
        Assert.Contains(PermissionCatalog.CommunityModerate, adminPerms);
        Assert.Contains(PermissionCatalog.GroupManagement, adminPerms);
        Assert.Contains(PermissionCatalog.AdminUserView, adminPerms);
        Assert.Contains(PermissionCatalog.SystemAuditView, adminPerms);
        Assert.Contains(PermissionCatalog.ReportView, adminPerms);
    }

    [Fact]
    public void PermissionCatalog_MemberRole_HasMemberDefaultPermissions_DisallowingStandalonePostAndPoll()
    {
        var memberPerms = PermissionCatalog.DefaultForRole("MEMBER");
        var expectedCount = PermissionCatalog.All.Count(p => p.MemberDefault);
        Assert.Equal(10, expectedCount);
        Assert.Equal(expectedCount, memberPerms.Count);
        Assert.DoesNotContain(PermissionCatalog.PostStandaloneCreate, memberPerms);
        Assert.DoesNotContain(PermissionCatalog.PollCreate, memberPerms);
        Assert.DoesNotContain(PermissionCatalog.ReportView, memberPerms);
        Assert.Contains(PermissionCatalog.PostCreate, memberPerms);
        Assert.Contains(PermissionCatalog.PollVote, memberPerms);
    }

    [Fact]
    public void PermissionCatalog_DomainProfessionalAndPublicFigure_HaveAllUserDomainPermissions()
    {
        var proPerms = PermissionCatalog.DefaultForRole("DOMAIN_PROFESSIONAL");
        var figurePerms = PermissionCatalog.DefaultForRole("PUBLIC_FIGURE");
        var userDomainCount = PermissionCatalog.All.Count(p => p.Type == 0);

        Assert.Equal(16, userDomainCount);
        Assert.Equal(16, proPerms.Count);
        Assert.Equal(16, figurePerms.Count);
        Assert.DoesNotContain(PermissionCatalog.ReportView, proPerms);
        Assert.DoesNotContain(PermissionCatalog.ReportView, figurePerms);
        Assert.Contains(PermissionCatalog.PostStandaloneCreate, proPerms);
        Assert.Contains(PermissionCatalog.PollCreate, proPerms);
        Assert.Contains(PermissionCatalog.GroupChatCreate, proPerms);
        Assert.Contains(PermissionCatalog.CreatorEarnings, proPerms);
        Assert.Contains(PermissionCatalog.CreatorPayouts, proPerms);
        Assert.Contains(PermissionCatalog.SubCommunityCreate, proPerms);
    }
}