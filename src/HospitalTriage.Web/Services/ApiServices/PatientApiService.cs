using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Web.ViewModels.Intake;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class PatientApiService
{
    private readonly IPatientService _service;

    public PatientApiService(IPatientService service)
    {
        _service = service;
    }

    public Task<(bool ok, string? error, PatientUpsertResult? data)> UpsertAsync(PatientIntakeVm vm, CancellationToken ct = default)
        => _service.UpsertAsync(new PatientUpsertRequest(
            vm.FullName,
            vm.DateOfBirth,
            vm.Gender,
            vm.IdentityNumber,
            vm.Phone,
            vm.Address,
            vm.InsuranceCode
        ), ct);
}
