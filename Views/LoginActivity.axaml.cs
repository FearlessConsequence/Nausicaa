using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class LoginActitvity : Window
{
    private readonly Window _previousWindow;
    public LoginActitvity()
    {
        InitializeComponent();
        btnLogin.Click += BtnLogin_Click;
    }
    
    private async void BtnLogin_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var db = new DatabaseHelper();
            
            string username = txtUsername.Text?.Trim() ?? "";
            string password = txtPassword.Text?.Trim() ?? "";
            
            var user = await db.AuthenticateUserWithRoleAsync(username, password);
            
            if (user != null)
            {
                App.CurrentUserId = user.Id;
                App.CurrentUserRole = user.Role;
                
                new MainWindow(user.Id).Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Неверный логин или пароль";
                txtError.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            txtError.Text = $"Ошибка: {ex.Message}";
            txtError.IsVisible = true;
        }
    }
}