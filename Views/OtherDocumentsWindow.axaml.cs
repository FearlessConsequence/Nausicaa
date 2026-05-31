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

public partial class OtherDocumentsWindow : Window
{
    private readonly DatabaseHelper? _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private List<ExternalDocument> _allDocuments = new();
    private List<ExternalDocument> _currentDocuments = new();
    
    private string _selectedFilterType = "Все";
    private CitizenSearchParams? _citizenSearchParams;

    public OtherDocumentsWindow()
    {
        InitializeComponent();
    }
    
    public OtherDocumentsWindow(int currentUserId, Window? previousWindow = null)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
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
        
        btn_search.Click += OnSearchClick;
        btn_select_citizen.Click += Btn_select_citizen_Click;
        btn_select_deal.Click += Btn_select_deal_Click;
        
        ConfigureFiltersByRole();
        
        // Показываем пустое состояние при открытии
        documentsContainer.ItemsSource = null;
        emptyStateBorder.IsVisible = true;
        var emptyText = this.FindControl<TextBlock>("emptyStateText");
        if (emptyText != null)
        {
            emptyText.Text = "Введите номер документа и нажмите 'Найти'";
        }
    }
    
    private void ConfigureFiltersByRole()
    {
        var role = App.CurrentUserRole;
        
        btn_filter_medical_cert.IsVisible = false;
        btn_filter_forensic.IsVisible = false;
        btn_filter_resolution.IsVisible = false;
        
        SelectCitizenPanel.IsVisible = false;
        SelectDealPanel.IsVisible = false;
        
        FilterTypeBorder.IsVisible = true;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
                btn_filter_medical_cert.IsVisible = true;
                btn_filter_forensic.IsVisible = true;
                btn_filter_resolution.IsVisible = true;
                SelectCitizenPanel.IsVisible = true;
                SelectDealPanel.IsVisible = true;
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
                SelectDealPanel.IsVisible = true;
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
    
    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        await PerformSearch();
    }
    
    private async Task PerformSearch()
    {
        try
        {
            string docNumber = txt_document_number.Text?.Trim() ?? "";
            
            // ✅ Проверка: номер документа обязателен
            if (string.IsNullOrWhiteSpace(docNumber))
            {
                NotificationsControl.ShowWarning("Введите номер документа", 
                    "Для поиска документов необходимо ввести номер документа");
                return;
            }
            
            if (_db == null) return;
            
            // ✅ Загружаем все внешние документы (чужие)
            _allDocuments = await _db.GetExternalDocumentsAsync(null, null);
            
            // ✅ Фильтруем по роли (оставляем только те типы, которые может видеть текущий пользователь)
            var role = App.CurrentUserRole;
            var filteredByRole = _allDocuments;
            
            switch (role)
            {
                case UserRole.MedicalExpert:
                    // Врач - только направления и акты
                    filteredByRole = _allDocuments.Where(d => 
                        d.DocumentType == "Направление на мед. освид." || 
                        d.DocumentType == "Акт медицинского освидетельствования"
                    ).ToList();
                    break;
                    
                case UserRole.ForensicExpert:
                    // Судмедэксперт - только экспертизы
                    filteredByRole = _allDocuments.Where(d => 
                        d.DocumentType == "Судебно-медицинская экспертиза"
                    ).ToList();
                    break;
                    
                case UserRole.PoliceOfficer:
                case UserRole.AdminInspector:
                case UserRole.Judge:
                default:
                    // Полицейский, инспектор, судья - все документы
                    filteredByRole = _allDocuments;
                    break;
            }
            
            var filtered = filteredByRole;
            
            // Фильтр по типу (если выбран конкретный тип)
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
            
            // Поиск по номеру документа (обязательно)
            filtered = filtered.Where(d => 
                d.Number?.Contains(docNumber) == true ||
                d.MaskedNumber?.Contains(docNumber) == true
            ).ToList();
            
            // Поиск по гражданину
            string citizenName = txt_citizen.Text?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(citizenName))
            {
                filtered = filtered.Where(d => 
                    d.CitizenFullName?.ToLower().Contains(citizenName.ToLower()) == true
                ).ToList();
            }
            
            // Поиск по делу
            string dealInfo = txt_deal.Text?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(dealInfo))
            {
                filtered = filtered.Where(d => 
                    d.DealInfo?.ToLower().Contains(dealInfo.ToLower()) == true
                ).ToList();
            }
            
            _currentDocuments = filtered;
            documentsContainer.ItemsSource = _currentDocuments;
            emptyStateBorder.IsVisible = _currentDocuments.Count == 0;
            
            if (_currentDocuments.Count == 0)
            {
                NotificationsControl.ShowInfo("Результаты поиска", 
                    $"Документы с номером '{docNumber}' не найдены");
            }
            
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PerformSearch: {ex.Message}");
            NotificationsControl.ShowError("Ошибка", ex.Message);
        }
    }
    
    private void SubscribeToButtons()
    {
        var buttons = documentsContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
            
        foreach (var button in buttons)
        {
            if (button.Name == "RequestButton")
            {
                button.Click -= OnRequestClick;
                button.Click += OnRequestClick;
            }
        }
    }
    
    private async void OnRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ExternalDocument doc)
        {
            var reasonWindow = new RequestReasonWindow();
            var reason = await reasonWindow.ShowDialog<string?>(this);
            
            if (!string.IsNullOrWhiteSpace(reason))
            {
                try
                {
                    if (_db == null) return;
                    await _db.SaveDocumentAccessRequestAsync(_currentUserId, doc.TableName, doc.Id, reason);
                    
                    NotificationsControl.ShowSuccess("Запрос отправлен", 
                        $"Запрос на документ {doc.DocumentType} №{doc.Number} отправлен");
                    
                    var fullDoc = await _db.GetFullDocumentAsync(doc.TableName, doc.Id);
                    var viewerWindow = new DocumentViewerWindow(_currentUserId, fullDoc, this);
                    viewerWindow.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    NotificationsControl.ShowError("Ошибка", ex.Message);
                }
            }
        }
    }
    
    private async void Btn_select_citizen_Click(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
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
    
    private async void Btn_select_deal_Click(object? sender, RoutedEventArgs e)
    {
        var dealWindow = new SelectDealWindow(_currentUserId, this);
        dealWindow.Closed += (s, args) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var selectedDeal = dealWindow.SelectedDeal;
                if (selectedDeal != null)
                {
                    txt_deal.Text = $"{selectedDeal.Number} - {selectedDeal.CitizenFullName}";
                    txt_deal.Tag = selectedDeal.Id;
                }
                Activate();
            });
        };
        await dealWindow.ShowDialog(this);
    }
}