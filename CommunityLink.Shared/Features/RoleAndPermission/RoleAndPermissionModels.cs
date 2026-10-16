namespace CommunityLink.Shared.Features.RoleAndPermission;

public sealed record RoleModel(int RoleId, string Code, string Name, string? Description, bool IsSystemRole, int UserCount, int RoleType = 0);
public sealed record PermissionModel(int PermissionId, string Code, string Name, string Module, string? Description, int Type = 0);
public sealed record RolePermissionItemModel(string Code, string Name, string Module, bool IsGranted, int Type = 0);
public sealed record RolePermissionMatrixResponseModel(int RoleId, string RoleCode, string RoleName, int RoleType, IReadOnlyList<RolePermissionItemModel> Permissions);
public sealed record UpdateRolePermissionsRequestModel(int RoleId, IReadOnlyList<string> GrantedPermissionCodes);
public sealed record CreateRoleRequestModel(string RoleCode, string RoleName, string? Description, int RoleType = 0);