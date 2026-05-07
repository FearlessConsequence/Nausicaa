using CourseWork.Models;

namespace CourseWork.Helpers;

public static class RoleHelper
{
    public static bool CanCreateDocument(UserRole role, string documentType)
    {
        return role switch
        {
            UserRole.PoliceOfficer => true,  // Может все
            UserRole.AdminInspector => true, // Может все
            UserRole.Judge => documentType == "resolution", // Только постановления
            UserRole.MedicalExpert => documentType == "examination", // Только экспертизы
            _ => false
        };
    }
    
    public static string GetRoleDisplayName(UserRole role)
    {
        return role switch
        {
            UserRole.PoliceOfficer => "Сотрудник полиции",
            UserRole.Judge => "Судья",
            UserRole.MedicalExpert => "Медицинский эксперт",
            UserRole.AdminInspector => "Инспектор административной практики",
            _ => "Неизвестно"
        };
    }
}