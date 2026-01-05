using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ReportsApiController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsApiController(IReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] int? departmentId, CancellationToken ct)
    {
        var (ok, error, result) = await _service.GetReportAsync(new ReportRequest(fromDate, toDate, departmentId), ct);
        if (!ok || result is null) return BadRequest(new { error });

        return Ok(result);
    }

    [HttpGet("csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] int? departmentId, CancellationToken ct)
    {
        var (ok, error, result) = await _service.GetReportAsync(new ReportRequest(fromDate, toDate, departmentId), ct);
        if (!ok || result is null) return BadRequest(new { error });

        var csv = _service.ExportVisitsPerDayCsv(result);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        var fileName = $"visits-per-day_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

        return File(bytes, "text/csv", fileName);
    }
}
