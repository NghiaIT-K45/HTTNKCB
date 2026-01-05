namespace HospitalTriage.Domain.Enums;

public enum VisitStatus
{
    Registered = 0,
    WaitingTriage = 1,
    Triaged = 2,
    WaitingDoctor = 3,
    InExamination = 4,
    Done = 5
}
