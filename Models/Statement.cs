using System;

namespace CourseWork.Models;

public class Statement
{
    public int Id { get; set; }
    public int? Number { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OfficerId { get; set; }
    public string OfficerFullName { get; set; } = string.Empty;
    public bool SignatureApplicant { get; set; }
    public bool SignaturePoliceOfficer { get; set; }
}