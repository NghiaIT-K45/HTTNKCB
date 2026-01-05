using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Web.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class ReportApiService
{
    private readonly IReportService _reportService;
    private readonly IDepartmentService _departmentService;

    public ReportApiService(IReportService reportService, IDepartmentService departmentService)
    {
        _reportService = reportService;
        _departmentService = departmentService;
    }

    public async Task<(bool ok, string? error, ReportIndexVm? data)> BuildIndexVmAsync(
        ReportFilterVm? filter,
        bool runReport,
        CancellationToken ct = default)
    {
        try
        {
            filter ??= new ReportFilterVm();

            var depts = await _departmentService.SearchAsync(keyword: null, ct);
            filter.Departments = BuildDepartmentSelectList(depts, filter.DepartmentId);

            var vm = new ReportIndexVm
            {
                Filter = filter
            };

            if (!runReport)
                return (true, null, vm);

            var (ok, error, result) = await _reportService.GetReportAsync(
                new ReportRequest(filter.FromDate, filter.ToDate, filter.DepartmentId),
                ct);

            if (!ok || result is null)
                return (false, error, null);

            vm.Result = new ReportResultVm
            {
                VisitsPerDay = result.VisitsPerDay
                    .Select(x => new VisitsPerDayItemVm { Date = x.Date, Count = x.Count })
                    .ToList(),
                AverageWaitingMinutes = result.AvgWaitingTime.AverageMinutes,
                SampleCount = result.AvgWaitingTime.SampleCount
            };

            return (true, null, vm);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool ok, string? error, (byte[] content, string fileName)? data)> ExportVisitsPerDayCsvAsync(
        ReportFilterVm filter,
        CancellationToken ct = default)
    {
        var (ok, error, result) = await _reportService.GetReportAsync(
            new ReportRequest(filter.FromDate, filter.ToDate, filter.DepartmentId),
            ct);

        if (!ok || result is null)
            return (false, error, null);

        var csv = _reportService.ExportVisitsPerDayCsv(result);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

        var fileName = $"visits-per-day_{filter.FromDate:yyyyMMdd}_{filter.ToDate:yyyyMMdd}.csv";
        return (true, null, (bytes, fileName));
    }

    private static List<SelectListItem> BuildDepartmentSelectList(IEnumerable<HospitalTriage.Domain.Entities.Department> depts, int? selectedId)
        => depts
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.Code} - {d.Name}",
                Selected = selectedId.HasValue && d.Id == selectedId.Value
            })
            .ToList();
}
