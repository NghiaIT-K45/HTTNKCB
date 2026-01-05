using System.Security.Claims;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Doctor", "Nurse", "Manager", "Admin")]
public class DashboardController : Controller
{
    private readonly DashboardApiService _api;
    private readonly IVisitService _visitService;

    public DashboardController(DashboardApiService api, IVisitService visitService)
    {
        _api = api;
        _visitService = visitService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? departmentId, CancellationToken ct)
    {
        var (ok, error, vm) = await _api.GetDashboardAsync(User, departmentId, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải dashboard.";
            return View(new HospitalTriage.Web.ViewModels.Dashboard.DashboardVm());
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeVisitStatus(int visitId, VisitStatus newStatus, int? departmentId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (ok, error) = await _visitService.ChangeStatusAsync(
            new VisitStatusChangeRequest(visitId, newStatus, userId),
            ct
        );

        if (!ok) TempData["Error"] = error ?? "Thao tác thất bại.";
        else TempData["Success"] = "Cập nhật trạng thái thành công.";

        return RedirectToAction(nameof(Index), new { departmentId });
    }
}
