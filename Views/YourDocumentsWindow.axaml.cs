using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Media;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class YourDocumentsWindow : Window
{
    private string _searchDealNumber = "";
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private List<MyDocument> _allDocuments = new();
    private List<MyDocument> _currentDocuments = new();
    
    private string _selectedFilterType = "Все";
    private string _searchText = "";

    public YourDocumentsWindow(int currentUserId) : this(currentUserId, null) { }
    
    public YourDocumentsWindow(int currentUserId, string? dealNumber)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _searchDealNumber = dealNumber ?? "";
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        _db = new DatabaseHelper();
        
        // ✅ Настройка кнопок фильтрации
        SetupFilterButtons();
        
        btn_search.Click += OnSearchClick;
        
        this.Opened += async (s, e) => 
        {
            await LoadDocumentsAsync();
            
            if (!string.IsNullOrWhiteSpace(_searchDealNumber))
            {
                txt_search.Text = _searchDealNumber;
                await PerformSearch();
            }
        };
    }
    
    // ✅ Настройка кнопок фильтрации
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
    
    // ✅ Выбор фильтра
    private void SelectFilter(string filterType)
    {
        _selectedFilterType = filterType;
        UpdateFilterButtonsUI(filterType);
        _ = PerformSearch();
    }
    
    // ✅ Обновление UI кнопок фильтра
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

    private async Task LoadDocumentsAsync()
    {
        try
        {
            _allDocuments = await _db.GetUserDocumentsAsync(_currentUserId);
            emptyStateBorder.IsVisible = false;
            documentsContainer.ItemsSource = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDocumentsAsync: {ex.Message}");
        }
    }

    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        await PerformSearch();
    }
    
    private async Task PerformSearch()
    {
        _searchText = txt_search.Text?.Trim() ?? "";
        
        var filtered = _allDocuments;
        
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType).ToList();
        }
        
        if (dp_date_from.SelectedDate.HasValue)
        {
            var dateFrom = dp_date_from.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date >= dateFrom).ToList();
        }
        
        if (dp_date_to.SelectedDate.HasValue)
        {
            var dateTo = dp_date_to.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date <= dateTo).ToList();
        }
        
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(d => 
                (d.Number?.ToString().Contains(_searchText) ?? false) ||
                d.CitizenFullName?.ToLower().Contains(_searchText.ToLower()) == true ||
                d.Content?.ToLower().Contains(_searchText.ToLower()) == true
            ).ToList();
        }
        
        _currentDocuments = filtered;
        
        documentsContainer.ItemsSource = _currentDocuments;
        emptyStateBorder.IsVisible = _currentDocuments.Count == 0;
        
        await Task.Delay(100);
        SubscribeToButtons();
    }
    
    // ✅ Подписка на кнопки "Открыть" и "Звездочка"
    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        foreach (var button in buttons)
        {
            if (button.Name == "FavoriteButton")
            {
                button.Click -= OnFavoriteClick;
                button.Click += OnFavoriteClick;
                
                // Обновляем иконку в зависимости от IsFavorite
                if (button.DataContext is MyDocument doc)
                {
                    button.Content = doc.IsFavorite ? "★" : "☆";
                    button.Foreground = new SolidColorBrush(Color.Parse("#FFB800"));
                }
            }
            else if (button.Name == "OpenButton")
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
            }
        }
    }
    
    // ✅ Обработчик избранного
    private async void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                await _db.ToggleFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                bool isFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                
                button.Content = isFavorite ? "★" : "☆";
                button.Foreground = new SolidColorBrush(Color.Parse("#FFB800"));
                
                var existingDoc = _currentDocuments.FirstOrDefault(d => d.Id == doc.Id && d.TableName == doc.TableName);
                if (existingDoc != null)
                {
                    existingDoc.IsFavorite = isFavorite;
                }
                
                string message = isFavorite ? "Добавлено в избранное" : "Удалено из избранного";
                NotificationsControl.ShowSuccess(message, $"Документ {doc.DocumentType} №{doc.Number}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Toggle favorite: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", $"Не удалось изменить статус избранного: {ex.Message}");
            }
        }
    }
    
    // ✅ Обработчик открытия документа
    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                Console.WriteLine($"[OPEN] Открываем документ: {doc.DocumentType} #{doc.Number}");
                
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                new DocumentViewerWindow(_currentUserId, fullDoc, "YourDocuments").Show();
                Close();    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Open document: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", $"Не удалось открыть документ: {ex.Message}");
            }
        }
    }
}