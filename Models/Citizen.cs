using System;

namespace CourseWork.Models;

public class Citizen
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public DateTime Birthday { get; set; }
    public string? Address { get; set; }
    public string? Passport { get; set; }
    public bool CriminalRecord { get; set; }
    public int? CountRecord { get; set; }
    public int? Post { get; set; }           // должность (id из таблицы post)
    public int? WorkingPlace { get; set; }    // место работы (id из structures)
    public int? Education { get; set; }       // образование (id)
    public int? FamilyStatus { get; set; }    // семейное положение (id)
    public int? Citizenship { get; set; }     // гражданство (id)
    
    // Для отображения названий (заполняются из JOIN)
    public string? Phone { get; set; }
    public string? WorkingPlaceName { get; set; }
    public string? EducationName { get; set; }
    public string? FamilyStatusName { get; set; }
    public string? CitizenshipName { get; set; }
    public string? PostName { get; set; }
    
    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
}