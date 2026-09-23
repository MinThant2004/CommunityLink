using CommunityLink.Shared;
using CommunityLink.Shared.Features.RoleAndPermission;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class RoleAndPermissionApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<RoleModel>>("api/roles", cancellationToken);

    public Task<Result<RolePermissionMatrixResponseModel>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default) =>
        GetAsync<RolePermissionMatrixResponseModel>($"api/roles/{roleId}/permissions", cancellationToken);

    public Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync("api/roles/permissions", request, cancellationToken);

    public Task<Result<IReadOnlyList<string>>> GetMyPermissionsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<string>>("api/roles/my-permissions", cancellationToken);
}