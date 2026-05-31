using System;

public class MedicalExaminationReport
{
    public int Id { get; set; }
    public int Number { get; set; }
    public DateTime MakingDate { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string? DealNumber { get; set; }
}