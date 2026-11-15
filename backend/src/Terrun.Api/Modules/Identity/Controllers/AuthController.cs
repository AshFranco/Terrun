using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Api.Modules.Identity.Services;
using Terrun.Api.Web;

namespace Terrun.Api.Modules.Identity.Controllers;

// LLD §3.2 (M1). 10 requests per minute per IP on every /auth endpoint (§3.1)
[ApiController]
[Route("auth")]
[EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
public sealed class AuthController(AuthService auth, EmailVerificationService emailVerification) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await auth.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        auth.LoginAsync(request, HttpContext.Connection.RemoteIpAddress, cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken) =>
        auth.RefreshAsync(request, HttpContext.Connection.RemoteIpAddress, cancellationToken);

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        await auth.LogoutAsync(userId, request.RefreshToken, HttpContext.Connection.RemoteIpAddress, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await emailVerification.VerifyAsync(request.Email, request.Code, HttpContext.Connection.RemoteIpAddress, cancellationToken);
        return NoContent();
    }

    // Always 202, whether or not the email has an account (no account enumeration)
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        await emailVerification.ResendAsync(request.Email, cancellationToken);
        return Accepted();
    }
}
