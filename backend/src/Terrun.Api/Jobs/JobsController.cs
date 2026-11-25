using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Terrun.Domain.Errors;

namespace Terrun.Api.Jobs;

// LLD §6 (D-25): the hourly GitHub Actions workflow calls this endpoint. It is not part of the app contract, so it stays
// out of the OpenAPI document (and of the app types)
[ApiController]
[Route("internal/jobs")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class JobsController(JobScheduler scheduler, IOptions<JobsOptions> options) : ControllerBase
{
    public const string KeyHeader = "X-Jobs-Key";

    [HttpPost("run")]
    public async Task<JobsRunResponse> RunAsync([FromHeader(Name = KeyHeader)] string? key, CancellationToken cancellationToken)
    {
        if (!IsValidKey(key))
        {
            throw new UnauthorizedException(ErrorCodes.InvalidToken, "Missing or wrong jobs key.");
        }

        return new JobsRunResponse(await scheduler.RunDueAsync(cancellationToken));
    }

    // Constant-time comparison, so the response time does not reveal how much of the key was right
    private bool IsValidKey(string? key)
    {
        var expected = options.Value;
        return expected.HasKey && key is not null
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(expected.Key!));
    }
}

public sealed record JobsRunResponse(IReadOnlyList<JobResult> Jobs);
