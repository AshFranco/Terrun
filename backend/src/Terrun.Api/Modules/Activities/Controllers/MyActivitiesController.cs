using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Activities.Services;

namespace Terrun.Api.Modules.Activities.Controllers;

// LLD §3.2 (M3): the caller's own history and period summary
[ApiController]
[Route("me")]
public sealed class MyActivitiesController(ActivityQueryService queries) : ControllerBase
{
    [HttpGet("activities")]
    public Task<ActivityPage> ListAsync([FromQuery] ActivityListQuery query, CancellationToken cancellationToken) =>
        queries.ListMineAsync(CurrentUserId, query, cancellationToken);

    [HttpGet("summary")]
    public Task<SummaryResponse> SummaryAsync([FromQuery, BindRequired] SummaryPeriod period, CancellationToken cancellationToken) =>
        queries.SummaryAsync(CurrentUserId, period, cancellationToken);

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
