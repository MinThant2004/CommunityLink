namespace CommunityLink.Shared.Features.RoleAndPermission;

public sealed record RoleModel(int RoleId, string Code, string Name, string? Description, bool IsSystemRole, int UserCount);
public sealed record PermissionModel(int PermissionId, string Code, string Name, string Module, string? Description);
public sealed record RolePermissionItemModel(string Code, string Name, string Module, bool IsGranted);
public sealed record RolePermissionMatrixResponseModel(int RoleId, string RoleCode, string RoleName, IReadOnlyList<RolePermissionItemModel> Permissions);
public sealed record UpdateRolePermissionsRequestModel(int RoleId, IReadOnlyList<string> GrantedPermissionCodes);