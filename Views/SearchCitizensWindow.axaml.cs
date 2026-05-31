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
        leftPanel?.SetUserId(App.CurrentUserId, App.CurrentUserRole);
        
        btn_search.Click += OnSearchClick;
        
        emptyStateBorder.IsVisible = true;
        citizensContainer.IsVisible = false;
    }
    
    // ✅ Конструктор с предустановленными параметрами поиска
    public SearchCitizensWindow(int currentUserId, CitizenSearchParams? searchParams) : this(currentUserId)
    {
        if (searchParams != null)
        {
            // Заполняем поля формы
            if (!string.IsNullOrWhiteSpace(searchParams.Passport))
                txt_passport.Text = searchParams.Passport;
            
            if (!string.IsNullOrWhiteSpace(searchParams.Phone))
                txt_phone.Text = searchParams.Phone;
            
            if (!string.IsNullOrWhiteSpace(searchParams.Address))
                txt_address.Text = searchParams.Address;
            
            if (!string.IsNullOrWhiteSpace(searchParams.LastName))
                txt_last_name.Text = searchParams.LastName;
            
            if (!string.IsNullOrWhiteSpace(searchParams.FirstName))
                txt_first_name.Text = searchParams.FirstName;
            
            if (!string.IsNullOrWhiteSpace(searchParams.Patronymic))
                txt_patronymic.Text = searchParams.Patronymic;
            
            if (searchParams.Birthday.HasValue)
                dp_birthday.SelectedDate = searchParams.Birthday.Value;
            
            // ✅ Автоматически выполняем поиск при открытии окна
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
            string lastName = txt_last_name.Text?.Trim() ?? "";
            string firstName = txt_first_name.Text?.Trim() ?? "";
            string patronymic = txt_patronymic.Text?.Trim() ?? "";
            string passport = txt_passport.Text?.Trim() ?? "";
            string phone = txt_phone.Text?.Trim() ?? "";
            string address = txt_address.Text?.Trim() ?? "";
            
            // ✅ Приоритет: если есть паспорт - ищем только по паспорту
            if (!string.IsNullOrWhiteSpace(passport))
            {
                var searchParams = new CitizenSearchParams
                {
                    Passport = passport
                };
                
                var results = await _db.SearchCitizensAsync(searchParams);
                
                citizensContainer.ItemsSource = results;
                emptyStateBorder.IsVisible = results.Count == 0;
                citizensContainer.IsVisible = results.Count > 0;
                
                if (results.Count == 0)
                {
                    NotificationsControl.ShowInfo("Не найдено", $"Гражданин с паспортом '{passport}' не найден");
                }
                else
                {
                    await Task.Delay(100);
                    SubscribeToButtons();
                }
                return;
            }
            
            // ✅ Если есть телефон - ищем по телефону
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var searchParams = new CitizenSearchParams
                {
                    Phone = phone
                };
                
                var results = await _db.SearchCitizensAsync(searchParams);
                
                citizensContainer.ItemsSource = results;
                emptyStateBorder.IsVisible = results.Count == 0;
                citizensContainer.IsVisible = results.Count > 0;
                
                if (results.Count == 0)
                {
                    NotificationsControl.ShowInfo("Не найдено", $"Гражданин с телефоном '{phone}' не найден");
                }
                else
                {
                    await Task.Delay(100);
                    SubscribeToButtons();
                }
                return;
            }
            
            // ✅ Если есть адрес - ищем по адресу
            if (!string.IsNullOrWhiteSpace(address))
            {
                var searchParams = new CitizenSearchParams
                {
                    Address = address
                };
                
                var results = await _db.SearchCitizensAsync(searchParams);
                
                citizensContainer.ItemsSource = results;
                emptyStateBorder.IsVisible = results.Count == 0;
                citizensContainer.IsVisible = results.Count > 0;
                
                if (results.Count == 0)
                {
                    NotificationsControl.ShowInfo("Не найдено", $"Гражданин с адресом '{address}' не найден");
                }
                else
                {
                    await Task.Delay(100);
                    SubscribeToButtons();
                }
                return;
            }
            
            // ✅ Если есть ФИО - проверяем фамилию (обязательна)
            if (string.IsNullOrWhiteSpace(lastName))
            {
                NotificationsControl.ShowWarning("Введите фамилию", 
                    "Для поиска гражданина укажите фамилию, паспорт, телефон или адрес");
                return;
            }
            
            var searchParamsFio = new CitizenSearchParams
            {
                LastName = lastName,
                FirstName = firstName,
                Patronymic = patronymic,
                Birthday = dp_birthday.SelectedDate?.DateTime,
                Address = address,
                Phone = phone,
                Passport = passport
            };
            
            // Собираем полное ФИО для поиска
            searchParamsFio.FullName = $"{lastName} {firstName} {patronymic}".Trim();
            
            var resultsFio = await _db.SearchCitizensAsync(searchParamsFio);
            
            citizensContainer.ItemsSource = resultsFio;
            emptyStateBorder.IsVisible = resultsFio.Count == 0;
            citizensContainer.IsVisible = resultsFio.Count > 0;
            
            if (resultsFio.Count > 0)
            {
                await Task.Delay(100);
                SubscribeToButtons();
            }
            else
            {
                NotificationsControl.ShowInfo("Не найдено", $"Граждане с фамилией '{lastName}' не найдены");
            }
        }
        catch (Exception ex)
        {
            NotificationsControl.ShowError("Ошибка", $"Ошибка при поиске: {ex.Message}");
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
                button.Click -= OnViewCardClick;
                button.Click += OnViewCardClick;
            }
        }
    }
    
    private async void OnViewCardClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Citizen citizen)
        {
            try
            {
                var fullCitizen = await _db.GetCitizenByIdAsync(citizen.Id);
                if (fullCitizen != null)
                {
                    var cardWindow = new CitizenCardWindow(App.CurrentUserId, fullCitizen);
                    await cardWindow.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                NotificationsControl.ShowError("Ошибка", $"Ошибка при открытии карточки: {ex.Message}");
            }
        }
    }
    
    public void SetCitizensWindow(Window citizensWindow)
    {
        _citizensWindow = citizensWindow;
    }
}