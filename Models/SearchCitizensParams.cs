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
    
    // ✅ Проверка, есть ли хоть какие-то данные для поиска
    public bool HasAnyCriteria()
    {
        return !string.IsNullOrWhiteSpace(FullName) ||
               !string.IsNullOrWhiteSpace(LastName) ||
               !string.IsNullOrWhiteSpace(FirstName) ||
               !string.IsNullOrWhiteSpace(Patronymic) ||
               Birthday.HasValue ||
               !string.IsNullOrWhiteSpace(Address) ||
               !string.IsNullOrWhiteSpace(Phone) ||
               !string.IsNullOrWhiteSpace(Passport);
    }
    
    // ✅ Проверка, заполнено ли ФИО (обязательное условие)
    public bool HasNameCriteria()
    {
        return !string.IsNullOrWhiteSpace(LastName) ||
               !string.IsNullOrWhiteSpace(FullName);
    }
}