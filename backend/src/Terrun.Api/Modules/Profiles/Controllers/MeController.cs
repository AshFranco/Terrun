using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Profiles.Contracts;
using Terrun.Api.Modules.Profiles.Services;
using Terrun.Api.Web;
using Terrun.Domain.Errors;

namespace Terrun.Api.Modules.Profiles.Controllers;

// LLD §3.2 (M2). The user always comes from the token: there is no id in the route to tamper with (§7.2)
[ApiController]
[Route("me")]
public sealed class MeController(ProfileService profiles) : ControllerBase
{
    [HttpGet]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    public Task<MeResponse> GetAsync(CancellationToken cancellationToken) =>
        profiles.GetMeAsync(CurrentUserId, cancellationToken);

    [HttpPatch("profile")]
    [ApiError(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    [ApiError(StatusCodes.Status409Conflict, ErrorCodes.HandleTaken)]
    public Task<MeResponse> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken) =>
        profiles.UpdateProfileAsync(CurrentUserId, request, HttpContext.Connection.RemoteIpAddress, cancellationToken);

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
