namespace HospitalTriage.Web.ViewModels.Triage;

public sealed class TriageWaitingListVm
{
    public DateOnly Date { get; set; }
    public List<TriageWaitingItemVm> Items { get; set; } = new();
}

public sealed class TriageWaitingItemVm
{
    public int VisitId { get; set; }
    public int QueueNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly VisitDate { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
}
