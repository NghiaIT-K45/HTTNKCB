using System.Security.Claims;
using HospitalTriage.Web.Filters;
using HospitalTriage.Web.Services.ApiServices;
using HospitalTriage.Web.ViewModels.Intake;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

[RequireLogin]
[RequireRole("Receptionist", "Manager", "Admin")]
public class IntakeController : Controller
{
    private readonly PatientApiService _patientApi;
    private readonly VisitApiService _visitApi;

    public IntakeController(PatientApiService patientApi, VisitApiService visitApi)
    {
        _patientApi = patientApi;
        _visitApi = visitApi;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new PatientIntakeVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(PatientIntakeVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (okPatient, errPatient, patientResult) = await _patientApi.UpsertAsync(vm, ct);
        if (!okPatient || patientResult is null)
        {
            ModelState.AddModelError(string.Empty, errPatient ?? "Không thể tạo/ cập nhật bệnh nhân.");
            return View(vm);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var (okVisit, errVisit, visitResult) = await _visitApi.CreateVisitAsync(patientResult.PatientId, today, userId, ct);
        if (!okVisit || visitResult is null)
        {
            ModelState.AddModelError(string.Empty, errVisit ?? "Không thể tạo lượt khám.");
            return View(vm);
        }

        var resultVm = new IntakeResultVm
        {
            PatientId = patientResult.PatientId,
            IsNewPatient = patientResult.IsNew,
            VisitId = visitResult.VisitId,
            QueueNumber = visitResult.QueueNumber,
            VisitDate = today
        };

        TempData["Success"] = "Tiếp nhận thành công.";
        return View("Result", resultVm);
    }
}
