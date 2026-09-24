using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Security;

public interface ICurrentUserContext
{
    int? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? RoleCode { get; }
    int? RoleId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsPremiumCreator { get; }
}

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public int? UserId => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("sub")?.Value, out var id) ? id : null;
    public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value ?? User?.FindFirst("name")?.Value;
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;
    public string? RoleCode => User?.FindFirst(ClaimTypes.Role)?.Value ?? User?.FindFirst("role")?.Value;
    public int? RoleId => int.TryParse(User?.FindFirst("role_id")?.Value, out var roleId) ? roleId : null;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => string.Equals(RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase) || (User?.HasClaim("permission", PermissionCatalog.AdminUserView) ?? false);
    public bool IsPremiumCreator => string.Equals(RoleCode, "DOMAIN_PROFESSIONAL", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(RoleCode, "PUBLIC_FIGURE", StringComparison.OrdinalIgnoreCase);
}