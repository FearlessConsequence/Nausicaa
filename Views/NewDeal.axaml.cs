using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class NewDeal : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private int? _selectedOffenderId;
    private readonly UserRole _currentUserRole;
    private int? _selectedWitness1Id;
    private int? _selectedWitness2Id;

    public NewDeal(int currentUserId, UserRole role)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = currentUserId;
        _currentUserRole = _currentUserRole;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_create.Click += Btn_create_Click;
        btn_cancel.Click += Btn_cancel_Click;
        btn_select_offender.Click += Btn_select_offender_Click;
        btn_select_witness1.Click += Btn_select_witness1_Click;
        btn_select_witness2.Click += Btn_select_witness2_Click;
    }
    
    private async void Btn_select_offender_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selected = citizensWindow.SelectedCitizen;
                if (selected != null)
                {
                    txt_offender.Text = selected.FullName;
                    _selectedOffenderId = selected.Id;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }
    
    private async void Btn_select_witness1_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selected = citizensWindow.SelectedCitizen;
                if (selected != null)
                {
                    txt_witness1.Text = selected.FullName;
                    _selectedWitness1Id = selected.Id;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }
    
    private async void Btn_select_witness2_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selected = citizensWindow.SelectedCitizen;
                if (selected != null)
                {
                    txt_witness2.Text = selected.FullName;
                    _selectedWitness2Id = selected.Id;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }   
    
    private async void Btn_create_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txt_deal_number.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите номер дела");
                return;
            }
            if (!int.TryParse(txt_deal_number.Text, out int dealNumber))
            {
                NotificationsControl.ShowWarning("Ошибка", "Номер дела должен быть числом");
                return;
            }
            if (string.IsNullOrWhiteSpace(txt_city.Text))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите город");
                return;
            }
            if (_selectedOffenderId == null)
            {
                NotificationsControl.ShowWarning("Ошибка", "Выберите нарушителя");
                return;
            }
            
            // Получаем ID статьи по номеру
            string articleNumber = txt_article.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(articleNumber))
            {
                NotificationsControl.ShowWarning("Ошибка", "Введите номер статьи");
                return;
            }
            
            int? articleId = await _db.GetArticleIdByNumberAsync(articleNumber);
            if (articleId == null)
            {
                NotificationsControl.ShowWarning("Ошибка", $"Статья с номером '{articleNumber}' не найдена");
                return;
            }
            
            // Получаем citizen_post_id текущего пользователя
            int? policeOfficerId = await _db.GetCitizensAndPostsIdByUserIdAsync(_currentUserId);
            if (policeOfficerId == null)
            {
                NotificationsControl.ShowWarning("Ошибка", "Не удалось определить должность сотрудника");
                return;
            }
            
            int responsibilityId = 1;

            int newId = await _db.CreateDealAsync(
                dealNumber: dealNumber,
                offenderId: _selectedOffenderId.Value,
                firstWitnessId: _selectedWitness1Id,
                secondWitnessId: _selectedWitness2Id,
                policeOfficerId: policeOfficerId.Value,
                articleId: articleId.Value,
                responsibilityId: responsibilityId
            );
            
            NotificationsControl.ShowSuccess("Успех", $"Дело №{dealNumber} успешно создано!");
            
            new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
            this.Close();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось создать дело: {ex.Message}");
        }
    }
    
    private void Btn_cancel_Click(object? sender, RoutedEventArgs e)
    {
        new MainWindow(App.CurrentUserId, App.CurrentUserRole).Show();
        this.Close();
    }
}