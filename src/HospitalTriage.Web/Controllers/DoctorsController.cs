using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using HospitalTriage.Web.ViewModels.Doctors;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Manager", "Admin")]
public class DoctorsController : Controller
{
    private readonly DoctorApiService _api;

    public DoctorsController(DoctorApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? departmentId, CancellationToken ct)
    {
        var (ok, error, vm) = await _api.SearchAsync(search, departmentId, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải danh sách bác sĩ.";
            vm = new DoctorIndexVm { Search = search, DepartmentId = departmentId };
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var (ok, error, vm) = await _api.GetCreateVmAsync(ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorCreateVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            // reload dropdown
            var (_, _, reloadVm) = await _api.GetCreateVmAsync(ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            return View(vm);
        }

        var (ok, error, _) = await _api.CreateAsync(vm, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Tạo bác sĩ thất bại.");
            var (_, _, reloadVm) = await _api.GetCreateVmAsync(ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            return View(vm);
        }

        TempData["Success"] = "Tạo bác sĩ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var (ok, error, vm) = await _api.GetEditVmAsync(id, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không tìm thấy bác sĩ.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DoctorEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var (_, _, reloadVm) = await _api.GetEditVmAsync(vm.Id, ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            return View(vm);
        }

        var (ok, error) = await _api.UpdateAsync(vm, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Cập nhật bác sĩ thất bại.");
            var (_, _, reloadVm) = await _api.GetEditVmAsync(vm.Id, ct);
            vm.Departments = reloadVm?.Departments ?? vm.Departments;
            return View(vm);
        }

        TempData["Success"] = "Cập nhật bác sĩ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _api.DeleteAsync(id, ct);
        if (!ok)
        {
            TempData["Error"] = error ?? "Xóa bác sĩ thất bại.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Đã vô hiệu hóa bác sĩ.";
        return RedirectToAction(nameof(Index));
    }
}
