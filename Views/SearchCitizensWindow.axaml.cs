using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class SearchCitizensWindow : Window
{
    private readonly DatabaseHelper _db;
    private Window? _citizensWindow;
    private readonly int _currentUserId;
     public Citizen? SelectedCitizen { get; private set; }
    public SearchCitizensWindow() : this(0) { }
    public SearchCitizensWindow(int currentUserId)
    {
        InitializeComponent();
        _currentUserId = currentUserId;
        _db = new DatabaseHelper();
        
        var leftPanel = this.FindControl<LeftPanel>("LeftPanelControl");
        leftPanel?.SetUserId(_currentUserId);
        
        btn_search.Click += OnSearchClick;
        
        emptyStateBorder.IsVisible = true;
        citizensContainer.IsVisible = false;
    }
    
    public SearchCitizensWindow(int currentUserId, CitizenSearchParams? searchParams) : this(currentUserId)
    {
        if (searchParams != null)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.Passport))
                txt_passport.Text = searchParams.Passport;
            
            if (!string.IsNullOrWhiteSpace(searchParams.Phone))
                txt_phone.Text = searchParams.Phone;
            
            if (!string.IsNullOrWhiteSpace(searchParams.Address))
                txt_address.Text = searchParams.Address;
            
            if (!string.IsNullOrWhiteSpace(searchParams.FullName))
            {
                var parts = searchParams.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) txt_last_name.Text = parts[0];
                if (parts.Length > 1) txt_first_name.Text = parts[1];
                if (parts.Length > 2) txt_patronymic.Text = parts[2];
            }
            
            this.Opened += async (s, e) => await PerformSearch();
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
            var searchParams = new CitizenSearchParams
            {
                LastName = txt_last_name.Text?.Trim(),
                FirstName = txt_first_name.Text?.Trim(),
                Patronymic = txt_patronymic.Text?.Trim(),
                Birthday = dp_birthday.SelectedDate?.DateTime,
                Address = txt_address.Text?.Trim(),
                Phone = txt_phone.Text?.Trim(),
                Passport = txt_passport.Text?.Trim()
            };
            
            // Собираем ФИО для поиска
            string lastName = searchParams.LastName ?? "";
            string firstName = searchParams.FirstName ?? "";
            string patronymic = searchParams.Patronymic ?? "";
            if (!string.IsNullOrWhiteSpace(lastName) || !string.IsNullOrWhiteSpace(firstName))
            {
                searchParams.FullName = $"{lastName} {firstName} {patronymic}".Trim();
            }
            
            // Проверяем, что хоть что-то введено
            if (string.IsNullOrWhiteSpace(searchParams.LastName) && 
                string.IsNullOrWhiteSpace(searchParams.FirstName) && 
                string.IsNullOrWhiteSpace(searchParams.Patronymic) &&
                !searchParams.Birthday.HasValue &&
                string.IsNullOrWhiteSpace(searchParams.Address) &&
                string.IsNullOrWhiteSpace(searchParams.Phone) &&
                string.IsNullOrWhiteSpace(searchParams.Passport))
            {
                NotificationsControl.ShowWarning("Внимание", "Введите хотя бы один параметр для поиска");
                return;
            }
            
            var results = await _db.SearchCitizensAsync(searchParams);
            
            citizensContainer.ItemsSource = results;
            emptyStateBorder.IsVisible = results.Count == 0;
            citizensContainer.IsVisible = results.Count > 0;
            
            if (results.Count > 0)
            {
                await Task.Delay(100);
                SubscribeToButtons();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PerformSearch: {ex.Message}");
            await ShowMessage("Ошибка", $"Ошибка при поиске: {ex.Message}");
        }
    }
    
    private void SubscribeToButtons()
    {
        var buttons = citizensContainer.GetVisualDescendants()
            .OfType<Button>()
            .ToList();
        
        foreach (var button in buttons)
        {
            if (button.Name == "btnViewCard")
            {
                button.Click += OnViewCardClick;
            }
        }
    }
    
    // В SearchCitizensWindow.cs, метод OnViewCardClick:
    private async void OnViewCardClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Citizen citizen)
        {
            try
            {
                // ✅ Загружаем полные данные с телефоном
                var fullCitizen = await _db.GetCitizenByIdAsync(citizen.Id);
                if (fullCitizen != null)
                {
                    var cardWindow = new CitizenCardWindow(_currentUserId, fullCitizen);
                    await cardWindow.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Ошибка при открытии карточки: {ex.Message}");
            }
        }
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

    public void SetCitizensWindow(Window citizensWindow)
    {
        _citizensWindow = citizensWindow;
    }
}