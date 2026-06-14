using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using CourseWork.Controls;
using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Views;

public partial class SelectOfficerWindow : Window
{
    private readonly DatabaseHelper _db;
    public UserWithRole? SelectedOfficer { get; private set; }
    private List<UserWithRole> _allOfficers = new();
    private readonly int _currentUserId;

    public SelectOfficerWindow(int currentUserId)
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        _currentUserId = currentUserId;
        
        btn_search.Click += Btn_search_Click;
        btn_cancel.Click += Btn_cancel_Click;
        
        // Подписываемся на событие загрузки контейнера, чтобы добавить обработчики кнопок
        officersContainer.ItemTemplate = (DataTemplate)officersContainer.ItemTemplate;
        
        Loaded += async (s, e) => await LoadAllOfficers();
    }

    private async Task LoadAllOfficers()
    {
        try
        {
            _allOfficers = await _db.GetOfficersAsync(_currentUserId);
            officersContainer.ItemsSource = _allOfficers;
            emptyStateBorder.IsVisible = _allOfficers.Count == 0;
            
            // После загрузки данных подписываем кнопки
            await Task.Delay(100);
            SubscribeToButtons();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
            NotificationsControl.ShowError("Ошибка", $"Не удалось загрузить сотрудников: {ex.Message}");
        }
    }
    
    private void SubscribeToButtons()
    {
        // Находим все кнопки "Выбрать" в контейнере
        var buttons = officersContainer.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Name == "btn_select")
            .ToList();
        
        Console.WriteLine($"[DEBUG] Найдено кнопок: {buttons.Count}");
        
        foreach (var button in buttons)
        {
            button.Click -= OnSelectButtonClick;
            button.Click += OnSelectButtonClick;
            
            // Убеждаемся, что Tag установлен
            var officer = button.DataContext as UserWithRole;
            if (officer != null)
            {
                button.Tag = officer;
                Console.WriteLine($"[DEBUG] Кнопка привязана к сотруднику: {officer.FullName}");
            }
        }
    }
    
    private void Btn_search_Click(object? sender, RoutedEventArgs e)
    {
        var lastName = txt_last_name.Text?.Trim();
        var firstName = txt_first_name.Text?.Trim();
        var patronymic = txt_patronymic.Text?.Trim();
        var ageRange = txt_age.Text?.Trim();
        var rank = txt_rank.Text?.Trim();
        
        var filtered = _allOfficers.AsEnumerable();
        
        // Фильтр по ФИО
        if (!string.IsNullOrEmpty(lastName))
        {
            filtered = filtered.Where(o => o.LastName != null && 
                o.LastName.Contains(lastName, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(firstName))
        {
            filtered = filtered.Where(o => o.FirstName != null && 
                o.FirstName.Contains(firstName, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(patronymic))
        {
            filtered = filtered.Where(o => o.Patronymic != null && 
                o.Patronymic.Contains(patronymic, StringComparison.OrdinalIgnoreCase));
        }
        
        // Фильтр по возрасту
        if (!string.IsNullOrEmpty(ageRange))
        {
            filtered = FilterByAge(filtered, ageRange);
        }
        
        // Фильтр по званию
        if (!string.IsNullOrEmpty(rank))
        {
            filtered = filtered.Where(o => o.Rank != null && 
                o.Rank.Contains(rank, StringComparison.OrdinalIgnoreCase));
        }
        
        var resultList = filtered.ToList();
        officersContainer.ItemsSource = resultList;
        emptyStateBorder.IsVisible = resultList.Count == 0;
        
        // После обновления списка - переподписываем кнопки
        if (resultList.Count > 0)
        {
            SubscribeToButtons();
        }
        else
        {
            NotificationsControl.ShowWarning("Результаты поиска", "Сотрудники не найдены");
        }
    }

    private IEnumerable<UserWithRole> FilterByAge(IEnumerable<UserWithRole> source, string ageRange)
    {
        ageRange = ageRange.Replace(" ", "");
        
        // Если просто число - ищем младше
        if (int.TryParse(ageRange, out int exactAge))
        {
            return source.Where(o => o.Age.HasValue && o.Age < exactAge);
        }
        else if (ageRange.Contains('-'))
        {
            var parts = ageRange.Split('-');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int minAge) && 
                int.TryParse(parts[1], out int maxAge))
            {
                return source.Where(o => o.Age.HasValue && o.Age >= minAge && o.Age <= maxAge);
            }
        }
        else if (ageRange.StartsWith('>'))
        {
            if (int.TryParse(ageRange.Substring(1), out int minAge))
            {
                return source.Where(o => o.Age.HasValue && o.Age > minAge);
            }
        }
        else if (ageRange.StartsWith('<'))
        {
            if (int.TryParse(ageRange.Substring(1), out int maxAge))
            {
                return source.Where(o => o.Age.HasValue && o.Age < maxAge);
            }
        }
        
        return source;
    }
    
    private void Btn_cancel_Click(object? sender, RoutedEventArgs e)
    {
        SelectedOfficer = null;
        Close();
    }
    
    private void SelectOfficer(UserWithRole officer)
    {
        Console.WriteLine($"[DEBUG] Выбран сотрудник: {officer.FullName}, ID={officer.Id}");
        SelectedOfficer = officer;
        Close();
    }
    
    private void OnSelectButtonClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine($"[DEBUG] Нажата кнопка Выбрать");
        
        if (sender is Button button)
        {
            // Пробуем взять Tag или DataContext
            var officer = button.Tag as UserWithRole ?? button.DataContext as UserWithRole;
            
            if (officer != null)
            {
                Console.WriteLine($"[DEBUG] Выбран сотрудник из кнопки: {officer.FullName}");
                SelectOfficer(officer);
            }
            else
            {
                Console.WriteLine($"[DEBUG] Не удалось получить сотрудника из кнопки");
                NotificationsControl.ShowError("Ошибка", "Не удалось определить сотрудника");
            }
        }
    }
}