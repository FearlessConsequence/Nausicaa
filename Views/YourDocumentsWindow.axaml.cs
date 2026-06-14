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
using Avalonia.Threading;

namespace CourseWork.Views;

public partial class YourDocumentsWindow : Window
{
    private readonly DatabaseHelper _db;
    private readonly int _currentUserId;
    private readonly Window? _previousWindow;
    private List<MyDocument> _allDocuments = new();
    private List<MyDocument> _filteredDocuments = new();
    
    private string _selectedFilterType = "Все";

    public YourDocumentsWindow(Window? previousWindow, int currentUserId, 
        string searchNumber = "", DateTime? date = null, 
        CitizenSearchParams? citizenParams = null, string documentType = "")
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        _db = new DatabaseHelper();

        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);

        SetupFilterButtons();
        ConfigureByRole();
        
        if (!string.IsNullOrWhiteSpace(searchNumber))
            txt_document_number.Text = searchNumber;
        
        if (date.HasValue)
        {
            dp_date_from.SelectedDate = date.Value;
            dp_date_to.SelectedDate = date.Value;
        }
        
        if (!string.IsNullOrWhiteSpace(documentType) && documentType != "Все" && documentType != "Любой")
            SelectFilter(documentType);
        
        if (citizenParams != null && !string.IsNullOrWhiteSpace(citizenParams.FullName))
            txt_citizen.Text = citizenParams.FullName;
        
        btn_search.Click += OnSearchClick;
        btn_select_citizen.Click += OnSelectCitizenClick;
        
        // this.Opened += async (s, e) => await LoadDocumentsAsync();
    }

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

    private void ConfigureByRole()
    {
        var role = App.CurrentUserRole;
        
        btn_filter_medical_cert.IsVisible = false;
        btn_filter_forensic.IsVisible = false;
        btn_filter_resolution.IsVisible = false;
        SelectCitizenPanel.IsVisible = false;
        
        switch (role)
        {
            case UserRole.PoliceOfficer:
            case UserRole.ChiefOfPolice:
            case UserRole.AdminInspector:
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

    private async Task LoadDocumentsAsync()
    {
        try
        {

            if (App.CurrentUserRole != UserRole.ChiefOfPolice)
            {
                _allDocuments = await _db.GetUserDocumentsAsync(_currentUserId, App.CurrentUserRole);
            }

            else
            {
                _allDocuments = await _db.GetChiefDocumentsAsync(_currentUserId);
            }
            
            
            var grouped = _allDocuments.GroupBy(d => d.DocumentType)
                .Select(g => $"{g.Key}: {g.Count()}");
            for (int i = 0; i < _allDocuments.Count; i++)
            {
                var doc = _allDocuments[i];
                doc.IsFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
            }
            
            _filteredDocuments = _allDocuments.ToList();
            documentsContainer.ItemsSource = _filteredDocuments;
            UpdateEmptyState();
            emptyStateBorder.IsVisible = _filteredDocuments.Count == 0;
            
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("ОШИБКА", $"LoadDocumentsAsync: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        if (_allDocuments.Count == 0)
        {
            documentsContainer.ItemsSource = null;
            emptyStateBorder.IsVisible = true;
            return;
        }
        
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
        
        string searchNumber = txt_document_number.Text?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(searchNumber))
        {
            filtered = filtered.Where(d => (d.Number?.ToString()?.Contains(searchNumber) ?? false));
        }
        
        string citizenName = txt_citizen.Text?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(citizenName))
        {
            filtered = filtered.Where(d => d.CitizenFullName?.ToLower().Contains(citizenName.ToLower()) == true);
        }
        
        _filteredDocuments = filtered.ToList();
        documentsContainer.ItemsSource = _filteredDocuments;
        emptyStateBorder.IsVisible = _filteredDocuments.Count == 0;
        
        UpdateEmptyState();
        SubscribeToButtons();
    }

    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        // Если документы ещё не загружены — загружаем
        if (_allDocuments.Count == 0)
        {
            await LoadDocumentsAsync();
        }
        
        // ✅ Проверка на пустой поиск
        if (string.IsNullOrWhiteSpace(txt_document_number.Text) && 
            string.IsNullOrWhiteSpace(txt_citizen.Text))
        {
            NotificationsControl.ShowWarning("Внимание", "Введите номер документа или выберите гражданина для поиска");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(txt_document_number.Text) && 
            string.IsNullOrWhiteSpace(txt_citizen.Text))
        {
            _filteredDocuments = _allDocuments.ToList();
            documentsContainer.ItemsSource = _filteredDocuments;
            emptyStateBorder.IsVisible = _filteredDocuments.Count == 0;
        }
        else
        {
            ApplyFilters();
        }
        
        SubscribeToButtons();
    }

    private void SubscribeToButtons()
    {
        Dispatcher.UIThread.Post(() =>
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
                            // ✅ Устанавливаем правильное отображение звезды
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
        });
    }

    private async void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var doc = button.Tag as MyDocument ?? button.DataContext as MyDocument;
            
            if (doc != null)
            {
                try
                {
                    await _db.ToggleFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                    bool isFavorite = await _db.IsFavoriteAsync(_currentUserId, doc.TableName, doc.Id);
                    
                    button.Content = isFavorite ? "★" : "☆";
                    button.Foreground = isFavorite 
                        ? new SolidColorBrush(Color.Parse("#FFB800")) 
                        : new SolidColorBrush(Color.Parse("#CCCCCC"));
                    
                    doc.IsFavorite = isFavorite;
                    
                    var existingDoc = _filteredDocuments.FirstOrDefault(d => d.Id == doc.Id && d.TableName == doc.TableName);
                    if (existingDoc != null)
                    {
                        existingDoc.IsFavorite = isFavorite;
                    }
                    
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

    private async void OnSelectCitizenClick(object? sender, RoutedEventArgs e)
    {
        var citizensWindow = new SelectCitizenWindow(_currentUserId, this);
        citizensWindow.Closed += (s, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var selectedCitizen = citizensWindow.SelectedCitizen;
                if (selectedCitizen != null)
                {
                    txt_citizen.Text = selectedCitizen.FullName;
                    ApplyFilters();
                }
                Activate();
            });
        };
        citizensWindow.Show();
    }

    private void UpdateEmptyState()
    {
        if (_allDocuments.Count == 0 && string.IsNullOrWhiteSpace(txt_document_number.Text) && string.IsNullOrWhiteSpace(txt_citizen.Text))
        {
            // Ничего не загружено и поиск не выполнялся
            emptyStateText.Text = "Введите номер документа и нажмите 'Найти'";
            emptyStateBorder.IsVisible = true;
            documentsContainer.IsVisible = false;
        }
        else if (_filteredDocuments.Count == 0)
        {
            // Результатов нет
            emptyStateText.Text = "Документы не найдены";
            emptyStateBorder.IsVisible = true;
            documentsContainer.IsVisible = false;
        }
        else
        {
            emptyStateBorder.IsVisible = false;
            documentsContainer.IsVisible = true;
        }
    }
}