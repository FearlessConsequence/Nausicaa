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

public partial class SelectDealWindow : Window
{
    private readonly Window? _previousWindow;
    private readonly DatabaseHelper? _db;
    private readonly int _currentUserId;
    private List<Deal> _allDeals = new();
    public Deal? SelectedDeal { get; private set; }
    public SelectDealWindow()
    {
        InitializeComponent();
    }

    public SelectDealWindow(int currentUserId, Window? previousWindow = null)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        
        btn_search.Click += Btn_search_Click;
        
        btn_back.Click += (s, e) =>
        {
            _previousWindow?.Show();
            Close();
        };
    }

    private async Task LoadDealsAsync()
    {
        if (_db == null) return; var drafts = await _db.GetDraftsAsync(_currentUserId);
        _allDeals = await _db.GetDealsAsync();
        ApplyFilter();
    }

    private async void Btn_search_Click(object? sender, RoutedEventArgs e)
    {
        string dealNumber = txt_deal_number.Text?.Trim() ?? "";
        
        // Скрываем предыдущее сообщение
        txt_error.IsVisible = false;
        txt_error.Text = "";
        
        // ✅ Проверка: номер дела обязателен
        if (string.IsNullOrWhiteSpace(dealNumber))
        {
            txt_error.Text = "Введите номер дела";
            txt_error.IsVisible = true;
            return;
        }
        
        await LoadDealsAsync();
        
        // ✅ Проверка: есть ли результаты
        if (dealsContainer.ItemsSource is ICollection<Deal> list && list.Count == 0)
        {
            txt_error.Text = $"Дело с номером '{dealNumber}' не найдено";
            txt_error.IsVisible = true;
        }
    }

    private void ApplyFilter()
    {
        var results = _allDeals.AsQueryable();

        // ✅ Поиск только по номеру дела
        if (!string.IsNullOrWhiteSpace(txt_deal_number.Text))
        {
            var search = txt_deal_number.Text.Trim();
            results = results.Where(d => d.Number.Contains(search));
        }

        // ❌ Остальные фильтры убраны
        // if (!string.IsNullOrWhiteSpace(txt_fullname.Text))
        // {
        //     var search = txt_fullname.Text.ToLower().Trim();
        //     results = results.Where(d => d.CitizenFullName.ToLower().Contains(search));
        // }
        //
        // if (dp_date_from.SelectedDate.HasValue)
        //     results = results.Where(d => d.DealDate.Date >= dp_date_from.SelectedDate.Value.Date);
        //     
        // if (dp_date_to.SelectedDate.HasValue)
        //     results = results.Where(d => d.DealDate.Date <= dp_date_to.SelectedDate.Value.Date);

        var filtered = results.ToList();
        
        dealsContainer.ItemsSource = filtered;
        emptyStateBorder.IsVisible = filtered.Count == 0;
        
        if (filtered.Count == 0 && !string.IsNullOrWhiteSpace(txt_deal_number.Text))
        {
            NotificationsControl.ShowInfo("Не найдено", $"Дело с номером '{txt_deal_number.Text}' не найдено");
        }
    }

    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Deal deal)
        {
            SelectedDeal = deal;
            Close();
        }
    }

    private void OnCardDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Deal deal)
        {
            SelectedDeal = deal;
            Close();
        }
    }
}