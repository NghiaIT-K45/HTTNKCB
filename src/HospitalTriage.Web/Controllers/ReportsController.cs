using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using HospitalTriage.Web.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Manager", "Admin")]
public class ReportsController : Controller
{
    private readonly ReportApiService _api;

    public ReportsController(ReportApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var (ok, error, vm) = await _api.BuildIndexVmAsync(filter: null, runReport: false, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tải trang báo cáo.";
            return View(new ReportIndexVm());
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReportFilterVm filter, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var (_, _, vmInvalid) = await _api.BuildIndexVmAsync(filter, runReport: false, ct);
            return View(vmInvalid ?? new ReportIndexVm { Filter = filter });
        }

        var (ok, error, vm) = await _api.BuildIndexVmAsync(filter, runReport: true, ct);
        if (!ok || vm is null)
        {
            TempData["Error"] = error ?? "Không thể tạo báo cáo.";
            var (_, _, vmFallback) = await _api.BuildIndexVmAsync(filter, runReport: false, ct);
            return View(vmFallback ?? new ReportIndexVm { Filter = filter });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(DateOnly fromDate, DateOnly toDate, int? departmentId, CancellationToken ct)
    {
        var filter = new ReportFilterVm
        {
            FromDate = fromDate,
            ToDate = toDate,
            DepartmentId = departmentId
        };

        var (ok, error, data) = await _api.ExportVisitsPerDayCsvAsync(filter, ct);
        if (!ok || data is null)
        {
            TempData["Error"] = error ?? "Không thể export CSV.";
            return RedirectToAction(nameof(Index));
        }

        return File(data.Value.content, "text/csv", data.Value.fileName);
    }
}
