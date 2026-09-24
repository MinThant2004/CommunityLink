using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Group;
using CommunityLink.Domain.Security;

namespace CommunityLink.Domain.Features.Group;

[Route("api/groups")]
public sealed class GroupController(IGroupService groupService, ICurrentUserContext currentUser) : BaseController
{
    /// <summary>
    /// Returns whether the current user is a Premium creator eligible to create Group Chats.
    /// </summary>
    [HttpGet("can-create-groupchat")]
    [Authorize]
    public IActionResult CanCreateGroupChat()
    {
        var result = new GroupChatEligibilityModel(
            currentUser.IsPremiumCreator,
            currentUser.RoleCode);
        return Ok(Result<GroupChatEligibilityModel>.Success(result));
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroups([FromQuery] int? subCommunityId, [FromQuery] string? search, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.GetGroupsAsync(subCommunityId, search, cancellationToken));

    [HttpGet("{groupId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupById(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.GetGroupByIdAsync(groupId, cancellationToken));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.CreateGroupAsync(request, cancellationToken));

    [HttpPut("{groupId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupRequestModel request, CancellationToken cancellationToken)
    {
        if (groupId != request.GroupId)
        {
            return BadRequest(Result.Failure("Mismatched group id."));
        }
        return ToActionResult(await groupService.UpdateGroupAsync(request, cancellationToken));
    }

    [HttpPost("{groupId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinGroup(int groupId, [FromBody] string? requestNote, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.JoinGroupAsync(groupId, requestNote, cancellationToken));

    [HttpPost("{groupId:int}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGroup(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.LeaveGroupAsync(groupId, cancellationToken));

    [HttpGet("{groupId:int}/members")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupMembers(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.GetGroupMembersAsync(groupId, cancellationToken));

    [HttpGet("{groupId:int}/join-requests")]
    [Authorize]
    public async Task<IActionResult> GetGroupJoinRequests(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.GetGroupJoinRequestsAsync(groupId, cancellationToken));

    [HttpPost("join-requests/{requestId:int}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveJoinRequest(int requestId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.ReviewJoinRequestAsync(requestId, true, cancellationToken));

    [HttpPost("join-requests/{requestId:int}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectJoinRequest(int requestId, CancellationToken cancellationToken) =>
        ToActionResult(await groupService.ReviewJoinRequestAsync(requestId, false, cancellationToken));
}
