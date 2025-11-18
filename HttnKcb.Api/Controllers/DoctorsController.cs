using HttnKcb.Api.Data;
using HttnKcb.Api.DTOs;
using HttnKcb.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HttnKcb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly HospitalDbContext _db;
    public DoctorsController(HospitalDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorReadDto>>> GetAll()
    {
        var items = await _db.Doctors.Include(d => d.Department)
            .OrderBy(d => d.FullName)
            .Select(d => new DoctorReadDto(d.Id, d.FullName, d.LicenseNumber, d.Phone, d.Email, d.DepartmentId, d.Department!.Name))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorReadDto>> GetById(int id)
    {
        var d = await _db.Doctors.Include(x => x.Department).FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NotFound();
        return new DoctorReadDto(d.Id, d.FullName, d.LicenseNumber, d.Phone, d.Email, d.DepartmentId, d.Department!.Name);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorReadDto>> Create([FromBody] DoctorCreateDto dto)
    {
        var dept = await _db.Departments.FindAsync(dto.DepartmentId);
        if (dept is null) return BadRequest($"Không tìm thấy khoa với Id = {dto.DepartmentId}");

        var entity = new Doctor
        {
            FullName = dto.FullName,
            LicenseNumber = dto.LicenseNumber,
            Phone = dto.Phone,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId
        };
        _db.Doctors.Add(entity);
        await _db.SaveChangesAsync();

        var result = new DoctorReadDto(entity.Id, entity.FullName, entity.LicenseNumber, entity.Phone, entity.Email, entity.DepartmentId, dept.Name);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateDto dto)
    {
        var entity = await _db.Doctors.FindAsync(id);
        if (entity is null) return NotFound();
        entity.FullName = dto.FullName;
        entity.LicenseNumber = dto.LicenseNumber;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.DepartmentId = dto.DepartmentId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Doctors.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Doctors.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
