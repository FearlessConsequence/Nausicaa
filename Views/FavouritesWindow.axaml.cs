using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using CourseWork.Data;
using CourseWork.Controls;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class FavouritesWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private List<MyDocument> _allFavorites = new();
    private string _selectedFilterType = "Все";

    public FavouritesWindow() : this(0) { }

    public FavouritesWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        // Подписка на кнопки фильтров
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
        
        btn_goToRecents.Click += Btn_goToRecents_Click;
        
        this.Opened += async (s, e) => await LoadFavoritesAsync();
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
            case UserRole.PoliceOfficer:
            case UserRole.AdminInspector:
            case UserRole.ChiefOfPolice:
                // Полицейский, инспектор, начальник — видят все типы
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
                
            case UserRole.MedicalExpert:
                // Врач — только направления и акты
                btn_filter_all.IsVisible = true;
                btn_filter_reports.IsVisible = true;        // Направление на мед. освид.
                btn_filter_medical_cert.IsVisible = true;   // Акт мед. освид.
                break;
                
            case UserRole.Judge:
                // Судья — все типы
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
                
            case UserRole.ForensicExpert:
                // Судмедэксперт — только экспертизы
                btn_filter_all.IsVisible = true;
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

    private void Btn_goToRecents_Click(object? sender, RoutedEventArgs e)
    {
        new RecentsWindow(_currentUserId).Show();
        this.Close();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var rawFavorites = await _db.GetFavoriteDocumentsAsync(_currentUserId);
            
            if (rawFavorites.Count == 0)
            {
                _allFavorites.Clear();
                documentsContainer.ItemsSource = null;
                emptyStateBorder.IsVisible = true;
                return;
            }

            _allFavorites = rawFavorites.Select(f => new MyDocument
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadFavoritesAsync: {ex.Message}");
            emptyStateBorder.IsVisible = true;
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allFavorites.AsEnumerable();
        
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType);
        }
        
        var filteredList = filtered.ToList();
        documentsContainer.ItemsSource = filteredList;
        emptyStateBorder.IsVisible = filteredList.Count == 0;
        
        // Даём время на отрисовку
        Dispatcher.UIThread.Post(() => SubscribeToButtons(), DispatcherPriority.Render);
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
            "Акт медицинского освидетельствования" => "medical_examination_certificate",
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
                    button.Click -= OnRemoveFromFavorites;
                    button.Click += OnRemoveFromFavorites;
                    
                    var doc = button.DataContext as MyDocument;
                    if (doc != null)
                    {
                        button.Tag = doc;
                    }
                    
                    button.Content = "★";
                    button.Foreground = new SolidColorBrush(Color.Parse("#FFB800"));
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

    private async void OnRemoveFromFavorites(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Пробуем взять Tag, если нет - из DataContext
            var doc = button.Tag as MyDocument ?? button.DataContext as MyDocument;
            
            if (doc != null)
            {
                try
                {
                    await _db.RemoveFromFavoritesAsync(_currentUserId, doc.TableName, doc.Id);
                    
                    // Удаляем из локального списка
                    _allFavorites.RemoveAll(d => d.Id == doc.Id && d.TableName == doc.TableName);
                    
                    // Обновляем UI
                    ApplyFilters();
                    
                    NotificationsControl.ShowSuccess("Успех", "Документ удалён из избранного");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] RemoveFromFavorites: {ex.Message}");
                    NotificationsControl.ShowError("Ошибка", $"Не удалось удалить из избранного: {ex.Message}");
                }
            }
            else
            {
                NotificationsControl.ShowError("Ошибка", "Не удалось определить документ");
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var doc = button.Tag as MyDocument ?? button.DataContext as MyDocument;
            
            if (doc != null)
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
}