using HttnKcb.Api.Data;
using HttnKcb.Api.DTOs;
using HttnKcb.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HttnKcb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly HospitalDbContext _db;
    public DepartmentsController(HospitalDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentReadDto>>> GetAll()
    {
        var items = await _db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentReadDto(d.Id, d.Name, d.Description, d.IsActive))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentReadDto>> GetById(int id)
    {
        var d = await _db.Departments.FindAsync(id);
        if (d is null) return NotFound();
        return new DepartmentReadDto(d.Id, d.Name, d.Description, d.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentReadDto>> Create([FromBody] DepartmentCreateDto dto)
    {
        var entity = new Department { Name = dto.Name, Description = dto.Description, IsActive = dto.IsActive };
        _db.Departments.Add(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict("Tên khoa đã tồn tại.");
            throw;
        }
        var result = new DepartmentReadDto(entity.Id, entity.Name, entity.Description, entity.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateDto dto)
    {
        var entity = await _db.Departments.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict("Tên khoa đã tồn tại.");
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Departments.FindAsync(id);
        if (entity is null) return NotFound();

        var hasDoctors = await _db.Doctors.AnyAsync(d => d.DepartmentId == id);
        if (hasDoctors) return Conflict("Không thể xóa khoa vì đang có bác sĩ liên kết.");

        _db.Departments.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
