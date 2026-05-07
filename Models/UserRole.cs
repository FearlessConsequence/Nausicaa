namespace CourseWork.Models;

public enum UserRole
{
    PoliceOfficer = 1, 
    Judge = 2,         
    MedicalExpert = 3, 
    AdminInspector = 4 
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