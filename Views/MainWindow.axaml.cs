using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class MainWindow : Window
{
    private readonly int _currentUserId;
    private readonly DatabaseHelper _db;
    private List<RecentDocument> _recentDocuments = new();
    private List<Draft> _drafts = new();
    
    public MainWindow() : this(0) { }
    
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
    
    private async Task LoadRecentDocumentsAsync()
    {
        try
        {
            _recentDocuments = await _db.GetAllDocumentsAsync();
            var recentDocs = _recentDocuments.Take(10).ToList();
            recentDocumentsList.ItemsSource = recentDocs;
            
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
            _drafts = await _db.GetDraftsAsync(_currentUserId);
            var recentDrafts = _drafts.Take(10).ToList();
            draftsList.ItemsSource = recentDrafts;
            
            var txtNoDrafts = this.FindControl<TextBlock>("txtNoDrafts");
            if (txtNoDrafts != null) txtNoDrafts.IsVisible = recentDrafts.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] LoadDrafts: {ex.Message}");
        }
    }
    
   private void btn_search_main_click(object? sender, RoutedEventArgs e)
    {
        var txb_series_number = this.FindControl<TextBox>("txb_series_number");
        var txb_fio_birth = this.FindControl<TextBox>("txb_fio_birth");
        var txbx_deal = this.FindControl<TextBox>("txbx_deal");
        var txb_phone = this.FindControl<TextBox>("txb_phone");
        var txb_address = this.FindControl<TextBox>("txb_address");
        
        string seriesNumber = txb_series_number?.Text?.Trim() ?? "";
        string fioBirth = txb_fio_birth?.Text?.Trim() ?? "";
        string dealNumber = txbx_deal?.Text?.Trim() ?? "";
        string phone = txb_phone?.Text?.Trim() ?? "";
        string address = txb_address?.Text?.Trim() ?? "";
        
        Console.WriteLine($"[DEBUG] Поиск: dealNumber={dealNumber}, seriesNumber={seriesNumber}, fioBirth={fioBirth}, phone={phone}, address={address}");
        
        if (!string.IsNullOrWhiteSpace(dealNumber))
        {
            Console.WriteLine($"[DEBUG] Переход в YourDocumentsWindow с dealNumber={dealNumber}");
            var yourDocumentsWindow = new YourDocumentsWindow(_currentUserId, dealNumber);
            yourDocumentsWindow.Show();
            this.Close();
        }
        else if (!string.IsNullOrWhiteSpace(seriesNumber) || 
                !string.IsNullOrWhiteSpace(fioBirth) || 
                !string.IsNullOrWhiteSpace(phone) || 
                !string.IsNullOrWhiteSpace(address))
        {
            Console.WriteLine($"[DEBUG] Переход в SearchCitizensWindow с параметрами");
            
            var searchParams = new CitizenSearchParams
            {
                Passport = seriesNumber,
                FullName = fioBirth,
                Phone = phone,
                Address = address
            };
            
            var citizensWindow = new SearchCitizensWindow(_currentUserId, searchParams);
            citizensWindow.Show();
            this.Close();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Пустой поиск");
            NotificationsControl.ShowWarning("Внимание", "Введите хотя бы один параметр для поиска");
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
}