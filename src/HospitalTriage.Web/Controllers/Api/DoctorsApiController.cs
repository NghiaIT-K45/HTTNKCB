using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/doctors")]
[Authorize(Roles = "Admin,Manager")]
public sealed class DoctorsApiController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorsApiController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] int? departmentId, CancellationToken ct)
    {
        var list = await _service.SearchAsync(search, departmentId, ct);

        var data = list.Select(x => new
        {
            x.Id,
            x.Code,
            x.FullName,
            x.DepartmentId,
            DepartmentName = x.Department?.Name,
            x.IsActive
        });

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();

        return Ok(new
        {
            entity.Id,
            entity.Code,
            entity.FullName,
            entity.DepartmentId,
            DepartmentName = entity.Department?.Name,
            entity.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DoctorCreateRequest request, CancellationToken ct)
    {
        var (ok, error, id) = await _service.CreateAsync(request, ct);
        if (!ok) return BadRequest(new { error });

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateRequest request, CancellationToken ct)
    {
        var (ok, error) = await _service.UpdateAsync(new DoctorUpdateRequest(id, request.Code, request.FullName, request.DepartmentId, request.IsActive), ct);
        if (!ok) return BadRequest(new { error });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _service.DeactivateAsync(id, ct);
        if (!ok) return BadRequest(new { error });

        return NoContent();
    }
}
