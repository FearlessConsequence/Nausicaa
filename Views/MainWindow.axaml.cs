#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class MainWindow : Window
{
    private readonly int _currentUserId;
    private readonly DatabaseHelper? _db;
    private List<RecentDocument> _recentDocuments = new();
    private List<Draft> _drafts = new();
    public MainWindow()
    {
        InitializeComponent();
    } 
    public MainWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        WindowState = WindowState.Maximized;
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_newAppel.Click += btn_newAppel_click;
        btn_newStatement.Click += btn_newStatement_click;
        btn_newExplanationProtocol.Click += btn_newExplanationProtocol_click;
        btn_newAdministrativeProtocol.Click += btn_newAdministrativeProtocol_click;
        btn_newExaminationReport.Click += btn_newExaminationReport_click;
        btn_searchMain.Click += btn_search_main_click;
        
        this.Opened += async (s, e) => 
        {
            await LoadRecentDocumentsAsync();
            await LoadDraftsAsync();
        };
    }

    private void btn_search_main_click(object? sender, RoutedEventArgs e)
    {
        string deal = txbx_deal.Text?.Trim() ?? "";
        string series = txbx_series_number.Text?.Trim() ?? "";
        string fio = txbx_fio.Text?.Trim() ?? "";
        string phone = txbx_phone_number.Text?.Trim() ?? "";
        string address = txbx_address.Text?.Trim() ?? "";
        
        // Если номер дела не пустой
        if (!string.IsNullOrWhiteSpace(deal))
        {
            var yourDocumentsWindow = new YourDocumentsWindow(_currentUserId, deal);
            yourDocumentsWindow.Show();
            Close();
            return;
        }
        
        // Если какие-то другие поля не пустые
        if (!string.IsNullOrWhiteSpace(series) || !string.IsNullOrWhiteSpace(fio) || 
            !string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(address))
        {
            var searchCitizensWindow = new SearchCitizensWindow(_currentUserId);
            searchCitizensWindow.Show();
            Close();
            return;
        }
        
        // Если всё пусто
        NotificationsControl.ShowWarning("Внимание", "Введите хотя бы один параметр для поиска");
    }
    
    private async Task LoadRecentDocumentsAsync()
    {
        try
        {
            if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
            _recentDocuments = await _db.GetAllDocumentsAsync();
            var recentDocs = _recentDocuments.Take(10).ToList();
            recentDocumentsList.ItemsSource = recentDocs;
            
            // ✅ Подписываемся на кнопки открытия
            await Task.Delay(100);
            SubscribeToRecentButtons();
            
            var txtNoRecent = this.FindControl<TextBlock>("txtNoRecent");
            if (txtNoRecent != null) txtNoRecent.IsVisible = recentDocs.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadRecentDocuments: {ex.Message}");
        }
    }
    
    private async Task LoadDraftsAsync()
    {
        try
        {
            if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
            _drafts = await _db.GetDraftsAsync(_currentUserId);
            var recentDrafts = _drafts.Take(10).ToList();
            draftsList.ItemsSource = recentDrafts;
            
            // ✅ Подписываемся на кнопки открытия
            await Task.Delay(100);
            SubscribeToDraftButtons();
            
            var txtNoDrafts = this.FindControl<TextBlock>("txtNoDrafts");
            if (txtNoDrafts != null) txtNoDrafts.IsVisible = recentDrafts.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDrafts: {ex.Message}");
        }
    }
    private void SubscribeToRecentButtons()
    {
        var buttons = recentDocumentsList.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btnOpenRecent")
            .ToList();
        
        foreach (var btn in buttons)
        {
            btn.Click -= OnRecentOpenClick;
            btn.Click += OnRecentOpenClick;
        }
    }

    private void SubscribeToDraftButtons()
    {
        var buttons = draftsList.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btnOpenDraft")
            .ToList();
        
        foreach (var btn in buttons)
        {
            btn.Click -= OnDraftOpenClick;
            btn.Click += OnDraftOpenClick;
        }
    }

    private async void OnRecentOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is RecentDocument doc)
        {
            // Получаем полный документ по ID и типу
            string tableName = doc.DocumentType switch
            {
                "Заявление" => "statement",
                "Обращение" => "appeals",
                "Протокол объяснения" => "explanation_protocol",
                "Направление на мед. освид." => "medical_examination_report",
                "Административный протокол" => "administrative_protocol",
                _ => "unknown"
            };
            
            if (tableName != "unknown")
            {
                if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
                var fullDoc = await _db.GetFullDocumentAsync(tableName, doc.Id);
                var viewer = new DocumentViewerWindow(_currentUserId, fullDoc);
                viewer.Show();
                this.Hide();
            }
        }
    }

    private async void OnDraftOpenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Draft draft)
        {
            Window? targetWindow = draft.DocumentType switch
            {
                "appeals" => new NewAppel(_currentUserId),
                "statement" => new NewStatement(_currentUserId),
                "explanation_protocol" => new NewExplanationProtocol(_currentUserId),
                "medical_examination_report" => new NewExaminationReport(_currentUserId),
                "administrative_protocol" => new NewAdministrativeProtocol(_currentUserId),
                _ => null
            };
            
            if (targetWindow != null)
            {
                // Загружаем черновик (если есть метод)
                if (targetWindow is NewAppel appel) await appel.LoadDraftAsync(draft);
                else if (targetWindow is NewStatement statement) await statement.LoadDraftAsync(draft);
                else if (targetWindow is NewExplanationProtocol exp) await exp.LoadDraftAsync(draft);
                else if (targetWindow is NewExaminationReport exam) await exam.LoadDraftAsync(draft);
                else if (targetWindow is NewAdministrativeProtocol admin) await admin.LoadDraftAsync(draft);
                
                targetWindow.Show();
                this.Close();
            }
        }
    }

    private void btn_newAppel_click(object? sender, RoutedEventArgs e)
    {
        var newAppel = new NewAppel(_currentUserId);
        newAppel.Show();
        this.Close();
    }
    
    private void btn_newStatement_click(object? sender, RoutedEventArgs e)
    {
        var newStatement = new NewStatement(_currentUserId);
        newStatement.Show();
        this.Close();
    }
    
    private void btn_newExplanationProtocol_click(object? sender, RoutedEventArgs e)
    {
        var newExplanationProtocol = new NewExplanationProtocol(_currentUserId);
        newExplanationProtocol.Show();
        this.Close();
    }
    
    private void btn_newAdministrativeProtocol_click(object? sender, RoutedEventArgs e)
    {
        var newAdministrativeProtocol = new NewAdministrativeProtocol(_currentUserId);
        newAdministrativeProtocol.Show();
        this.Close();
    }
    
    private void btn_newExaminationReport_click(object? sender, RoutedEventArgs e)
    {
        var newExaminationReport = new NewExaminationReport(_currentUserId);
        newExaminationReport.Show();
        this.Close();
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
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
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