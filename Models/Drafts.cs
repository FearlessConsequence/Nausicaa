using System;

namespace CourseWork.Models;

public class Draft
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Number { get; set; }
    public DateTime? DocumentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // для JSON
    public string FormDataJson { get; set; } = "{}";
    
    // для отображения в UI
    public string Preview { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    
    // для обращения и заявления
    public int? CitizenId { get; set; }
    public int? ApplicantId { get; set; }
    
    // для административного протокола и объяснения
    public int? DealId { get; set; }
    public string? ProtocolNumber { get; set; }
    public string? Description { get; set; }
    public string? OtherInfo { get; set; }
    public int? Witness1Id { get; set; }
    public int? Witness2Id { get; set; }
    
    // для направления на медосвидетельствование
    public int? PatientId { get; set; }
    public int? ReportTypeId { get; set; }
    public string? Signs { get; set; }
    
    // для протокола объяснения
    public bool? NeedMedicalExamination { get; set; }
    public bool? NeedCertificate { get; set; }
    
    // для заявления
    public bool? SignatureApplicant { get; set; }
    public bool? SignatureOfficer { get; set; }
    
    // вычисляемые свойства для отображения
    public string TypeDisplayName => DocumentType switch
    {
        "appeals" => "Обращение",
        "statement" => "Заявление",
        "administrative_protocol" => "Административный протокол",
        "examination_report" => "Направление на мед. освидетельствование",
        "explanation_protocol" => "Протокол объяснения",
        _ => DocumentType
    };
    
    public string DateFormatted => UpdatedAt.ToString("dd.MM.yyyy HH:mm");
}