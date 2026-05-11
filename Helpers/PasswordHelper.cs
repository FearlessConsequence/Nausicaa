using System;
using System.Security.Cryptography;
using System.Text;

namespace CourseWork.Helpers;

public static class PasswordHelper
{
    // Хэширование пароля
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    // Проверка пароля
    public static bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }
}