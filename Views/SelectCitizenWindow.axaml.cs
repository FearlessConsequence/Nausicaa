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

public partial class SelectCitizenWindow : Window
{
    private readonly DatabaseHelper _db;
    private List<Citizen> _allCitizens = new();
    private readonly int _currentUserId = 0;
    private readonly Window? _previousWindow;
    public Citizen? SelectedCitizen { get; private set; }

    public SelectCitizenWindow(int currentUserId, Window? _previousWindow = null)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = currentUserId;
        _previousWindow = _previousWindow;
        
        btn_search.Click += OnSearchClick;
        btn_cancel.Click += (s, e) => Close();
        
        // НЕ загружаем автоматически, показываем пустой список
        citizensContainer.ItemsSource = null;
        emptyStateBorder.IsVisible = true;
    }
    
    private async void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        await PerformSearch();
    }
    
    private async Task PerformSearch()
    {
        try
        {
            txt_last_name_error.IsVisible = false;
            txt_first_name_error.IsVisible = false;
            txt_patronymic_error.IsVisible = false;
            
            string lastName = txt_last_name.Text?.Trim() ?? "";
            string firstName = txt_first_name.Text?.Trim() ?? "";
            string patronymic = txt_patronymic.Text?.Trim() ?? "";
            
            bool hasError = false;
            
            if (string.IsNullOrWhiteSpace(lastName))
            {
                txt_last_name_error.IsVisible = true;
                hasError = true;
            }
            
            if (string.IsNullOrWhiteSpace(firstName))
            {
                txt_first_name_error.IsVisible = true;
                hasError = true;
            }
            
            if (string.IsNullOrWhiteSpace(patronymic))
            {
                txt_patronymic_error.IsVisible = true;
                hasError = true;
            }
            
            if (hasError)
            {
                return;
            }
            
            var searchParams = new CitizenSearchParams
            {
                LastName = lastName,
                FirstName = firstName,
                Patronymic = patronymic,
                Birthday = dp_birthday.SelectedDate?.DateTime,
                Address = txt_address.Text?.Trim(),
                Phone = txt_phone.Text?.Trim(),
                Passport = txt_passport.Text?.Trim()
            };
            
            // Собираем полное ФИО для поиска
            if (!string.IsNullOrWhiteSpace(lastName) || !string.IsNullOrWhiteSpace(firstName))
            {
                searchParams.FullName = $"{lastName} {firstName} {patronymic}".Trim();
            }
            
            var results = await _db.SearchCitizensAsync(searchParams);
            
            citizensContainer.ItemsSource = results;
            emptyStateBorder.IsVisible = results.Count == 0;
            
            if (results.Count == 0)
            {
                NotificationsControl.ShowInfo("Не найдено", "Граждане по вашему запросу не найдены");
            }
            
            if (results.Count > 0)
            {
                await Task.Delay(100);
                SubscribeToButtons();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PerformSearch: {ex.Message}");
            NotificationsControl.ShowError("Ошибка", $"Ошибка при поиске: {ex.Message}");
        }
    }
    
    private void SubscribeToButtons()
    {
        var buttons = citizensContainer.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Content?.ToString()?.Contains("Выбрать") == true)
            .ToList();
        
        foreach (var button in buttons)
        {
            button.Click -= OnSelectClick;
            button.Click += OnSelectClick;
        }
        
        var cards = citizensContainer.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.BorderBrush?.ToString()?.Contains("#E9ECEF") == true)
            .ToList();
        
        foreach (var card in cards)
        {
            card.PointerPressed -= OnCardClick;
            card.PointerPressed += OnCardClick;
        }
    }
    
    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Citizen citizen)
        {
            SelectedCitizen = citizen;
            Close();
        }
    }
    
    private void OnCardClick(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Citizen citizen)
        {
            SelectedCitizen = citizen;
            Close();
        }
    }
}