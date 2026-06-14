namespace CourseWork.Models;

public enum UserRole
{
    PoliceOfficer = 1,      // Полицейский
    Judge = 2,              // Судья
    MedicalExpert = 3,      // Врач
    AdminInspector = 5,     // Инспектор адм. практики (было 4, стало 5)
    ForensicExpert = 4, 
    ChiefOfPolice = 6      // Судмедэксперт (было 5, стало 4)
}
public class UserWithRole
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public UserRole Role { get; set; } = UserRole.PoliceOfficer;
    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    public string? Rank { get; set; }  // ← звание из таблицы rank
    public int? Age { get; set; }       // ← возраст
    public string? WorkPlace { get; set; }
}

