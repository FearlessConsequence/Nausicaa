using System;

namespace CourseWork.Models;

public class Appeal
{
    public int Id { get; set; }
    public int? Number { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CitizenId { get; set; }
    public int OfficerId { get; set; }
}