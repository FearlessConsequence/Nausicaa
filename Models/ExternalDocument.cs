using System;

namespace CourseWork.Models;

public class ExternalDocument
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string MaskedNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CitizenFullName { get; set; } = string.Empty; 
    public string DealInfo { get; set; } = string.Empty;
    public int? CitizenId { get; set; } 
    public int? DealId { get; set; }    
}

