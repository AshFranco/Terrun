using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Terrun.Domain.Errors;

namespace Terrun.Api.Tests.Platform;

// Test-only endpoints that exercise the API conventions before real modules exist; never part of the app
[ApiController]
[Route("probe")]
public sealed class ProbeController : ControllerBase
{
    public const string AuthPolicy = "auth";

    [HttpGet("enum")]
    public ActionResult<ProbeResponse> GetEnum() => new ProbeResponse(ProbeStatus.OwnedByMe);

    [HttpPost("validated")]
    public IActionResult PostValidated(ProbeRequest request) => NoContent();

    [HttpGet("not-found")]
    public IActionResult GetNotFound() => throw new NotFoundException("Probe not found");

    [HttpGet("conflict")]
    public IActionResult GetConflict() => throw new ConflictException(ErrorCodes.EmailAlreadyRegistered, "Email already registered");

    [HttpGet("crash")]
    public IActionResult GetCrash() => throw new InvalidOperationException("secret internal detail");

    [HttpPost("auth")]
    [EnableRateLimiting(AuthPolicy)]
    public IActionResult PostAuth() => NoContent();
}

public enum ProbeStatus
{
    OwnedByMe,
}

public sealed record ProbeResponse(ProbeStatus Status);

public sealed record ProbeRequest(string? Email, int DistanceM);

public sealed class ProbeRequestValidator : AbstractValidator<ProbeRequest>
{
    public ProbeRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.DistanceM).GreaterThan(0);
    }
}
