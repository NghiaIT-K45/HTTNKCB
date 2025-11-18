using HttnKcb.Api.Data;
using HttnKcb.Api.DTOs;
using HttnKcb.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HttnKcb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly HospitalDbContext _db;
    public UsersController(HospitalDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
    {
        var items = await _db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserReadDto(u.Id, u.Username, u.FullName, u.Email,
                (DTOs.UserRoleDto)u.Role, u.IsActive))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserReadDto>> GetById(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        return new UserReadDto(u.Id, u.Username, u.FullName, u.Email, (DTOs.UserRoleDto)u.Role, u.IsActive);
    }

    [HttpPost]
    public async Task<ActionResult<UserReadDto>> Create([FromBody] UserCreateDto dto)
    {
        var exists = await _db.Users.AnyAsync(x => x.Username == dto.Username);
        if (exists) return Conflict($"Username '{dto.Username}' đã tồn tại");

        var entity = new AppUser
        {
            Username = dto.Username,
            FullName = dto.FullName,
            Email = dto.Email,
            Role = (Models.UserRole)dto.Role,
            IsActive = dto.IsActive
        };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        var result = new UserReadDto(entity.Id, entity.Username, entity.FullName, entity.Email, (DTOs.UserRoleDto)entity.Role, entity.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserCreateDto dto)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity is null) return NotFound();
        entity.FullName = dto.FullName;
        entity.Email = dto.Email;
        entity.Role = (Models.UserRole)dto.Role;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
