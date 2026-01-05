using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/visits")]
[Authorize(Roles = "Admin,Receptionist,Nurse,Doctor,Manager")]
public sealed class VisitsApiController : ControllerBase
{
    private readonly IVisitService _service;

    public VisitsApiController(IVisitService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var v = await _service.GetByIdAsync(id, includeDetails: true, ct);
        if (v is null) return NotFound();

        return Ok(new
        {
            v.Id,
            v.PatientId,
            PatientName = v.Patient?.FullName,
            v.VisitDate,
            v.QueueDate,
            v.QueueNumber,
            v.DepartmentId,
            DepartmentName = v.Department?.Name,
            v.DoctorId,
            DoctorName = v.Doctor?.FullName,
            v.Symptoms,
            v.CurrentStatus
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetByStatus([FromQuery] VisitStatus status, [FromQuery] int? departmentId, CancellationToken ct)
    {
        var list = await _service.GetWaitingListAsync(status, departmentId, ct);

        return Ok(list.Select(v => new
        {
            v.Id,
            v.PatientId,
            PatientName = v.Patient?.FullName,
            v.VisitDate,
            v.QueueNumber,
            v.DepartmentId,
            DepartmentName = v.Department?.Name,
            v.DoctorId,
            DoctorName = v.Doctor?.FullName,
            v.CurrentStatus
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create([FromBody] VisitCreateRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error, result) = await _service.CreateVisitAsync(request, userId, ct);
        if (!ok || result is null) return BadRequest(new { error });

        return Ok(result);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = "Admin,Nurse,Doctor")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] VisitStatus newStatus, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await _service.ChangeStatusAsync(new VisitStatusChangeRequest(id, newStatus, userId), ct);
        if (!ok) return BadRequest(new { error });

        return NoContent();
    }
}
