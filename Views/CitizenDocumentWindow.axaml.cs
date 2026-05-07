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
    private readonly int _currentUserId;
    private readonly int _citizenId;
    private readonly string _citizenFullName;
    private readonly DatabaseHelper _db;
    
    private List<MyDocument> _allDocuments = new();
    private string _selectedFilterType = "Все";

    // Конструктор
    public CitizenDocumentsWindow(int currentUserId, int citizenId, string citizenFullName, Window? parent = null)
    {
        InitializeComponent();
        
        _currentUserId = currentUserId;
        _citizenId = citizenId;
        _citizenFullName = citizenFullName;
        _db = new DatabaseHelper();
        
        // Настройка левой панели
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        // Заголовки
        txtTitle.Text = "Документы гражданина";
        txtSubtitle.Text = citizenFullName;
        
        // Настройка кнопок фильтрации
        SetupFilterButtons();
        
        // Подписка на кнопки
        btnBack.Click += BtnBack_Click;
        btn_search.Click += BtnSearch_Click;
        
        // Подписка на фильтры даты (авто-применение)
        dp_date_from.SelectedDateChanged += (s, e) => ApplyFilters();
        dp_date_to.SelectedDateChanged += (s, e) => ApplyFilters();
        txt_search.TextChanged += (s, e) => ApplyFilters();
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
        
        UpdateFilterButtonsUI("Все");
    }
    
    // Выбор фильтра
    private void SelectFilter(string filterType)
    {
        _selectedFilterType = filterType;
        UpdateFilterButtonsUI(filterType);
        ApplyFilters();  // ✅ авто-применение фильтра
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
            {"Направление на мед. освид.", btn_filter_reports}
        };
        
        foreach (var btn in buttons)
        {
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

    // Загрузка из БД (тяжёлая)
    private async Task LoadDocumentsAsync()
    {
        try
        {
            var docs = await _db.GetCitizenDocumentsAsync(_citizenId);
            
            // ✅ Отладка через уведомление
            NotificationsControl.ShowInfo("Отладка", 
                $"CitizenId: {_citizenId}\n" +
                $"Документов получено: {docs?.Count ?? 0}\n" +
                $"Тип результата: {docs?.GetType()}");
            
            _allDocuments = docs ?? new List<MyDocument>();
            
            if (_allDocuments.Count == 0)
            {
                NotificationsControl.ShowWarning("Нет документов", 
                    $"Для гражданина ID {_citizenId} не найдено документов");
            }
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"LoadDocumentsAsync: {ex.Message}");
        }
    }

    // Фильтрация (лёгкая, по памяти)
    private void ApplyFilters()
    {
        if (_allDocuments.Count == 0) return;
        
        var filtered = _allDocuments;
        
        // Фильтр по типу
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType).ToList();
        }
        
        // Фильтр по дате "от"
        if (dp_date_from.SelectedDate.HasValue)
        {
            var dateFrom = dp_date_from.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date >= dateFrom).ToList();
        }
        
        // Фильтр по дате "до"
        if (dp_date_to.SelectedDate.HasValue)
        {
            var dateTo = dp_date_to.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date <= dateTo).ToList();
        }
        
        // Текстовый поиск
        var searchText = txt_search.Text?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(d => 
                (d.Number?.ToString().Contains(searchText) ?? false) ||
                d.Content?.ToLower().Contains(searchText.ToLower()) == true ||
                d.DocumentType?.ToLower().Contains(searchText.ToLower()) == true
            ).ToList();
        }
        
        documentsContainer.ItemsSource = filtered;
        txtNoDocuments.IsVisible = filtered.Count == 0;
        
        // Переподписываем кнопки после обновления списка
        Task.Delay(100).ContinueWith(_ => 
        {
            Dispatcher.UIThread.InvokeAsync(() => SubscribeToButtons());
        });
    }

    // Обработчик кнопки "Назад"
    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        var searchWindow = new SearchCitizensWindow(_currentUserId);
        searchWindow.Show();
        this.Close();
    }

    // Кнопка "Найти" — загружает из БД и применяет фильтры
    private async void BtnSearch_Click(object? sender, RoutedEventArgs e)
    {
        await LoadDocumentsAsync();
        ApplyFilters();
        
        if (_allDocuments.Count == 0)
        {
            txtNoDocuments.IsVisible = true;
        }
    }

    // Подписка на кнопки "Открыть"
    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "OpenButton")
            .ToList();
        
        foreach (var button in buttons)
        {
            button.Click -= OnOpenClick;
            button.Click += OnOpenClick;
        }
    }

    // Открытие документа
    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                var viewer = new DocumentViewerWindow(
                    _currentUserId, 
                    fullDoc, 
                    "CitizenDocuments",
                    _citizenId,
                    _citizenFullName);
                viewer.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Open document: {ex.Message}");
            }
        }
    }
}