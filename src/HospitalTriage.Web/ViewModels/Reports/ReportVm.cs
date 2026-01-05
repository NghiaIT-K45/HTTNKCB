using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.ViewModels.Reports;

public sealed class ReportFilterVm
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly FromDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

    [Required]
    [DataType(DataType.Date)]
    public DateOnly ToDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public int? DepartmentId { get; set; }

    public List<SelectListItem> Departments { get; set; } = new();
}

public sealed class VisitsPerDayItemVm
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public sealed class ReportResultVm
{
    public List<VisitsPerDayItemVm> VisitsPerDay { get; set; } = new();
    public double AverageWaitingMinutes { get; set; }
    public int SampleCount { get; set; }
}

public sealed class ReportIndexVm
{
    public ReportFilterVm Filter { get; set; } = new();
    public ReportResultVm? Result { get; set; }
}
