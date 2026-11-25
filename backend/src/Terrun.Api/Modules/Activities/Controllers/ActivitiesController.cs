using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Activities.Services;
using Terrun.Api.Web;
using Terrun.Domain.Errors;

namespace Terrun.Api.Modules.Activities.Controllers;

// LLD §3.2 (M3)
[ApiController]
[Route("activities")]
public sealed class ActivitiesController(ActivityUploadService uploads) : ControllerBase
{
    // LLD §7.3: this endpoint accepts up to 8 MB (2 MB everywhere else), enough for activity.max_points points
    private const long _maxUploadBytes = 8 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(_maxUploadBytes)]
    [ProducesResponseType<ActivityUploadResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ActivityUploadResponse>(StatusCodes.Status200OK)]
    [ApiError(StatusCodes.Status409Conflict, ErrorCodes.ActivityOverlap)]
    public async Task<ActionResult<ActivityUploadResponse>> UploadAsync(CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var (activity, isNew) = await uploads.UploadAsync(userId, request, cancellationToken);
        return isNew ? Accepted(activity) : Ok(activity);
    }
}
