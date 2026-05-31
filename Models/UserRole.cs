namespace CourseWork.Models;

public enum UserRole
{
    PoliceOfficer = 1,      // Полицейский
    Judge = 2,              // Судья
    MedicalExpert = 3,      // Врач (мед. освидетельствование)
    AdminInspector = 4,     // Инспектор адм. практики
    ForensicExpert = 5      // Судмедэксперт
}
public class UserWithRole
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public UserRole Role { get; set; } = UserRole.PoliceOfficer;
}