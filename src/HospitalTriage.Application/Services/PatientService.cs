using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Services;

public sealed class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo)
    {
        _repo = repo;
    }

    public Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<(bool ok, string? error, PatientUpsertResult? result)> UpsertAsync(PatientUpsertRequest request, CancellationToken ct = default)
    {
        var fullName = (request.FullName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Họ tên bệnh nhân là bắt buộc.", null);

        if (request.DateOfBirth == default)
            return (false, "Ngày sinh không hợp lệ.", null);

        // Ưu tiên match theo IdentityNumber nếu có
        Patient? existing = null;
        if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
        {
            existing = await _repo.FindByIdentityNumberAsync(request.IdentityNumber.Trim(), ct);
        }

        // Fallback match theo fullName + dob (+ phone)
        if (existing is null)
        {
            existing = await _repo.FindByBasicInfoAsync(fullName, request.DateOfBirth, request.Phone?.Trim(), ct);
        }

        if (existing is not null)
        {
            // Update thông tin nếu có thay đổi (nhẹ)
            existing.FullName = fullName;
            existing.Gender = request.Gender;
            existing.DateOfBirth = request.DateOfBirth;
            existing.IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? existing.IdentityNumber : request.IdentityNumber?.Trim();
            existing.Phone = request.Phone?.Trim();
            existing.Address = request.Address?.Trim();
            existing.InsuranceCode = request.InsuranceCode?.Trim();

            await _repo.UpdateAsync(existing, ct);
            await _repo.SaveChangesAsync(ct);

            return (true, null, new PatientUpsertResult(existing.Id, IsNew: false));
        }

        var entity = new Patient
        {
            FullName = fullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            IdentityNumber = request.IdentityNumber?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            InsuranceCode = request.InsuranceCode?.Trim()
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null, new PatientUpsertResult(entity.Id, IsNew: true));
    }
}
