using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class RecentsWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private List<MyDocument> _allDocuments = new();
    private List<MyDocument> _currentDocuments = new();
    private string _selectedFilterType = "Все";

    public RecentsWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        // Настройка кнопок фильтрации
        btn_filter_all.Click += (s, e) => SelectFilter("Все");
        btn_filter_appeals.Click += (s, e) => SelectFilter("Обращение");
        btn_filter_statements.Click += (s, e) => SelectFilter("Заявление");
        btn_filter_protocols.Click += (s, e) => SelectFilter("Административный протокол");
        btn_filter_explanations.Click += (s, e) => SelectFilter("Протокол объяснения");
        btn_filter_reports.Click += (s, e) => SelectFilter("Направление на мед. освид.");
        btn_filter_medical_cert.Click += (s, e) => SelectFilter("Акт медицинского освидетельствования");
        btn_filter_forensic.Click += (s, e) => SelectFilter("Судебно-медицинская экспертиза");
        btn_filter_resolution.Click += (s, e) => SelectFilter("Постановление");
        
        dp_date_from.SelectedDateChanged += (s, e) => ApplyFilters();
        dp_date_to.SelectedDateChanged += (s, e) => ApplyFilters();
        
        ConfigureFiltersByRole();
        
        this.Opened += async (s, e) => await LoadDocumentsAsync();
    }
    
    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        // Скрываем все кнопки по умолчанию
        btn_filter_all.IsVisible = false;
        btn_filter_appeals.IsVisible = false;
        btn_filter_statements.IsVisible = false;
        btn_filter_protocols.IsVisible = false;
        btn_filter_explanations.IsVisible = false;
        btn_filter_reports.IsVisible = false;
        btn_filter_medical_cert.IsVisible = false;
        btn_filter_forensic.IsVisible = false;
        btn_filter_resolution.IsVisible = false;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
                // Полицейский — все типы
                btn_filter_all.IsVisible = true;
                btn_filter_appeals.IsVisible = true;
                btn_filter_statements.IsVisible = true;
                btn_filter_protocols.IsVisible = true;
                btn_filter_explanations.IsVisible = true;
                btn_filter_reports.IsVisible = true;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_forensic.IsVisible = false;
                btn_filter_resolution.IsVisible = false;
                break;
                
            case UserRole.AdminInspector:
                // Инспектор — все типы
                btn_filter_all.IsVisible = true;
                btn_filter_appeals.IsVisible = true;
                btn_filter_statements.IsVisible = true;
                btn_filter_protocols.IsVisible = true;
                btn_filter_explanations.IsVisible = true;
                btn_filter_reports.IsVisible = true;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_forensic.IsVisible = false;
                btn_filter_resolution.IsVisible = false;
                break;
                
            case UserRole.ChiefOfPolice:
                // Начальник — все типы
                btn_filter_all.IsVisible = true;
                btn_filter_appeals.IsVisible = true;
                btn_filter_statements.IsVisible = true;
                btn_filter_protocols.IsVisible = true;
                btn_filter_explanations.IsVisible = true;
                btn_filter_reports.IsVisible = true;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_forensic.IsVisible = false;
                btn_filter_resolution.IsVisible = false;
                break;
                
            case UserRole.MedicalExpert:
                // Врач — только направления и акты
                btn_filter_all.IsVisible = true;
                btn_filter_reports.IsVisible = true;
                btn_filter_medical_cert.IsVisible = true;
                break;
                
            case UserRole.Judge:
                // Судья — все типы
                btn_filter_all.IsVisible = false;
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_reports.IsVisible = false;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_forensic.IsVisible = false;
                btn_filter_resolution.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                // Судмедэксперт — только экспертизы
                btn_filter_all.IsVisible = false;
                btn_filter_forensic.IsVisible = true;
                break;
        }
    }
    
    private void SelectFilter(string filterType)
    {
        _selectedFilterType = filterType;
        UpdateFilterButtonsUI(filterType);
        ApplyFilters();
    }
    
    private void UpdateFilterButtonsUI(string filterType)
    {
        var activeColor = new SolidColorBrush(Color.Parse("#0F4B5E"));
        var inactiveColor = new SolidColorBrush(Color.Parse("#E9ECEF"));
        var activeForeground = new SolidColorBrush(Color.Parse("White"));
        var inactiveForeground = new SolidColorBrush(Color.Parse("#0F4B5E"));
        
        var buttons = new Dictionary<string, Button>
        {
            {"Все", btn_filter_all},
            {"Обращение", btn_filter_appeals},
            {"Заявление", btn_filter_statements},
            {"Административный протокол", btn_filter_protocols},
            {"Протокол объяснения", btn_filter_explanations},
            {"Направление на мед. освид.", btn_filter_reports},
            {"Акт медицинского освидетельствования", btn_filter_medical_cert},
            {"Судебно-медицинская экспертиза", btn_filter_forensic},
            {"Постановление", btn_filter_resolution}
        };
        
        foreach (var btn in buttons)
        {
            if (!btn.Value.IsVisible) continue;
            
            if (btn.Key == filterType)
            {
                btn.Value.Background = activeColor;
                btn.Value.Foreground = activeForeground;
            }
            else
            {
                btn.Value.Background = inactiveColor;
                btn.Value.Foreground = inactiveForeground;
            }
        }
    }

    private async Task LoadDocumentsAsync()
    {
        try
        {
            var role = App.CurrentUserRole;
            var userId = App.CurrentUserId;
            
            var recentDocs = await _db.GetAllDocumentsAsync(role, userId);
            
            _allDocuments = recentDocs.Select(d => new MyDocument
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                TableName = GetTableName(d.DocumentType), // ← правильное получение имени таблицы
                Number = d.Number,
                CreatedAt = d.MakingDateAndTime,
                CitizenFullName = d.CitizenName ?? "Неизвестно",
                Content = "",
                IsFavorite = false
            }).ToList();
            
            // Загружаем реальный статус избранного
            for (int i = 0; i < _allDocuments.Count; i++)
            {
                var doc = _allDocuments[i];
                doc.IsFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
            }
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDocumentsAsync: {ex.Message}");
            emptyStateBorder.IsVisible = true;
        }
    }
    
    private void ApplyFilters()
    {
        var filtered = _allDocuments.AsEnumerable();
        
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType);
        }
        
        if (dp_date_from.SelectedDate.HasValue)
        {
            var dateFrom = dp_date_from.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date >= dateFrom);
        }
        
        if (dp_date_to.SelectedDate.HasValue)
        {
            var dateTo = dp_date_to.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date <= dateTo);
        }
        
        _currentDocuments = filtered.ToList();
        documentsContainer.ItemsSource = _currentDocuments;
        emptyStateBorder.IsVisible = _currentDocuments.Count == 0;
        
        // Подписываем кнопки после обновления списка
        Dispatcher.UIThread.Post(() => SubscribeToButtons(), DispatcherPriority.Render);
    }

    // ✅ ПРАВИЛЬНЫЙ метод получения имени таблицы
    private string GetTableName(string documentType)
    {
        return documentType switch
        {
            "Заявление" => "statement",
            "Обращение" => "appeals",
            "Протокол объяснения" => "explanation_protocol",
            "Направление на мед. освид." => "medical_examination_report",
            "Административный протокол" => "administrative_protocol",
            "Акт медицинского освидетельствования" => "medical_examination_certificate", // ← правильное имя!
            "Судебно-медицинская экспертиза" => "forensic_medical_examination",
            "Постановление" => "resolution",
            "Дело" => "deal",
            _ => "unknown"
        };
    }

    private void SubscribeToButtons()
    {
        try
        {
            var allButtons = this.GetVisualDescendants()
                .OfType<Button>()
                .ToList();
            
            foreach (var button in allButtons)
            {
                if (button.Name == "FavoriteButton")
                {
                    button.Click -= OnFavoriteClick;
                    button.Click += OnFavoriteClick;
                    
                    var doc = button.DataContext as MyDocument;
                    if (doc != null)
                    {
                        button.Tag = doc;
                        button.Content = doc.IsFavorite ? "★" : "☆";
                        button.Foreground = doc.IsFavorite 
                            ? new SolidColorBrush(Color.Parse("#FFB800")) 
                            : new SolidColorBrush(Color.Parse("#6C757D"));
                    }
                }
                else if (button.Name == "OpenButton")
                {
                    button.Click -= OnOpenClick;
                    button.Click += OnOpenClick;
                    
                    var doc = button.DataContext as MyDocument;
                    if (doc != null)
                    {
                        button.Tag = doc;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] SubscribeToButtons: {ex.Message}");
        }
    }

    private async void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                await _db.ToggleFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                bool isFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                
                // Обновляем кнопку
                button.Content = isFavorite ? "★" : "☆";
                button.Foreground = isFavorite 
                    ? new SolidColorBrush(Color.Parse("#FFB800")) 
                    : new SolidColorBrush(Color.Parse("#6C757D"));
                
                // Обновляем статус в списке
                doc.IsFavorite = isFavorite;
                var existingDoc = _currentDocuments.FirstOrDefault(d => d.Id == doc.Id && d.TableName == doc.TableName);
                if (existingDoc != null)
                {
                    existingDoc.IsFavorite = isFavorite;
                }
                
                // Обновляем в _allDocuments
                var allDoc = _allDocuments.FirstOrDefault(d => d.Id == doc.Id && d.TableName == doc.TableName);
                if (allDoc != null)
                {
                    allDoc.IsFavorite = isFavorite;
                }
                if (isFavorite)
                {
                    NotificationsControl.ShowSuccess("Избранное", $"Документ «{doc.DocumentType}» добавлен в избранное");
                }
                else
                {
                    NotificationsControl.ShowSuccess("Избранное", $"Документ «{doc.DocumentType}» удалён из избранного");
                }
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
        else
        {
            NotificationsControl.ShowError("Ошибка", "Не удалось определить документ");
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                
                if (fullDoc == null)
                {
                    NotificationsControl.ShowError("Ошибка", "Документ не найден");
                    return;
                }
                
                var viewer = new DocumentViewerWindow(_currentUserId, fullDoc, this);
                viewer.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
        else
        {
            NotificationsControl.ShowError("Ошибка", "Не удалось определить документ");
        }
    }
}