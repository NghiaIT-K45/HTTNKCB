using System.Security.Claims;
using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using HospitalTriage.Web.ViewModels.Triage;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Doctor", "Nurse", "Manager", "Admin")]
public class TriageController : Controller
{
    private readonly TriageApiService _api;

    public TriageController(TriageApiService api)
    {
        _api = api;
    }

    // ✅ /Triage -> /Triage/Index
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date, CancellationToken ct)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);

        var (ok, error, vm) = await _api.GetWaitingListAsync(d, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải danh sách chờ phân luồng.";
            return View(new TriageWaitingListVm { Date = d });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Process(int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            TempData["Error"] = "Vui lòng chọn lượt khám để phân luồng.";
            return RedirectToAction(nameof(Index));
        }

        var (ok, error, vm) = await _api.GetTriageVmAsync(id, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không tìm thấy lượt khám.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(TriageVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var (_, _, reloadVm) = await _api.GetTriageVmAsync(vm.VisitId, ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            vm.Doctors = reloadVm?.Doctors ?? vm.Doctors;
            vm.PatientName = reloadVm?.PatientName ?? vm.PatientName;
            vm.QueueNumber = reloadVm?.QueueNumber ?? vm.QueueNumber;
            vm.VisitDate = reloadVm?.VisitDate ?? vm.VisitDate;
            return View(vm);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await _api.SubmitAsync(vm, userId, ct);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Phân luồng thất bại.");
            var (_, _, reloadVm) = await _api.GetTriageVmAsync(vm.VisitId, ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            vm.Doctors = reloadVm?.Doctors ?? vm.Doctors;
            vm.PatientName = reloadVm?.PatientName ?? vm.PatientName;
            vm.QueueNumber = reloadVm?.QueueNumber ?? vm.QueueNumber;
            vm.VisitDate = reloadVm?.VisitDate ?? vm.VisitDate;
            return View(vm);
        }

        TempData["Success"] = "Phân luồng thành công.";
        return RedirectToAction("Index", "Dashboard");
    }
}
