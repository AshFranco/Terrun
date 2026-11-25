using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Api.Modules.Identity.Services;

namespace Terrun.Api.Modules.Identity.Controllers;

// LLD §3.2 (M1): public access to the terms and privacy policy currently in effect (P-01, P-02, P-32)
[ApiController]
[Route("legal")]
public sealed class LegalController(LegalConsentService legalConsents) : ControllerBase
{
    [HttpGet("current")]
    [AllowAnonymous]
    public Task<IReadOnlyList<LegalDocumentResponse>> GetCurrentAsync(CancellationToken cancellationToken) =>
        legalConsents.GetCurrentAsync(cancellationToken);
}
