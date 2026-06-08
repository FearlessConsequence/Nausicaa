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
    private string _selectedFilterType = "Все";

    public FavouritesWindow() : this(0) { }

    public FavouritesWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
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
        
        // Скрываем ненужные кнопки по роли
        ConfigureFiltersByRole();
        
        btn_goToRecents.Click += Btn_goToRecents_Click;
        
        this.Opened += async (s, e) => await LoadFavoritesAsync();
    }

    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        // Сначала показываем все кнопки
        btn_filter_all.IsVisible = true;
        btn_filter_appeals.IsVisible = true;
        btn_filter_statements.IsVisible = true;
        btn_filter_protocols.IsVisible = true;
        btn_filter_explanations.IsVisible = true;
        btn_filter_reports.IsVisible = true;
        btn_filter_medical_cert.IsVisible = true;
        btn_filter_forensic.IsVisible = true;
        btn_filter_resolution.IsVisible = true;
        
        switch (role)
        {
            case UserRole.MedicalExpert:
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_forensic.IsVisible = false;
                btn_filter_resolution.IsVisible = false;
                break;
                
            case UserRole.ForensicExpert:
                btn_filter_all.IsVisible = false;
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_reports.IsVisible = false;
                btn_filter_medical_cert.IsVisible = false;
                btn_filter_resolution.IsVisible = false;
                btn_filter_forensic.IsVisible = true;
                break;
                
            case UserRole.Judge:
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
                
            case UserRole.PoliceOfficer:
            case UserRole.ChiefOfPolice:
            case UserRole.AdminInspector:
            default:
                // Все кнопки видны
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
        
        // Фильтр по типу
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType);
        }
        
        documentsContainer.ItemsSource = filtered.ToList();
        emptyStateBorder.IsVisible = filtered.Count() == 0;
        
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
            "Акт медицинского освидетельствования" => "medical_examination_certificate",
            "Судебно-медицинская экспертиза" => "forensic_medical_examination",
            "Постановление" => "resolution",
            "Дело" => "deal",
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
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                await _db.RemoveFromFavoritesAsync(_currentUserId, doc.TableName, doc.Id);
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RemoveFromFavorites: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", $"Не удалось удалить из избранного: {ex.Message}");
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                var viewer = new DocumentViewerWindow(_currentUserId, fullDoc, this);
                viewer.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }
}