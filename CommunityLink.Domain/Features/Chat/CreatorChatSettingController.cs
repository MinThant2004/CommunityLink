using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;

namespace CommunityLink.Domain.Features.Chat;

[Route("api/creator/chat/settings")]
[Authorize]
public sealed class CreatorChatSettingController(ICreatorChatSettingService settingService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken) =>
        ToActionResult(await settingService.GetSettingsAsync(null, cancellationToken));

    [HttpGet("{creatorUserId:int}")]
    public async Task<IActionResult> GetCreatorSettings(int creatorUserId, CancellationToken cancellationToken) =>
        ToActionResult(await settingService.GetSettingsAsync(creatorUserId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> SaveSettings([FromBody] SaveCreatorChatSettingRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await settingService.SaveSettingsAsync(request, cancellationToken));
}
