using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Terrun.Api.Modules.Platform.Contracts;
using Terrun.Api.Modules.Platform.Services;

namespace Terrun.Api.Modules.Platform.Controllers;

// LLD §3.2 (M13): public parameters consumed by the mobile app (auto-pause, conquest threshold, route trim, level curve)
[ApiController]
[Route("settings")]
public sealed class SettingsController(SystemSettingsReader settings) : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public Task<PublicSettingsResponse> GetPublicAsync(CancellationToken cancellationToken) =>
        settings.GetPublicAsync(cancellationToken);
}
