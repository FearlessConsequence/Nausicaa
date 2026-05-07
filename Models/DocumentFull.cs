using System;

namespace CourseWork.Models;

public class DocumentFull
{
    // общие поля
    public string DocumentType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    // для административного протокола
    public string DealNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OtherInformation { get; set; } = string.Empty;
    public bool SignatureForKnowing { get; set; }
    public string FirstWitnessName { get; set; } = string.Empty;
    public string SecondWitnessName { get; set; } = string.Empty;
    public string OfficerName { get; set; } = string.Empty;
    public string ArticleName { get; set; } = string.Empty;
    
    // для других типов документов
    public string PatientName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string SignsOfIntoxication { get; set; } = string.Empty;
}