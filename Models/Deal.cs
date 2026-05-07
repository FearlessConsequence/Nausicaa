using System;

namespace CourseWork.Models;

public class Deal
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CitizenFullName { get; set; } = string.Empty;
    public DateTime DealDate { get; set; }
    public string DateFormatted => DealDate.ToString("dd.MM.yyyy");
}

public class DealSearchParams
{
    public string? FullName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Address { get; set; }
    public string? Passport { get; set; }
    public string? Phone { get; set; }
    public string? DealNumber { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}