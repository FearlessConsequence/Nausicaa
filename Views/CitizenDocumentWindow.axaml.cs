using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class CitizenDocumentsWindow : Window
{
    public CitizenDocumentsWindow() : this(0, 0, null, null) { }
    private readonly int _currentUserId;
    private readonly int _citizenId;
    
    private readonly string? _citizenFullName;
    private readonly DatabaseHelper _db;
    
    private List<MyDocument> _allDocuments = new();
    private List<MyDocument> _currentDocuments = new();
    private string _selectedFilterType = "Все";
    private string _searchText = "";

    // Конструктор
    public CitizenDocumentsWindow(int currentUserId, int citizenId, string? citizenFullName, Window? parent = null)
    {
        InitializeComponent();
        
        _currentUserId = currentUserId;
        _citizenId = citizenId;
        _citizenFullName = citizenFullName;
        _db = new DatabaseHelper();
        
        // Настройка левой панели
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        // Заголовки
        txtTitle.Text = "Документы гражданина";
        txtSubtitle.Text = citizenFullName;
        
        // Настройка кнопок фильтрации (только меняют UI, не вызывают поиск)
        SetupFilterButtons();
        
        // Подписка на кнопки
        btnBack.Click += BtnBack_Click;
        btn_search.Click += BtnSearch_Click;
    }

    // Настройка кнопок фильтрации
    private void SetupFilterButtons()
    {
        btn_filter_all.Click += (s, e) => SelectFilter("Все");
        btn_filter_appeals.Click += (s, e) => SelectFilter("Обращение");
        btn_filter_statements.Click += (s, e) => SelectFilter("Заявление");
        btn_filter_protocols.Click += (s, e) => SelectFilter("Административный протокол");
        btn_filter_explanations.Click += (s, e) => SelectFilter("Протокол объяснения");
        btn_filter_reports.Click += (s, e) => SelectFilter("Направление на мед. освид.");
        btn_filter_medical_cert.Click += (s, e) => SelectFilter("Акт медицинского освидетельствования");
        btn_filter_forensic.Click += (s, e) => SelectFilter("Судебно-медицинская экспертиза");
        btn_filter_resolution.Click += (s, e) => SelectFilter("Постановление");
        
        ConfigureFiltersByRole();
        UpdateFilterButtonsUI("Все");
    }
    
    // Выбор фильтра (только меняет тип, НЕ вызывает поиск)
    private void SelectFilter(string filterType)
    {
        _selectedFilterType = filterType;
        UpdateFilterButtonsUI(filterType);
    }
    
    // Обновление UI кнопок фильтра
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

    // Загрузка из БД
    private async Task LoadDocumentsAsync()
    {
        try
        {
            var docs = await _db.GetCitizenDocumentsAsync(_citizenId);
            _allDocuments = docs ?? new List<MyDocument>();
            
            if (_allDocuments.Count == 0)
            {
                NotificationsControl.ShowWarning("Нет документов", 
                    $"Для гражданина {_citizenFullName} не найдено документов");
            }
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"LoadDocumentsAsync: {ex.Message}");
        }
    }

    // Кнопка "Найти" — загружает и применяет фильтры
    private async void BtnSearch_Click(object? sender, RoutedEventArgs e)
    {
        _searchText = txt_search.Text?.Trim() ?? "";
        
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            NotificationsControl.ShowWarning("Введите номер документа", 
                "Для поиска необходимо ввести номер документа в текстовое поле");
            return;
        }
        
        await LoadDocumentsAsync();
        ApplyFilters();
    }

    // Фильтрация (по памяти, без БД)
    private void ApplyFilters()
    {
        if (_allDocuments.Count == 0)
        {
            documentsContainer.ItemsSource = null;
            txtNoDocuments.IsVisible = true;
            return;
        }
        
        var filtered = _allDocuments.AsEnumerable();
        
        // Фильтр по типу
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType);
        }
        
        // Фильтр по дате "от"
        if (dp_date_from.SelectedDate.HasValue)
        {
            var dateFrom = dp_date_from.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date >= dateFrom);
        }
        
        // Фильтр по дате "до"
        if (dp_date_to.SelectedDate.HasValue)
        {
            var dateTo = dp_date_to.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date <= dateTo);
        }
        
        // ✅ Текстовый поиск (без ошибки приведения типов)
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(d => 
                (d.Number?.ToString()?.Contains(_searchText) ?? false) ||
                (d.Content?.ToLower().Contains(_searchText.ToLower()) == true) ||
                (d.DocumentType?.ToLower().Contains(_searchText.ToLower()) == true)
            );
        }
        
        _currentDocuments = filtered.ToList();
        documentsContainer.ItemsSource = _currentDocuments;
        txtNoDocuments.IsVisible = _currentDocuments.Count == 0;
        
        SubscribeToButtons();
    }

    // Обработчик кнопки "Назад"
    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        var searchWindow = new SearchCitizensWindow(App.CurrentUserId);
        searchWindow.Show();
        this.Close();
    }

    // Подписка на кнопки "Открыть"
    private void SubscribeToButtons()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var buttons = documentsContainer.GetVisualDescendants()
                .OfType<Button>()
                .Where(b => b.Name == "OpenButton")
                .ToList();
            
            foreach (var button in buttons)
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
                
                var doc = button.DataContext as MyDocument;
                if (doc != null)
                {
                    button.Tag = doc;
                }
            }
        });
    }

    // Открытие документа
    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                
                this.Hide();
                
                var viewer = new DocumentViewerWindow(App.CurrentUserId, fullDoc, this);
                viewer.Show();
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
                this.Show();
            }
        }
    }

    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        // Скрываем все кнопки фильтров по умолчанию
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
            case UserRole.MedicalExpert:
                // Врач - Все, Направления, Акты
                btn_filter_all.IsVisible = true;
                btn_filter_reports.IsVisible = true;           // Направление на мед. освид.
                btn_filter_medical_cert.IsVisible = true;      // Акт медицинского освидетельствования
                break;
                
            case UserRole.ForensicExpert:
                // Судмедэксперт - Все и Экспертиза
                btn_filter_all.IsVisible = false;
                btn_filter_forensic.IsVisible = true;          // Судебно-медицинская экспертиза
                break;
                
            case UserRole.PoliceOfficer:
            case UserRole.Judge:
            case UserRole.ChiefOfPolice:
            case UserRole.AdminInspector:
            default:
                // Все остальные видят все типы документов
                btn_filter_all.IsVisible = true;
                btn_filter_appeals.IsVisible = true;
                btn_filter_statements.IsVisible = true;
                btn_filter_protocols.IsVisible = true;
                btn_filter_explanations.IsVisible = true;
                btn_filter_reports.IsVisible = true;
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                break;
        }
    }
}