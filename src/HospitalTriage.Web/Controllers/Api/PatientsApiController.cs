using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/patients")]
[Authorize(Roles = "Admin,Receptionist,Nurse,Manager,Doctor")]
public sealed class PatientsApiController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsApiController(IPatientService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var p = await _service.GetByIdAsync(id, ct);
        if (p is null) return NotFound();

        return Ok(new
        {
            p.Id,
            p.FullName,
            p.DateOfBirth,
            p.Gender,
            p.IdentityNumber,
            p.Phone,
            p.Address,
            p.InsuranceCode
        });
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] PatientUpsertRequest request, CancellationToken ct)
    {
        var (ok, error, result) = await _service.UpsertAsync(request, ct);
        if (!ok || result is null) return BadRequest(new { error });

        return Ok(result);
    }
}
