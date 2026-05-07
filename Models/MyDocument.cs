using System;

namespace CourseWork.Models;

public class MyDocument
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? Number { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CitizenId { get; set; }
    public bool IsFavorite { get; set; } = false;
}