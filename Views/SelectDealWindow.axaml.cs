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

    public SelectDealWindow(int currentUserId, Window? previousWindow = null)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = currentUserId;
        _previousWindow = previousWindow;
        
        btn_search.Click += Btn_search_Click;
        btn_back.Click += (s, e) => Close();
    }

    private async void Btn_search_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string dealNumber = txt_deal_number.Text?.Trim() ?? "";
            
            txt_error.IsVisible = false;
            txt_error.Text = "";
            
            if (string.IsNullOrWhiteSpace(dealNumber))
            {
                txt_error.Text = "Введите номер дела";
                txt_error.IsVisible = true;
                return;
            }
            
            if (_db == null) return;
            
            
            _allDeals = await _db.GetDealsByUserAsync(_currentUserId);
            
            
            var filtered = _allDeals.Where(d => d.Number.Contains(dealNumber)).ToList();
            
            
            dealsContainer.ItemsSource = filtered;
            emptyStateBorder.IsVisible = filtered.Count == 0;
            
            if (filtered.Count == 0)
            {
                txt_error.Text = $"Дело с номером '{dealNumber}' не найдено";
                txt_error.IsVisible = true;
                NotificationsControl.ShowInfo("Не найдено", $"Дело с номером '{dealNumber}' не найдено");
            }
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", ex.Message);
        }
    }

    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.Tag is Deal deal)
            {
                Console.WriteLine($"[DEBUG] SelectDealWindow: Выбрано дело ID={deal.Id}, Номер={deal.Number}");
                SelectedDeal = deal;
                Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] SelectDealWindow.OnSelectClick: {ex.Message}");
        }
    }

    private void OnCardDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        try
        {
            if (sender is Border border && border.DataContext is Deal deal)
            {
                Console.WriteLine($"[DEBUG] SelectDealWindow: Двойной клик по делу ID={deal.Id}, Номер={deal.Number}");
                SelectedDeal = deal;
                Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] SelectDealWindow.OnCardDoubleTapped: {ex.Message}");
        }
    }
}