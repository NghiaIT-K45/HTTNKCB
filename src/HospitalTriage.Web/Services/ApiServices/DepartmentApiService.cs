using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Web.ViewModels.Departments;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class DepartmentApiService
{
    private readonly IDepartmentService _service;

    public DepartmentApiService(IDepartmentService service)
    {
        _service = service;
    }

    public async Task<(bool ok, string? error, DepartmentIndexVm? data)> SearchAsync(string? keyword, CancellationToken ct = default)
    {
        try
        {
            var items = await _service.SearchAsync(keyword, ct);

            var vm = new DepartmentIndexVm
            {
                Search = keyword,
                Items = items.Select(x => new DepartmentListItemVm
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    IsGeneral = x.IsGeneral
                }).ToList()
            };

            return (true, null, vm);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool ok, string? error, DepartmentEditVm? data)> GetEditVmAsync(int id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
            return (false, "Không tìm thấy khoa.", null);

        var vm = new DepartmentEditVm
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            IsGeneral = entity.IsGeneral
        };

        return (true, null, vm);
    }

    public Task<(bool ok, string? error, int? id)> CreateAsync(DepartmentCreateVm vm, CancellationToken ct = default)
        => _service.CreateAsync(new DepartmentCreateRequest(vm.Code, vm.Name, vm.IsActive, vm.IsGeneral), ct);

    public Task<(bool ok, string? error)> UpdateAsync(DepartmentEditVm vm, CancellationToken ct = default)
        => _service.UpdateAsync(new DepartmentUpdateRequest(vm.Id, vm.Code, vm.Name, vm.IsActive, vm.IsGeneral), ct);

    public Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
        => _service.DeactivateAsync(id, ct);
}
