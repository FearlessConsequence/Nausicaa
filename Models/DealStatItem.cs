using System;

namespace CourseWork.Models;

public class DealStatItem
{
    public int DealId { get; set; }
    public string DealNumber { get; set; } = string.Empty;
    public string ArticleName { get; set; } = string.Empty;
    public string OfficerName { get; set; } = string.Empty;
    public DateTime DealDate { get; set; }
    public string DateFormatted => DealDate.ToString("dd.MM.yyyy");
    public bool HasResolution { get; set; }
}