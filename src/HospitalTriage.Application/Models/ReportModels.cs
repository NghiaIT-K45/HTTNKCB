namespace HospitalTriage.Application.Models;

public sealed record ReportRequest(
    DateOnly FromDate,
    DateOnly ToDate,
    int? DepartmentId
);

public sealed record VisitsPerDayItem(
    DateOnly Date,
    int Count
);

public sealed record AvgWaitingTimeResult(
    double AverageMinutes,
    int SampleCount
);

public sealed record ReportResult(
    List<VisitsPerDayItem> VisitsPerDay,
    AvgWaitingTimeResult AvgWaitingTime
);
