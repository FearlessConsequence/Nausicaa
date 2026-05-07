using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class SelectDealWindow : Window
{
    private readonly DatabaseHelper _db;
    private List<Deal> _allDeals = new();
    public Deal? SelectedDeal { get; private set; }

    public SelectDealWindow()
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        
        btn_search.Click += Btn_search_Click;
        
    }

    private async Task LoadDealsAsync()
    {
        try
        {
            txt_debug.Text = "Поиск...";
            _allDeals = await _db.GetDealsAsync();
            txt_debug.Text = $"✓ Найдено: {_allDeals.Count} дел";
            ApplyFilter();
        }
        catch (Exception ex)
        {
            txt_debug.Text = $"✗ Ошибка: {ex.Message}";
        }
    }

    private async void Btn_search_Click(object? sender, RoutedEventArgs e) => await LoadDealsAsync();

    private void ApplyFilter()
    {
        var results = _allDeals.AsQueryable();

        if (!string.IsNullOrWhiteSpace(txt_deal_number.Text))
        {
            var search = txt_deal_number.Text.Trim();
            results = results.Where(d => d.Number.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(txt_fullname.Text))
        {
            var search = txt_fullname.Text.ToLower().Trim();
            results = results.Where(d => d.CitizenFullName.ToLower().Contains(search));
        }

        if (dp_date_from.SelectedDate.HasValue)
            results = results.Where(d => d.DealDate.Date >= dp_date_from.SelectedDate.Value.Date);
        
        if (dp_date_to.SelectedDate.HasValue)
            results = results.Where(d => d.DealDate.Date <= dp_date_to.SelectedDate.Value.Date);

        var filtered = results.ToList();
        
        dealsContainer.ItemsSource = filtered;
        emptyStateBorder.IsVisible = filtered.Count == 0;
    }

    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Deal deal)
        {
            SelectedDeal = deal;
            Close(deal);
        }
    }

    private void OnCardDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Deal deal)
        {
            SelectedDeal = deal;
            Close(deal);
        }
    }
}