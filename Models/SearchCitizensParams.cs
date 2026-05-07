using System;

namespace CourseWork.Models;

public class CitizenSearchParams
{
    public string? FullName { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Patronymic { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Passport { get; set; }
}