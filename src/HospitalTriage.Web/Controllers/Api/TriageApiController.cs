using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/triage")]
[Authorize(Roles = "Admin,Nurse")]
public sealed class TriageApiController : ControllerBase
{
    private readonly ITriageService _service;

    public TriageApiController(ITriageService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Triage([FromBody] TriageRequest request, CancellationToken ct)
    {
        // override ChangedByUserId from current user
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var req = request with { ChangedByUserId = userId };

        var (ok, error, result) = await _service.TriageAsync(req, ct);
        if (!ok || result is null) return BadRequest(new { error });

        return Ok(result);
    }
}
