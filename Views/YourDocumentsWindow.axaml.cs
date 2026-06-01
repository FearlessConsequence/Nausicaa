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
    private readonly DatabaseHelper? _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private List<MyDocument> _allDocuments = new();
    private List<MyDocument> _currentDocuments = new();
    
    private string _selectedFilterType = "Все";
    private CitizenSearchParams? _citizenSearchParams;

    public YourDocumentsWindow(Window? previousWindow, int currentUserId, string searchValue = "", 
    DateTime? date = null, CitizenSearchParams? citizenParams = null, string documentType = "")
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _citizenSearchParams = citizenParams;
        _previousWindow = previousWindow;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        _db = new DatabaseHelper();
        
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
        
        btn_search.Click += OnSearchClick;
        btn_select_citizen.Click += Btn_select_citizen_Click;
        
        ConfigureFiltersByRole();
        
        // Заполняем поле номера документа
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            txt_document_number.Text = searchValue;
        }
        
        // ✅ ПРИМЕНЯЕМ ТИП ДОКУМЕНТА ДЛЯ ВСЕХ РОЛЕЙ
        if (!string.IsNullOrWhiteSpace(documentType) && documentType != "Все" && documentType != "Любой")
        {
            SelectFilter(documentType);
        }
        
        // Для судьи - применяем тип документа из ComboBox (дублируем на всякий случай)
        if (App.CurrentUserRole == UserRole.Judge && !string.IsNullOrWhiteSpace(documentType) && documentType != "Все")
        {
            SelectFilter(documentType);
        }
        
        // Для врача - применяем тип документа
        if (App.CurrentUserRole == UserRole.MedicalExpert && !string.IsNullOrWhiteSpace(documentType) && documentType != "Все")
        {
            SelectFilter(documentType);
        }
        
        if (date.HasValue)
        {
            dp_date_from.SelectedDate = date.Value;
            dp_date_to.SelectedDate = date.Value;
        }
        
        if (_citizenSearchParams != null)
        {
            string citizenName = _citizenSearchParams.FullName ?? _citizenSearchParams.LastName ?? "";
            if (!string.IsNullOrWhiteSpace(citizenName))
            {
                txt_citizen.Text = citizenName;
            }
        }
        
        this.Opened += async (s, e) => 
        {
            await LoadDocumentsAsync();
        };
    }
    
    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        // Скрываем дополнительные кнопки по умолчанию
        btn_filter_medical_cert.IsVisible = false;
        btn_filter_forensic.IsVisible = false;
        btn_filter_resolution.IsVisible = false;
        
        // Скрываем панель выбора гражданина
        SelectCitizenPanel.IsVisible = false;
        
        // По умолчанию фильтр по типу виден
        FilterTypeBorder.IsVisible = true;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                SelectCitizenPanel.IsVisible = true;
                break;
                
            case UserRole.MedicalExpert:
                btn_filter_appeals.IsVisible = false;
                btn_filter_statements.IsVisible = false;
                btn_filter_protocols.IsVisible = false;
                btn_filter_explanations.IsVisible = false;
                btn_filter_medical_cert.IsVisible = true;
                SelectCitizenPanel.IsVisible = true;
                break;
                
            case UserRole.Judge:
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                SelectCitizenPanel.IsVisible = true;
                break;
                
            case UserRole.ForensicExpert:
                FilterTypeBorder.IsVisible = false;
                SelectCitizenPanel.IsVisible = true;
                break;
        }
    }
    
    private void SelectFilter(string filterType)
    {
        _selectedFilterType = filterType;
        UpdateFilterButtonsUI(filterType);
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
            if (_db == null) return;
            
            _allDocuments = await _db.GetUserDocumentsAsync(App.CurrentUserId);
            
            NotificationsControl.ShowInfo("Отладка", $"Загружено документов: {_allDocuments.Count}");
            
            // Показываем все документы
            documentsContainer.ItemsSource = _allDocuments;
            emptyStateBorder.IsVisible = _allDocuments.Count == 0;
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDocumentsAsync: {ex.Message}");
            NotificationsControl.ShowError("Ошибка загрузки", ex.Message);
        }
    }
    
    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        await PerformSearch();
    }
    
    private async Task PerformSearch()
    {
        // Получаем значения полей
        string docNumber = txt_document_number.Text?.Trim() ?? "";
        string citizenName = txt_citizen.Text?.Trim() ?? "";
        
        // Если ничего не введено - показываем все документы
        if (string.IsNullOrWhiteSpace(docNumber) && string.IsNullOrWhiteSpace(citizenName))
        {
            documentsContainer.ItemsSource = _allDocuments;
            emptyStateBorder.IsVisible = _allDocuments.Count == 0;
            NotificationsControl.ShowInfo("Поиск", "Введите номер документа для поиска");
            return;
        }
        
        var filtered = _allDocuments;
        
        // Фильтр по типу
        if (_selectedFilterType != "Все")
        {
            filtered = filtered.Where(d => d.DocumentType == _selectedFilterType).ToList();
        }
        
        // Фильтр по дате
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
        
        // Поиск по номеру документа
        if (!string.IsNullOrWhiteSpace(docNumber))
        {
            int beforeCount = filtered.Count();
            filtered = filtered.Where(d => 
                (d.Number?.ToString().Contains(docNumber) ?? false)
            ).ToList();
            NotificationsControl.ShowInfo("Отладка", $"Поиск по номеру '{docNumber}': было {beforeCount}, стало {filtered.Count()}");
        }
        
        // Поиск по гражданину
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            filtered = filtered.Where(d => 
                d.CitizenFullName?.ToLower().Contains(citizenName.ToLower()) == true
            ).ToList();
        }
        
        _currentDocuments = filtered.ToList();
        documentsContainer.ItemsSource = _currentDocuments;
        emptyStateBorder.IsVisible = _currentDocuments.Count == 0;
        
        if (_currentDocuments.Count == 0 && !string.IsNullOrWhiteSpace(docNumber))
        {
            NotificationsControl.ShowInfo("Результаты поиска", 
                $"Документы с номером '{docNumber}' не найдены");
        }
        
        await Task.Delay(100);
        SubscribeToButtons();
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
                button.Click -= OnFavoriteClick;
                button.Click += OnFavoriteClick;
                
                if (button.DataContext is MyDocument doc)
                {
                    button.Content = doc.IsFavorite ? "★" : "☆";
                    button.Foreground = doc.IsFavorite 
                        ? new SolidColorBrush(Color.Parse("#FFB800")) 
                        : new SolidColorBrush(Color.Parse("#CCCCCC"));
                }
            }
            else if (button.Name == "OpenButton")
            {
                button.Click -= OnOpenClick;
                button.Click += OnOpenClick;
            }
        }
    }
    
    private async void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                if (_db == null) return;
                await _db.ToggleFavoriteAsync(App.CurrentUserId, doc.TableName, doc.Id);
                bool isFavorite = await _db.IsFavoriteAsync(App.CurrentUserId, doc.TableName, doc.Id);
                
                button.Content = isFavorite ? "★" : "☆";
                button.Foreground = isFavorite 
                    ? new SolidColorBrush(Color.Parse("#FFB800")) 
                    : new SolidColorBrush(Color.Parse("#CCCCCC"));
                
                var existingDoc = _currentDocuments.FirstOrDefault(d => d.Id == doc.Id && d.TableName == doc.TableName);
                if (existingDoc != null)
                {
                    existingDoc.IsFavorite = isFavorite;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OnFavoriteClick: {ex.Message}");
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }
    
    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MyDocument doc)
        {
            try
            {
                if (_db == null) return;
                var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                var viewer = new DocumentViewerWindow(App.CurrentUserId, fullDoc, this);
                viewer.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", ex.Message);
            }
        }
    }
    
    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(App.CurrentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selectedCitizen = citizensWindow.SelectedCitizen;
                if (selectedCitizen != null)
                {
                    txt_citizen.Text = selectedCitizen.FullName;
                    txt_citizen.Tag = selectedCitizen.Id;
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }
}