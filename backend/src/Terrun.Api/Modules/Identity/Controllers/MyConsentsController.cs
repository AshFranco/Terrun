using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Api.Modules.Identity.Services;

namespace Terrun.Api.Modules.Identity.Controllers;

// LLD §3.2 (M1): records acceptance of new legal document versions for the authenticated runner (P-32)
[ApiController]
[Route("me")]
public sealed class MyConsentsController(LegalConsentService legalConsents) : ControllerBase
{
    [HttpPost("consents")]
    public Task<IReadOnlyList<ConsentResponse>> AcceptAsync(AcceptConsentsRequest request, CancellationToken cancellationToken) =>
        legalConsents.AcceptAsync(CurrentUserId, request, HttpContext.Connection.RemoteIpAddress, cancellationToken);

    private Guid CurrentUserId => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
