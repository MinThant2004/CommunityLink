using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.Community;
using CommunityLink.Domain.Features.Post;
using CommunityLink.Domain.Features.Poll;
using CommunityLink.Domain.Features.Chat;
using CommunityLink.Domain.Features.Administration;
using CommunityLink.Domain.Features.Dashboard;
using CommunityLink.Domain.Features.UserProfile;
using CommunityLink.Domain.Services;
using CommunityLink.Shared;

using CommunityLink.Domain.Features.Group;
using CommunityLink.Domain.Features.ChatGroup;
using CommunityLink.Domain.Features.LinkDrop;

namespace CommunityLink.Domain;

public static class FeatureManager
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        var customSetting = new CustomSettingModel();
        configuration.Bind(customSetting);
        services.AddSingleton(customSetting);

        services.AddScoped(_ =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            if (!string.IsNullOrWhiteSpace(customSetting.ConnectionStrings.DefaultConnection))
                options.UseSqlServer(customSetting.ConnectionStrings.DefaultConnection);
            else
                options.UseInMemoryDatabase("CommunityLinkInMemoryDb");

            return options.Options;
        });
        services.AddScoped<AppDbContext>();

        // Security & Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Infrastructure Services
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Domain Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRoleAndPermissionService, RoleAndPermissionService>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<CommunityLink.Domain.Features.Notification.INotificationService, CommunityLink.Domain.Features.Notification.NotificationService>();
        services.AddScoped<ILinkDropPaymentService, LinkDropPaymentService>();
        services.AddScoped<CommunityLink.Domain.Features.Admin.IPlatformSettingService, CommunityLink.Domain.Features.Admin.PlatformSettingService>();
        services.AddScoped<IChatGroupService, ChatGroupService>();
        services.AddScoped<CommunityLink.Domain.Features.Creator.ICreatorEarningsService, CommunityLink.Domain.Features.Creator.CreatorEarningsService>();

        return services;
    }
}