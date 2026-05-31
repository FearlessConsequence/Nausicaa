using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CourseWork.Data;
using CourseWork.Controls;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class FavouritesWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private List<MyDocument> _allFavorites = new();
    private List<MyDocument> _currentFavorites = new();
    private string _selectedFilterType = "Все";

    public FavouritesWindow() : this(0) { }
    
    public FavouritesWindow(int currentUserId)
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
        
        btn_goToRecents.Click += btn_goToRecents_click;
        
        this.Opened += async (s, e) => await LoadFavoritesAsync();
    }
    
    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        btn_filter_medical_cert.IsVisible = false;
        btn_filter_forensic.IsVisible = false;
        btn_filter_resolution.IsVisible = false;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                break;
                
            case UserRole.MedicalExpert:
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_medical_cert.IsVisible = true;
                break;
                
            case UserRole.Judge:
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                btn_filter_all.IsVisible = false;
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_reports.IsVisible = false;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = false;
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

    private void btn_goToRecents_click(object? sender, RoutedEventArgs e)
    {
        var recentsWindow = new RecentsWindow(_currentUserId);
        recentsWindow.Show();
        this.Close();
    }   

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var allFavorites = await _db.GetFavoriteDocumentsAsync(_currentUserId);
            
            _allFavorites = allFavorites.Select(f => new MyDocument
            {
                Id = f.Id,
                DocumentType = f.DocumentType,
                TableName = GetTableName(f.DocumentType),
                Number = f.Number,
                CreatedAt = f.MakingDateAndTime,
                CitizenFullName = f.CitizenName ?? "Неизвестно",
                Content = "",
                IsFavorite = true 
            }).ToList();
            
            ApplyFilters();
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadFavoritesAsync: {ex.Message}");
            emptyStateBorder.IsVisible = true;
        }
    }
    
    private void ApplyFilters()
    {
        var filtered = _allFavorites;
        
        // Фильтр по типу
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType).ToList();
        }
        
        // Фильтр по дате от
        if (dp_date_from.SelectedDate.HasValue)
        {
            var dateFrom = dp_date_from.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date >= dateFrom).ToList();
        }
        
        // Фильтр по дате до
        if (dp_date_to.SelectedDate.HasValue)
        {
            var dateTo = dp_date_to.SelectedDate.Value.Date;
            filtered = filtered.Where(d => d.CreatedAt.Date <= dateTo).ToList();
        }
        
        _currentFavorites = filtered;
        documentsContainer.ItemsSource = _currentFavorites;
        emptyStateBorder.IsVisible = _currentFavorites.Count == 0;
        
        SubscribeToButtons();
    }

    private string GetTableName(string documentType)
    {
        return documentType switch
        {
            "Заявление" => "statement",
            "Обращение" => "appeals",
            "Протокол объяснения" => "explanation_protocol",
            "Направление на мед. освид." => "medical_examination_report",
            "Административный протокол" => "administrative_protocol",
            "Акт медицинского освидетельствования" => "medical_certificate",
            "Судебно-медицинская экспертиза" => "forensic_medical_examination",
            "Постановление" => "resolution",
            _ => "unknown"
        };
    }

    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        foreach (var button in buttons)
        {
            if (button.Name == "FavoriteButton")
            {
                button.Click -= OnRemoveFromFavorites;
                button.Click += OnRemoveFromFavorites;
                button.Content = "★";
                button.Foreground = new SolidColorBrush(Color.Parse("#FFB800"));
            }
            else if (button.Name == "OpenButton")
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
            }
        }
    }

    private async void OnRemoveFromFavorites(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MyDocument doc)
        {
            try
            {
                await _db.RemoveFromFavoritesAsync(_currentUserId, doc.TableName, doc.Id);
                await LoadFavoritesAsync();
                NotificationsControl.ShowSuccess("Избранное", "Документ удалён");
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                var viewerWindow = new DocumentViewerWindow(_currentUserId, fullDoc, this);
                viewerWindow.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Open document: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось открыть документ: {ex.Message}");
            }
        }
    }

    private async Task ShowMessage(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
                }
            }
        };
        
        var okButton = (dialog.Content as StackPanel)?.Children[1] as Button;
        if (okButton != null) okButton.Click += (s, args) => dialog.Close();
        
        await dialog.ShowDialog(this);
    }
}