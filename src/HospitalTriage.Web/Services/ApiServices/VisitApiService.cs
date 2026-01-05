using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class VisitApiService
{
    private readonly IVisitService _service;

    public VisitApiService(IVisitService service)
    {
        _service = service;
    }

    public Task<(bool ok, string? error, VisitCreateResult? data)> CreateVisitAsync(int patientId, DateOnly visitDate, string? changedByUserId, CancellationToken ct = default)
        => _service.CreateVisitAsync(new VisitCreateRequest(patientId, visitDate), changedByUserId, ct);

    public Task<HospitalTriage.Domain.Entities.Visit?> GetByIdAsync(int visitId, bool includeDetails = false, CancellationToken ct = default)
        => _service.GetByIdAsync(visitId, includeDetails, ct);
}
