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
        leftPanel?.SetUserId(_currentUserId);
        
        // Заголовки
        txtTitle.Text = "Документы гражданина";
        txtSubtitle.Text = citizenFullName;
        
        // Настройка кнопок фильтрации (только меняют UI, не вызывают поиск)
        SetupFilterButtons();
        
        // Подписка на кнопки
        btnBack.Click += BtnBack_Click;
        btn_search.Click += BtnSearch_Click;
        
        // ❌ Убрана авто-применение фильтров даты и текста
        // dp_date_from.SelectedDateChanged += (s, e) => ApplyFilters();
        // dp_date_to.SelectedDateChanged += (s, e) => ApplyFilters();
        // txt_search.TextChanged += (s, e) => ApplyFilters();
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
        
        // ✅ Проверка: текст должен быть обязательно
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            NotificationsControl.ShowWarning("Введите номер документа", "Для поиска необходимо ввести номер документа в текстовое поле");
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
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(d => 
                (d.Number?.ToString().Contains(_searchText) ?? false) ||
                d.Content?.ToLower().Contains(_searchText.ToLower()) == true ||
                d.DocumentType?.ToLower().Contains(_searchText.ToLower()) == true
            ).ToList();
        }
        
        _currentDocuments = filtered;
        documentsContainer.ItemsSource = _currentDocuments;
        txtNoDocuments.IsVisible = _currentDocuments.Count == 0;
        
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
        if (sender is Button button && button   .Tag is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                
                // ✅ ShowDialog блокирует родительское окно
                var viewer = new DocumentViewerWindow(_currentUserId, fullDoc);
                await viewer.ShowDialog(this);
                
                // ✅ После закрытия диалога управление возвращается сюда,
                // все поля (_citizenId, _citizenFullName) сохраняются
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }
}