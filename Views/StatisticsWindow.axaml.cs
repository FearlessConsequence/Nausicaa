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

public partial class StatisticsWindow : Window
{
    private readonly DatabaseHelper _db;
    private int? _selectedOfficerId;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;

    public StatisticsWindow()
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        btn_apply_filters.Click += Btn_apply_filters_Click;
        btn_select_officer.Click += Btn_select_officer_Click;
        btn_clear_filters.Click += Btn_clear_filters_Click;
        
        this.Opened += async (s, e) => await LoadStatistics();
    }
    
    private async Task LoadStatistics()
    {
        try
        {
            var allDeals = await _db.GetFilteredDealsAsync(null, null, null, null);
            
            txt_total_deals.Text = allDeals.Count.ToString();
            int completedCount = allDeals.Count(d => d.HasResolution);
            txt_completed.Text = completedCount.ToString();
            txt_in_progress.Text = (allDeals.Count - completedCount).ToString();
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Не удалось загрузить общую статистику: {ex.Message}");
        }
    }
    
    private async void Btn_apply_filters_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _dateFrom = dp_date_from.SelectedDate?.DateTime;
            _dateTo = dp_date_to.SelectedDate?.DateTime;
            string articleNumber = txt_article.Text?.Trim();
            
            var deals = await _db.GetFilteredDealsAsync(_dateFrom, _dateTo, _selectedOfficerId, articleNumber);
            
            dealsList.ItemsSource = deals;
            int count = deals.Count;
            
            // ✅ Обновляем общую статистику (тоже с учётом фильтров)
            txt_total_deals.Text = count.ToString();
            int completedCount = deals.Count(d => d.HasResolution);
            txt_completed.Text = completedCount.ToString();
            txt_in_progress.Text = (count - completedCount).ToString();
            
            filterResultBorder.IsVisible = true;
            txt_filter_result.Text = count.ToString();
            dealsList.IsVisible = deals.Count > 0;
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", ex.Message);
        }
    }
    
    private async void Btn_select_officer_Click(object? sender, RoutedEventArgs e)
    {
        var officerWindow = new SelectOfficerWindow(App.CurrentUserId);
        officerWindow.Closed += (s, args) =>
        {
            if (officerWindow.SelectedOfficer != null)
            {
                _selectedOfficerId = officerWindow.SelectedOfficer.Id;
                txt_officer.Text = officerWindow.SelectedOfficer.FullName;
            }
        };
        await officerWindow.ShowDialog(this);
    }
    
    private async void Btn_clear_filters_Click(object? sender, RoutedEventArgs e)
    {
        dp_date_from.SelectedDate = null;
        dp_date_to.SelectedDate = null;
        _selectedOfficerId = null;
        txt_officer.Text = "";
        txt_article.Text = "";
        filterResultBorder.IsVisible = false;
        dealsList.ItemsSource = null;
        
        await LoadStatistics();
    }
}