using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewCitizen : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;

    public NewCitizen(int currentUserId, UserRole role)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = App.CurrentUserId;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_create.Click += Btn_create_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_save_draft.Click += Btn_save_draft_Click;
        
        // Показываем/скрываем поле количества судимостей
        chk_criminal_record.Click += (s, e) => 
        {
            panel_count_record.IsVisible = chk_criminal_record.IsChecked == true;
        };

        // В конструкторе добавьте обработчики для чекбоксов
        chk_russian_citizenship.Click += (s, e) => 
        {
            if (chk_russian_citizenship.IsChecked == true)
            {
                chk_other_citizenship.IsChecked = false;
                txt_other_citizenship.IsEnabled = false;
                txt_other_citizenship.IsVisible = false;
            }
        };

        chk_other_citizenship.Click += (s, e) =>
        {
            if (chk_other_citizenship.IsChecked == true)
            {
                chk_russian_citizenship.IsChecked = false;
                txt_other_citizenship.IsEnabled = true;
                txt_other_citizenship.IsVisible = true;
            }
            else
            {
                txt_other_citizenship.IsEnabled = false;
                txt_other_citizenship.IsVisible = false;
            }
        };
    }
    
    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txt_last_name.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите фамилию");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_first_name.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите имя");
                return;
            }
            if (dp_birthday.SelectedDate == null)
            {
                NotificationsControl.ShowWarning("Ошибка", "Выберите дату рождения");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_birth_city.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите город рождения");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_address.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите адрес регистрации");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_passport.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите паспортные данные");
                return;
            }
            
            // Получаем значения из ComboBox
            string familyStatus = (cmb_family_status.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string education = (cmb_education.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string citizenship = "Российская Федерация";
            if (chk_other_citizenship.IsChecked == true && !string.IsNullOrWhiteSpace(txt_other_citizenship.Text))
            {
                citizenship = txt_other_citizenship.Text.Trim();
            }
            
            var citizen = new Citizen
            {
                LastName = txt_last_name.Text.Trim(),
                FirstName = txt_first_name.Text.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(txt_patronymic.Text) ? null : txt_patronymic.Text.Trim(),
                Birthday = dp_birthday.SelectedDate.Value.DateTime,
                Address = txt_address.Text.Trim(),
                Passport = txt_passport.Text.Trim(),
                CriminalRecord = chk_criminal_record.IsChecked == true,
                CountRecord = chk_criminal_record.IsChecked == true ? (int.TryParse(txt_count_record.Text, out int count) ? count : 0) : 0,
                WorkingPlace = 1,
                Post = 1,
                FamilyStatus = 1,
                Education = 1,
                Citizenship = 1,
                Phone = txt_phone.Text.Trim()
            };
            
            int newId = await _db.CreateCitizenAsync(citizen);
            
            NotificationsControl.ShowSuccess("Успех", $"Гражданин {citizen.FullName} добавлен");
            
            new NewCitizen(App.CurrentUserId, App.CurrentUserRole).Show();
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать гражданина: {ex.Message}");
        }
    }
    
    private void Btn_cancel_Click(object? sender, RoutedEventArgs e)
    {
        new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
        this.Close();
    }
    
    private async void Btn_save_draft_Click(object? sender, RoutedEventArgs e)
    {
        NotificationsControl.ShowInfo("Черновик", "Сохранение черновика пока не реализовано");
    }
}