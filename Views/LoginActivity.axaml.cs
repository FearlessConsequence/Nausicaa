using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class LoginActitvity : Window
{ 
    public LoginActitvity()
    {
        try
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
        }
        catch (Exception ex)
        {
            ShowDebug(1, $"Ошибка инициализации: {ex.Message}");
        }
    }
    
    private void ShowDebug(int number, string message)
    {
        var debugBlock = number switch
        {
            1 => txtDebug1, 
            2 => txtDebug2,
            3 => txtDebug3,
            4 => txtDebug4,
            _ => txtDebug1
        };
        
        debugBlock.Text = message;
        debugBlock.IsVisible = true;
    }
    
    private void ClearDebug()
    {
        txtDebug1.IsVisible = false;
        txtDebug2.IsVisible = false;
        txtDebug3.IsVisible = false;
        txtDebug4.IsVisible = false;
        txtDebug1.Text = "";
        txtDebug2.Text = "";
        txtDebug3.Text = "";
        txtDebug4.Text = "";
    }
    
    private void ShowError(string message)
    {
        txtError.Text = message;
        txtError.IsVisible = true;
    }
    
    private async void BtnLogin_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            ClearDebug();
            btnLogin.IsEnabled = false;
            txtError.IsVisible = false;
            
            ShowDebug(1, "Шаг 1: Получаем логин и пароль...");
            
            string username = txtUsername.Text?.Trim() ?? "";
            string password = txtPassword.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }
            
            ShowDebug(2, $"Шаг 2: Подключаемся к БД...");
            
            var db = new DatabaseHelper();
            
            ShowDebug(3, $"Шаг 3: Ищем пользователя '{username}'...");
            
            var user = await db.AuthenticateUserWithRoleAsync(username, password);
            
            if (user != null)
            {
                ShowDebug(4, $"Шаг 4: Пользователь найден! Id={user.Id}, Role={user.Role}");
                
                App.CurrentUserId = user.Id;
                App.CurrentUserRole = user.Role;
                
                ShowDebug(4, $"Шаг 5: Создаём MainWindow...");
                
                var mainWindow = new MainWindow(user.Id, user.Role);
                mainWindow.Show();
                
                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль");
                ShowDebug(4, $"Пользователь '{username}' не найден или пароль неверный");
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Ошибка: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nВнутренняя: {ex.InnerException.Message}";
            }
            ShowError(errorMessage);
            ShowDebug(4, $"Stack: {ex.StackTrace}");
        }
        finally
        {
            btnLogin.IsEnabled = true;
        }
    }
}