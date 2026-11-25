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
public sealed class ActivitiesController(ActivityUploadService uploads, ActivityQueryService queries, ActivityEditService edits) : ControllerBase
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
        var (activity, isNew) = await uploads.UploadAsync(CurrentUserId, request, cancellationToken);
        return isNew ? Accepted(activity) : Ok(activity);
    }

    [HttpGet("{id:guid}")]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public Task<ActivityResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        queries.GetAsync(CurrentUserId, id, cancellationToken);

    [HttpGet("{id:guid}/route")]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public Task<RouteResponse> GetRouteAsync(Guid id, CancellationToken cancellationToken) =>
        queries.GetRouteAsync(CurrentUserId, id, cancellationToken);

    [HttpGet("{id:guid}/splits")]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public Task<IReadOnlyList<SplitResponse>> GetSplitsAsync(Guid id, CancellationToken cancellationToken) =>
        queries.GetSplitsAsync(CurrentUserId, id, cancellationToken);

    [HttpPatch("{id:guid}")]
    [ApiError(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden)]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public Task<ActivityResponse> UpdateAsync(Guid id, UpdateActivityRequest request, CancellationToken cancellationToken) =>
        edits.UpdateAsync(CurrentUserId, id, request, cancellationToken);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiError(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden)]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await edits.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
