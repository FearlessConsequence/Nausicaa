using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Threading;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Models;
using CourseWork.Controls;

namespace CourseWork.Views;

public partial class CitizenCardWindow : Window
{
    private Citizen? _citizen;
    private Window? _searchWindow;
    private readonly int _currentUserId;

    public CitizenCardWindow() : this(0) { }
    
    public CitizenCardWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        btnClose.Click += (s, e) => Close();
        btnViewDocuments.Click += BtnViewDocuments_Click;
    }

    // ✅ ДОБАВЛЕН: Конструктор, принимающий Citizen (решает ошибку компиляции)
    public CitizenCardWindow(int currentUserId, Citizen citizen) : this(currentUserId)
    {
        SetCitizen(citizen);
    }

    public void SetCitizen(Citizen citizen)
    {
        _citizen = citizen;
        
        txtLastName.Text = citizen.LastName;
        txtFirstName.Text = citizen.FirstName;
        txtPatronymic.Text = citizen.Patronymic ?? "Не указано";
        txtBirthday.Text = citizen.Birthday.ToString("dd.MM.yyyy");
        txtAge.Text = CalculateAge(citizen.Birthday).ToString();
        txtCitizenship.Text = citizen.CitizenshipName ?? "Российская Федерация";
        txtPhone.Text = citizen.Phone ?? "Не указан";
        txtAddress.Text = citizen.Address ?? "Не указан";
        txtPassport.Text = citizen.Passport ?? "Не указан";
        txtWorkPlace.Text = citizen.WorkingPlaceName ?? "Не указано";
        txtEducation.Text = citizen.EducationName ?? "Не указано";
        txtFamilyStatus.Text = citizen.FamilyStatusName ?? "Не указано";
        txtPosition.Text = citizen.PostName ?? "Не указана";
        txtConvictions.Text = citizen.CriminalRecord 
            ? $"Есть судимость ({(citizen.CountRecord.HasValue ? citizen.CountRecord.Value.ToString() : "кол-во не указано")})" 
            : "Нет судимостей";
        txtPosition.Text = citizen.PostName ?? "Не указана";
    }
        
    private async void BtnViewDocuments_Click(object? sender, RoutedEventArgs e)
    {
        if (_citizen == null) return;

        var reasonWindow = new RequestReasonWindow();
        var reason = await reasonWindow.ShowDialog<string?>(this);

        if (!string.IsNullOrWhiteSpace(reason))
        {
            // 1️⃣ Открываем целевое окно
            var documentsWindow = new CitizenDocumentsWindow(_currentUserId, _citizen.Id, _citizen.FullName, this);
            documentsWindow.Show();

            // 2️⃣ Закрываем ВСЕ остальные окна (правильный способ для Avalonia)
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var allWindows = desktop.Windows.ToList(); // Создаём копию списка
                foreach (var win in allWindows)
                {
                    if (win != documentsWindow)
                    {
                        win.Close();
                    }
                }
            }
        }
    }
    
    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    public void SetSearchWindow(Window searchWindow)
    {
        _searchWindow = searchWindow;
    }
}