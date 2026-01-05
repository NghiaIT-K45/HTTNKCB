using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/departments")]
[Authorize(Roles = "Admin,Manager")]
public sealed class DepartmentsApiController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsApiController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? search, CancellationToken ct)
    {
        var list = await _service.SearchAsync(search, ct);

        var data = list.Select(x => new
        {
            x.Id,
            x.Code,
            x.Name,
            x.IsActive,
            x.IsGeneral
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
            entity.Name,
            entity.IsActive,
            entity.IsGeneral
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DepartmentCreateRequest request, CancellationToken ct)
    {
        var (ok, error, id) = await _service.CreateAsync(request, ct);
        if (!ok) return BadRequest(new { error });

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateRequest request, CancellationToken ct)
    {
        var (ok, error) = await _service.UpdateAsync(new DepartmentUpdateRequest(id, request.Code, request.Name, request.IsActive, request.IsGeneral), ct);
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
