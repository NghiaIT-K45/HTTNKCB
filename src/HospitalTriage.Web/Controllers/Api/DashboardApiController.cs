using System.Security.Claims;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Manager,Doctor")]
public sealed class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IDoctorService _doctorService;

    public DashboardApiController(IDashboardService dashboardService, IDoctorService doctorService)
    {
        _dashboardService = dashboardService;
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? departmentId, CancellationToken ct)
    {
        int? effectiveDeptId = departmentId;

        var isDoctor = User.IsInRole("Doctor");
        if (isDoctor)
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { error = "Không xác định được tài khoản bác sĩ." });

            var doctor = await _doctorService.GetByCodeAsync(code, ct);
            if (doctor is null)
                return BadRequest(new { error = $"Không tìm thấy bác sĩ theo code '{code}'." });

            effectiveDeptId = doctor.DepartmentId;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (ok, error, result) = await _dashboardService.GetDashboardAsync(
            new DashboardRequest(effectiveDeptId, userId, isDoctor),
            ct);

        if (!ok || result is null) return BadRequest(new { error });

        return Ok(result);
    }
}
