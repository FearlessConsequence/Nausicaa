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
        InitializeComponent();
        btnLogin.Click += BtnLogin_Click;

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
            btnLogin.IsEnabled = false;
            txtError.IsVisible = false;
            
            
            string username = txtUsername.Text?.Trim() ?? "";
            string password = txtPassword.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }
            
            
            var db = new DatabaseHelper();
            
            
            var user = await db.AuthenticateUserWithRoleAsync(username, password);
            
            if (user != null)
            {
                
                App.CurrentUserId = user.Id;
                App.CurrentUserRole = user.Role;
                
                
                // ✅ Выбор окна в зависимости от роли
                Window targetWindow;
                
                switch (user.Role)
                {
                    case UserRole.AdminInspector:
                        targetWindow = new MainWindow(user.Id, user.Role);
                        break;
                        
                    case UserRole.ChiefOfPolice:
                        App.CurrentUserId = user.Id;
                        App.CurrentUserRole = UserRole.ChiefOfPolice;  // ← сначала
                        App.IsChief = true;
                        targetWindow = new MainWindow(user.Id, UserRole.ChiefOfPolice);  // ← потом
                        break;
                        
                    case UserRole.Judge:
                        targetWindow = new MainWindow(user.Id, user.Role);
                        break;
                        
                    case UserRole.MedicalExpert:
                        targetWindow = new MainWindow(user.Id, user.Role);
                        break;
                        
                    case UserRole.ForensicExpert:
                        targetWindow = new MainWindow(user.Id, user.Role);
                        break;
                        
                    case UserRole.PoliceOfficer:
                    default:
                        targetWindow = new MainWindow(user.Id, user.Role);
                        break;
                }
                
                targetWindow.Show();
                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль");
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
        }
        finally
        {
            btnLogin.IsEnabled = true;
        }
    }
}   