using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using HospitalTriage.Web.ViewModels.Departments;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Manager", "Admin")]
public class DepartmentsController : Controller
{
    private readonly DepartmentApiService _api;

    public DepartmentsController(DepartmentApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        var (ok, error, vm) = await _api.SearchAsync(search, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải danh sách khoa.";
            vm = new DepartmentIndexVm { Search = search };
        }

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DepartmentCreateVm { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentCreateVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (ok, error, _) = await _api.CreateAsync(vm, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Tạo khoa thất bại.");
            return View(vm);
        }

        TempData["Success"] = "Tạo khoa thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var (ok, error, vm) = await _api.GetEditVmAsync(id, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không tìm thấy khoa.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DepartmentEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (ok, error) = await _api.UpdateAsync(vm, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Cập nhật khoa thất bại.");
            return View(vm);
        }

        TempData["Success"] = "Cập nhật khoa thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _api.DeleteAsync(id, ct);
        if (!ok)
        {
            TempData["Error"] = error ?? "Xóa khoa thất bại.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Đã vô hiệu hóa khoa.";
        return RedirectToAction(nameof(Index));
    }
}
